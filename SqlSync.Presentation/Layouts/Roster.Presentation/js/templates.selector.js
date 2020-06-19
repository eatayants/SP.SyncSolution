(function () {

    Roster.Common.ListTemplatesAll('DataService.svc/ListTemplates', function (tmplColl) {
        var doc = Roster.CustomActions.getOriginalWindowObj().document;
        var selCtrl = doc.getElementById('templateSelector');
        for (var k = 0; k < tmplColl.length; k++) {
            var newTemplateOption = new Option(tmplColl[k].text, tmplColl[k].id);
            selCtrl.appendChild(newTemplateOption);
        }
    });

}());