function UpdateViewsList(e) {
    var listId = '00000000-0000-0000-0000-000000000000';
    var settingsDiv = document.getElementById('list_and_view_selector_block');
    var viewsDdlCtrl = GetElementByClassName(settingsDiv, "views_control");

    if (e !== null) {
        listId = e.options[e.selectedIndex].value;
    } else {
        var listsDdlCtrl = GetElementByClassName(settingsDiv, "lists_control");
        listId = listsDdlCtrl.options[listsDdlCtrl.selectedIndex].value;
    }

    // clear options
    var selectedViewId = viewsDdlCtrl.options[viewsDdlCtrl.selectedIndex].value;
    while(viewsDdlCtrl.options.length > 0){                
        viewsDdlCtrl.remove(0);
    }

    var listViewRelations = JSON.parse(viewsDdlCtrl.nextSibling.value);
    for (var i = 0; i < listViewRelations.length; i++) {
        if (listViewRelations[i].list == listId) {
            var views = listViewRelations[i].views;
            for (var j = 0; j < views.length; j++) {
                var _option = document.createElement('option');
                _option.value = views[j].viewId;
                _option.innerHTML = views[j].viewName;
                viewsDdlCtrl.appendChild(_option);
            }

            break;
        }
    }

    //restore selected View
    viewsDdlCtrl.value = selectedViewId;
}

function WaitUntilWPpropsLoaded() {
    var varCounter = 0;
    var myTimer = setInterval(function () {
        if (varCounter >= 10) {
            clearInterval(myTimer); // wait max 10sec.
        }

        if (document.getElementById('list_and_view_selector_block') != null) {
            UpdateViewsList(null);
            clearInterval(myTimer);
        }

        varCounter++;
    }, 1000);
}
WaitUntilWPpropsLoaded();