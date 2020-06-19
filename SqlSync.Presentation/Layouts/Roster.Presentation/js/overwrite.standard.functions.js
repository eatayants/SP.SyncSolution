SP.SOD.executeOrDelayUntilEventNotified(OverwriteInitFunction, "sp.bodyloaded");

function OverwriteInitFunction() {
    // register dblclick event for Week and Day calendar modes
    RegisterAddRosterEventOnDoubleClick();

    // re-color events
    ExecuteOrDelayUntilScriptLoaded(AddColorsToEvents, "sp.ui.applicationpages.calendar.js");

    // add tooltips
    ExecuteOrDelayUntilScriptLoaded(AddTooltipsToCalendarEvents, "sp.ui.applicationpages.calendar.js");

    // set DropDown Filter in case of BIG filters data
    ExecuteOrDelayUntilScriptLoaded(InitCustomFilter, "core.js");
}

function SPGridView_FilterCallbackHandler_Custom(result, context) {
    var values = context.split(';');

    if (values.length != 4) {
        alert("ERROR: SPGridView_FilterCallbackHandler() - values.length != 4");
        return;
    }
    var gridViewClientId = values[0];
    var templateClientId = values[1];
    var menuClientId = values[2];
    var dataFieldName = values[3];
    var gridView = document.getElementById(gridViewClientId);

    if (gridView == null) {
        alert("ERROR: SPGridView_FilterCallbackHandler() - gridView == null");
        return;
    }
    var menuTemplate = document.getElementById(templateClientId);

    if (menuTemplate == null) {
        alert("ERROR: SPGridView_FilterCallbackHandler() - menuTemplate == null");
        return;
    }
    var menu = document.getElementById(menuClientId);

    if (menu == null) {
        alert("ERROR: SPGridView_FilterCallbackHandler() - menu == null");
        return;
    }
    var postbackEventReference = gridView.getAttribute("postbackEventReference");

    if (postbackEventReference == null || postbackEventReference.length <= 0) {
        alert("ERROR: SPGridView_FilterCallbackHandler() - postbackEventReference is null or empty");
        return;
    }
    var filterFieldName = gridView.getAttribute("filterFieldName");
    var filterFieldValue = gridView.getAttribute("filterFieldValue");
    var filterCurrentlyOn = IsFilterByColumnExists(dataFieldName);

    SPGridView_FixupFilterValuesFromMenuTemplate(menuTemplate, filterCurrentlyOn);
    // hide sort option from menu if column does not support sorting
    HideSortOptionsAndUpdateClearFilterOption(menu, menuTemplate, dataFieldName); // CUSTOM :: hide sort for lookups

    // Add filter Opertor Row to menu
    var filterOperInfo = window.RosterContext.FilterOperators || '';
    if (filterOperInfo) {
        var filterOperArr = filterOperInfo.split(';#');
        for (var j = 0; j < filterOperArr.length; j += 2) {
            if (filterOperArr[j] == dataFieldName && (j + 1) < filterOperArr.length) {
                var _mItemText = '';
                var _oper = filterOperArr[j + 1].toLowerCase();
                switch (_oper) {
                    case 'and':
                        _mItemText = 'Matches all';
                        break;
                    case 'or':
                        _mItemText = 'Matches one';
                        break;
                    case 'not':
                        _mItemText = 'Does not match';
                        break;
                    default:
                        _mItemText = 'Matches one';
                        break;
                }

                var _menuItem = CAMOpt(menuTemplate, String.format("'{0}' operator is used!", _mItemText), '');
                _menuItem.setAttribute("enabled", false);
                _menuItem.setAttribute("enabledOverride", false);
                _menuItem.setAttribute("isFilterItem", true);
            }
        }
    }

    values = result.split(';');
    for (var valueIndex = 0; valueIndex < values.length; valueIndex++) {
        var value = unescape(values[valueIndex]);
        var valueParts = value.split('##');
        if (valueParts.length > 1) {
            // lookup filter values
            var script = "javascript:HandleMyFilter(event, '" + dataFieldName + "', '" + valueParts[0] + "')"; // set LookupId as postbackargument
            var newMenuItem = CAMOpt(menuTemplate, valueParts[1], script);                                     // set LookupValue as filter Title
        } else {
            // general filter
            var script = "javascript:HandleMyFilter(event, '" + dataFieldName + "', '" +
                (((valueParts[0].replace(/\\/g, "\\\\")).replace(/\'/g, "\\'")).replace(/%/g, "%25")).replace(/;/g, "%3b") + "')";
            var newMenuItem = CAMOpt(menuTemplate, value, script);
        }

        newMenuItem.setAttribute("isFilterItem", "true");
        if (IsFilterItemChecked(dataFieldName, valueParts[0])) {
            newMenuItem.setAttribute("checked", "true");
        }
    }
    HideMenu(menuTemplate);
    MMU_Open(menuTemplate, menu);
}
function HideSortOptionsAndUpdateClearFilterOption(menu, menuTemplate, dataFieldName) {

    var sortEnabled = menu.children.length ? "false" : "true";

    for (var menuChildIndex = 0; menuChildIndex < menuTemplate.childNodes.length; menuChildIndex++) {
        var menuChild = menuTemplate.childNodes[menuChildIndex];
        if (menuChild.nodeName != "#text") {
            var _iconsrc = menuChild.getAttribute("iconsrc");
            if (_iconsrc !== null && (_iconsrc.indexOf("sortazlang.gif") >= 0 || _iconsrc.indexOf("sortzalang.gif") >= 0)) {
                menuChild.setAttribute("enabled", sortEnabled);
                menuChild.setAttribute("enabledOverride", sortEnabled);
            } else if (menuChild.getAttribute("clearFilterItem") == "true") {
                menuChild.setAttribute("onmenuclick", "javascript:HandleMyFilter(event, '" + dataFieldName + "', 'clearFilterItem')");
            }
        }
    }
}
function HandleMyFilter(e, fieldName, filterValue) {
    var filterUrl = GetUrlForCurrentFilterOption(fieldName, filterValue);

    document.getElementById('hidFiltersHistory').value = 'filterAction';

    HandleFilter(e, filterUrl);
}
function GetUrlForCurrentFilterOption(fldName, fldVal) {
    var filterUrl = '?errorFilter=1';
    var strFilterField = escapeProperly(fldName);
    var strFilterValue = escapeProperly(fldVal);
    var strDocUrl = document.location.search || '?';

    var strFilteredValue;
    var filterNo = 0;
    var arrayField;
    var arrayValue;
    var renumNextFilters = false;
    var isFilterAlreadyInUrl = false;
    var isMultiValues = false;

    do {
        filterNo++;
        isMultiValues = false;

        arrayField = strDocUrl.match(new RegExp("FilterField" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayField))
            arrayField = strDocUrl.match(new RegExp("FilterFields" + String(filterNo) + "=[^&#]*"));
        arrayValue = strDocUrl.match(new RegExp("&FilterValue" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayValue)) {
            arrayValue = strDocUrl.match(new RegExp("&FilterValues" + String(filterNo) + "=[^&#]*"));
            isMultiValues = true;
        }
        if (arrayField != null && arrayValue != null) {
            strFilteredValue = getFilterValueFromUrl(arrayField.toString() + arrayValue.toString(), strFilterField);
            if (strFilteredValue != null) {
                isFilterAlreadyInUrl = true;
                var vals = (unescapeProperly(strFilteredValue)).split(';#');
                var fValIndex = Array.indexOf(vals, (fldVal.replace(/%25/g, "%")).replace(/%3b/g, ";"));
                var filterValueKey = isMultiValues ? ("FilterValues" + String(filterNo) + "=") : ("FilterValue" + String(filterNo) + "=")
                if ((fValIndex >= 0 && vals.length === 1) || (fldVal == 'clearFilterItem')) {
                    // totally remove filter by this field
                    filterUrl = strDocUrl.replace(arrayField.toString() + arrayValue.toString(), '');
                    renumNextFilters = true;
                } else if (fValIndex >= 0 && vals.length > 1) {
                    // remove option
                    vals.splice(fValIndex, 1);
                    filterUrl = strDocUrl.replace(filterValueKey + escapeProperly(strFilteredValue), filterValueKey + escapeProperly(vals.join(";#")));
                } else {
                    // append filter
                    filterUrl = strDocUrl.replace(filterValueKey + escapeProperly(strFilteredValue), filterValueKey + escapeProperly(strFilteredValue + ";#" + (fldVal.replace(/%25/g, "%")).replace(/%3b/g, ";")));
                }

                if (!renumNextFilters)
                    arrayField = null; // to stop looping if renum is not required
            } else if (renumNextFilters) {
                // renumerate filters after Removed one
                var oldFilter = arrayField.toString() + arrayValue.toString();
                var expr = new RegExp(filterNo + '=', 'g');
                var newFilter = oldFilter.replace(expr, (filterNo - 1) + '=');
                filterUrl = filterUrl.replace(oldFilter, newFilter);
            }
        }
    } while (null != arrayField);

    if (!isFilterAlreadyInUrl) {
        // add new filter to URL
        filterUrl = String.format("{0}{1}FilterFields{2}={3}&FilterValues{2}={4}", strDocUrl,
            (strDocUrl == '?' ? '' : '&'), filterNo++, strFilterField, strFilterValue);
    }

    return (filterUrl.replace(/\?&/g, '?')).replace(/&{2,}/g, '&');
}
function IsFilterItemChecked(fldName, fldVal) {
    var isChecked = false;

    var strFilterField = escapeProperly(fldName);
    var strDocUrl = document.location.search;

    var strFilteredValue;
    var filterNo = 0;
    var arrayField;
    var arrayValue;

    do {
        filterNo++;
        var multi = false;

        arrayField = strDocUrl.match(new RegExp("FilterField" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayField))
            arrayField = strDocUrl.match(new RegExp("FilterFields" + String(filterNo) + "=[^&#]*"));
        arrayValue = strDocUrl.match(new RegExp("&FilterValue" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayValue)) {
            arrayValue = strDocUrl.match(new RegExp("&FilterValues" + String(filterNo) + "=[^&#]*"));
        }
        if (arrayField != null && arrayValue != null) {
            strFilteredValue = getFilterValueFromUrl(arrayField.toString() + arrayValue.toString(), strFilterField);
            if (strFilteredValue != null) {
                var vals = (unescapeProperly(strFilteredValue)).split(';#');
                isChecked = (Array.indexOf(vals, (fldVal.replace(/%25/g, "%")).replace(/%3b/g, ";")) >= 0);
                arrayField = null; // to stop looping
            }
        }
    } while (null != arrayField);

    return isChecked;
}
function IsFilterByColumnExists(fldName) {
    return (GetFilterValueByColumn(fldName) != null);
}
function GetFilterValueByColumn(fldName) {
    var strFilterField = escapeProperly(fldName);
    var strDocUrl = document.location.search;

    var strFilteredValue;
    var filterNo = 0;
    var arrayField;
    var arrayValue;

    do {
        filterNo++;
        var multi = false;

        arrayField = strDocUrl.match(new RegExp("FilterField" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayField))
            arrayField = strDocUrl.match(new RegExp("FilterFields" + String(filterNo) + "=[^&#]*"));
        arrayValue = strDocUrl.match(new RegExp("&FilterValue" + String(filterNo) + "=[^&#]*"));
        if (!Boolean(arrayValue)) {
            arrayValue = strDocUrl.match(new RegExp("&FilterValues" + String(filterNo) + "=[^&#]*"));
        }
        if (arrayField != null && arrayValue != null) {
            strFilteredValue = getFilterValueFromUrl(arrayField.toString() + arrayValue.toString(), strFilterField);
            if (strFilteredValue != null) {
                arrayField = null; // to stop looping
            }
        }
    } while (null != arrayField);

    return strFilteredValue;
}

function InitCustomFilter() {

    (function () {
        var HideMenu_original = HideMenu;
        HideMenu = function () {
            // destroy select2 on menu hide
            try {
                var _selectCtrl = document.getElementById('filter_select_box');
                if (_selectCtrl) {
                    $j(_selectCtrl).select2("destroy");
                }
            } catch (e) { }

            // destroy datepicker on menu hide
            try {
                var _dtCtrl = document.getElementById('filter_datepicker');
                if (_dtCtrl) {
                    $j(_dtCtrl).datepicker("hide");
                    $j(_dtCtrl).datepicker("destroy");
                }
            } catch (e) { }

            HideMenu_original.apply(this, arguments);
        }
    })();

    (function () {
        var createMenuOption_original = CreateMenuOption;
        CreateMenuOption = function () {
            var view_all_filters_key = "viewAllFilters:";
            var view_datepicker_key = "filterBySingleDate";
            var _onMenuClick = '';
            var oNode = arguments[2];
            var sText = EvalAttributeValue(oNode, "text", "textScript");
            if (sText == null || sText == "") {
                var innerNode = oNode.firstChild;
                if (innerNode != null && innerNode.nodeType == 3)
                    sText = innerNode.nodeValue;
            }

            if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(sText) && sText.indexOf(view_all_filters_key) == 0) {
                var oSelect = document.createElement("select");
                oSelect.id = "filter_select_box";

                var oMenuitem = arguments[1];
                _onMenuClick = oMenuitem.getAttribute('onmenuclick');
                oMenuitem.setAttribute('onmenuclick', null);
                oMenuitem['onclick'] = function (e) { cancelDefault(e); }; // not to hide select2 on click
                SetIType(oMenuitem, "option");
                oMenuitem.appendChild(oSelect);

                // convert control to select2
                var filterParams = JSON.parse(sText.replace(view_all_filters_key, ''));

                Roster.Common.FilterInit(oSelect, filterParams, _onMenuClick.replace(sText, "__viewAllFilters__"));
            } else if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(sText) && sText.indexOf(view_datepicker_key) == 0) {
                var oPicker = document.createElement("input");
                oPicker.id = "filter_datepicker";
                oPicker.style.width = "220px";

                var oMenuitem = arguments[1];
                _onMenuClick = oMenuitem.getAttribute('onmenuclick');
                oMenuitem.setAttribute('onmenuclick', null);
                oMenuitem['onclick'] = function (e) { cancelDefault(e); };
                SetIType(oMenuitem, "option");
                oMenuitem.appendChild(oPicker);

                // convert control to DateTimePicker
                Roster.Common.DateFilterInit(oPicker, _onMenuClick);
            } else {
                createMenuOption_original.apply(this, arguments);
            }
        }
    })();

    // don not hide filter menu on mouse over and on click on picker elems
    $j(function () {
        try
        {
            $j.datepicker._updateDatepicker_original = $j.datepicker._updateDatepicker;
            $j.datepicker._updateDatepicker = function (inst) {
                $j.datepicker._updateDatepicker_original(inst);
                var afterShow = this._get(inst, 'afterShow');
                if (afterShow)
                    afterShow.apply((inst.input ? inst.input[0] : null));  // trigger custom callback
            }
        } catch (e) {
            if (window.console && window.console.log) { console.log(e); }
        }
    });
}


function RegisterAddRosterEventOnDoubleClick() {

    m$('tr.ms-acal-hour30 > td, tr.ms-acal-hour00 > td').dblclick(function (e) {
        var selectedCell = m$(this);
        if (selectedCell.tagName !== 'TD')
            selectedCell = selectedCell.closest('td');
        var closestRow = selectedCell.closest('tr');

        var time = undefined;
        var detailtime = closestRow.find('th.ms-acal-detailtime').find('span');
        if (detailtime.length) {
            time = detailtime[0].innerHTML;
        } else {
            // get Time value from previous table row
            time = m$(closestRow[0].previousSibling).find('th.ms-acal-detailtime').find('span')[0].innerHTML;
        }

        // get calendar view header row
        var date;
        var headerRow = selectedCell.closest('table').find('tr.ms-acal-week-top').first();
        if (headerRow.length) {
            // WEEK view mode
            // get cell index in parent row
            var columnIndex = 0;
            var siblinsTDs = closestRow.find('td');
            for (var i = 0; i < siblinsTDs.length; i++) {
                if (siblinsTDs[i] == selectedCell[0]) {
                    columnIndex = i;
                    break;
                }
            }

            // get cell by index from header row and extract date info
            var rowDate = m$(headerRow.find('td')[columnIndex]).attr('date');
            //date = Date._parseExact(rowDate, Sys.CultureInfo.InvariantCulture.dateTimeFormat.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
            date = Date._parseExact(rowDate, RosterContext.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
            if (date == null) {
                date = Date._parseExact(rowDate, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
            }
        } else {
            // DAY view mode
            var dayAsStr = GetInnerText(m$('div.ms-acal-header').find('span.ms-acal-display')[0]).trim();
            date = Date._parseExact(dayAsStr, 'dddd, MMMM d, yyyy', Sys.CultureInfo.InvariantCulture);
        }

        date.setHours(time);
        var endDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), (date.getHours() + 1), 0, 0, 0);
        var urlSuffix = String.format('&StartDate={0}&EndDate={1}',
            date.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z',
            endDate.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z');
        
        SP.UI.ModalDialog.ShowPopupDialog(String.format('{0}{1}&ListId={2}',
            window.RosterContext.listNewForm, urlSuffix, window.RosterContext.listId));
    });

    m$('tr.ms-acal-summary-itemrow div').dblclick(function (e) {
        // MONTH view mode
        var time = '12'; // default value for Month mode

        // try find clicked Date on previous row
        var selectedCell = m$(this);
        if (selectedCell.tagName !== 'TD')
            selectedCell = selectedCell.closest('td');
        var closestRow = selectedCell.closest('tr');

        // get cell index in parent row
        var columnIndex = 0;
        var siblinsTDs = closestRow.find('td');
        for (var i = 0; i < siblinsTDs.length; i++) {
            if (siblinsTDs[i] == selectedCell[0]) {
                columnIndex = i;
                break;
            }
        }

        var date = '';
        try {
            // get date from previous row
            date = m$(m$(closestRow[0].previousSibling).find('td')[columnIndex]).attr('date');
        } catch (e) { }

        //var dt = Date._parseExact(date, Sys.CultureInfo.InvariantCulture.dateTimeFormat.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
        var dt = Date._parseExact(date, RosterContext.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
        if (dt == null) {
            dt = Date._parseExact(date, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
        }
        dt.setHours(time);
        var endDate = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate(), (dt.getHours() + 1), 0, 0, 0);
        var urlSuffix = String.format('&StartDate={0}&EndDate={1}',
            dt.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z',
            endDate.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z');
        
        SP.UI.ModalDialog.ShowPopupDialog(String.format('{0}{1}&ListId={2}',
            window.RosterContext.listNewForm, urlSuffix, window.RosterContext.listId));
    });
}

function AddColorsToEvents() {

    var colorInfo = window.RosterContext.ColorSettings || '';
    if (colorInfo) {
        (function () {
            var onItemsSucceed_original = SP.UI.ApplicationPages.CalendarStateHandler.prototype.onItemsSucceed;
            SP.UI.ApplicationPages.CalendarStateHandler.prototype.onItemsSucceed = function () {
                var allShifts = arguments[0];
                var colorEnum = Enumerable.From(colorInfo);
                for (var i = 0; i < allShifts.length; i++) {
                    var evId = allShifts[i].$11_0;
                    var dotIndex = evId.indexOf('.');
                    if (dotIndex > 0) {
                        evId = evId.substr(0, dotIndex); // remove suffix from recurrent Instance events
                    }
                    var _colorSetElem = colorEnum.Where(function (x) { return x.ids.indexOf(evId) >= 0 }).FirstOrDefault();
                    if (_colorSetElem && _colorSetElem.color) {
                        allShifts[i].$N_0 = {
                            color: _colorSetElem.color.replace('#', ''),
                            disableDrag: true,
                            formUrl: allShifts[i].$N_0.formUrl,
                            id: allShifts[i].$N_0.id,
                            name: allShifts[i].$N_0.name,
                            primary: false
                        };
                    }
                }

                onItemsSucceed_original.apply(this, arguments);
            }
        })();
    }

}

function AddTooltipsToCalendarEvents() {

    if (window.RosterContext.WithTooltips) {
        // month Calendar view :: override 'renderGrids' function
        var proxiedRenderGridsSummary = SP.UI.ApplicationPages.SummaryCalendarView.prototype.renderGrids;
        SP.UI.ApplicationPages.SummaryCalendarView.prototype.renderGrids = function () {
            proxiedRenderGridsSummary.apply(this, arguments);

            AddHoverHandlerToEvents();
        }

        // day or week Calendar view :: override 'renderGrids' function
        var proxiedRenderGridsDetail = SP.UI.ApplicationPages.DetailCalendarView.prototype.renderGrids;
        SP.UI.ApplicationPages.DetailCalendarView.prototype.renderGrids = function () {
            proxiedRenderGridsDetail.apply(this, arguments);

            AddHoverHandlerToEvents();
        }
    }
}
function AddHoverHandlerToEvents() {
    try {
        var _eventsElems = m$('div.ms-acal-item'); // do we need mQuery??
        for (var i = 0; i < _eventsElems.length; i++) {
            ToolTips.ToolTipManager.attachToolTipRawAsync(_eventsElems[i]);
        }
    } catch (e) { }
}