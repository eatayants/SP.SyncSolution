if (typeof (Roster) == "undefined")
    Roster = {};

Roster.CustomActions = {
    FindRepOptions: {
        destinationURL: '',
        fieldsMapping: [],
        showInModal: false,
        dialogWidth: 800,
        dialogHeigth: 600,
        viewGuid: ''
    },
    doesUserCanSaveAsTemplate: undefined,
    publishingInProgress: false,
    isPageRefreshingNow: false,
    errorHandler: function (sender, args) {
        alert('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
    },
    getOriginalWindowObj: function () {
        return (window.location != window.parent.location) ? window.parent : window;
    }
};

Roster.CustomActions.FindReplacementWorker = function (destUrl, fldMapping, isModal, dWidth, dHeight) {
    Roster.CustomActions.FindRepOptions.destinationURL = destUrl;
    Roster.CustomActions.FindRepOptions.fieldsMapping = fldMapping ? fldMapping.split(';#') : [];
    Roster.CustomActions.FindRepOptions.showInModal = isModal;
    Roster.CustomActions.FindRepOptions.dialogWidth = dWidth;
    Roster.CustomActions.FindRepOptions.dialogHeigth = dHeight;

    var siteUrl = _spPageContextInfo.webServerRelativeUrl;
    if (siteUrl.substr(-1) === "/" && destUrl.indexOf("/") == 0) {
        siteUrl = siteUrl.slice(0, -1); // remove duplicate slashes
    }
    var srvRelativeUrl = String.format("{0}{1}", siteUrl, destUrl);

    this.context = new SP.ClientContext.get_current();
    var oFile = this.context.get_web().getFileByServerRelativeUrl(srvRelativeUrl);
    //get the limited web part manager for the page
    var limitedWebPartManager = oFile.getLimitedWebPartManager(SP.WebParts.PersonalizationScope.shared);
    //get the web part definitions on the current page
    this.collWebPart = limitedWebPartManager.get_webParts();

    // request the web part def collection and load it from the server
    this.context.load(this.collWebPart);
    this.context.executeQueryAsync(Function.createDelegate(this, function () {
        var wpDefCount = this.collWebPart.get_count();
        for (var x = 0; x < wpDefCount; x++) {
            // try to find XSLT web part
            var webPart = this.collWebPart.get_item(x).get_webPart();

            this.context.load(webPart, 'Properties');
            this.context.executeQueryAsync(Function.createDelegate({ 'wPart': webPart }, function () {
                var props = this.wPart.get_properties();
                var prop_XmlDef = props.get_fieldValues().XmlDefinition;

                // run only if it's a XSLT web part
                if (prop_XmlDef !== undefined) {
                    Roster.CustomActions.FindRepOptions.viewGuid = prop_XmlDef.substr(prop_XmlDef.indexOf('View Name=') + 12, 36).toLowerCase();

                    // get Roster field names
                    var rosterFields = [];
                    var _mapping = Roster.CustomActions.FindRepOptions.fieldsMapping;
                    for (var j = 0; j < _mapping.length; j += 2) {
                        rosterFields.push(_mapping[j]);
                    }

                    Roster.Common.RosterItemData('DataService.svc/RosterItemData', GetUrlKeyValue('ID'), GetUrlKeyValue('ListId'), rosterFields.join(';'), function (data) {
                        var i = 1;
                        var fields = [];
                        var schiftId = '';
                        var startDate = '';
                        var singleFilterTempl = 'FilterField{0}%3D{1}-FilterValue{0}%3D{2}';
                        var multiFilterTempl = 'FilterFields{0}%3D{1}-FilterValues{0}%3D{2}';
                        var multiFilterLookupIdTempl = 'FilterField{0}%3D{1}-FilterValue{0}%3D{2}-FilterOp{0}%3DIn-FilterLookupId{0}%3D1';

                        for (var k = 0; k < data.length; k++) {
                            var fldName = data[k].FieldName;
                            var fldValue = data[k].FieldValue;
                            if (fldName == 'RosterEventId') {
                                schiftId = fldValue; continue;
                            } else if (fldName == 'StartDate') {
                                startDate = fldValue; continue;
                            } else if (filterFieldName == 'ID') {
                                fields.push(String.format(multiFilterTempl, (i++), filterFieldName, parseInt(fldValue))); continue;
                            } else if (Array.indexOf(rosterFields, fldName) < 0) {
                                continue; // field is NOT in mapping list
                            }
                            
                            //var filterFieldName = _mapping[Array.indexOf(_mapping, fldName) + 1];
                            for (var n = 0; n < _mapping.length; n += 2) {
                                if (_mapping[n] != fldName) { continue; }

                                var filterFieldName = _mapping[n + 1];
                                if (data[k].FilterLookupId) {
                                    fields.push(String.format(multiFilterLookupIdTempl, (i++), filterFieldName, escapeProperly(fldValue)));
                                } else {
                                    fields.push(String.format(multiFilterTempl, (i++), filterFieldName, escapeProperly(fldValue)));
                                }
                            }
                        }

                        // open New window
                        if (!Roster.CustomActions.FindRepOptions.showInModal) {
                            window.open(String.format('{0}{1}?SchiftId={4}&CalendarDate={5}#InplviewHash{2}={3}',
                                _spPageContextInfo.webAbsoluteUrl,
                                Roster.CustomActions.FindRepOptions.destinationURL,
                                Roster.CustomActions.FindRepOptions.viewGuid,
                                fields.join('-'), schiftId, startDate));
                        } else {
                            var _width = Roster.CustomActions.FindRepOptions.dialogWidth ? parseInt(Roster.CustomActions.FindRepOptions.dialogWidth) : 600;
                            var _height = Roster.CustomActions.FindRepOptions.dialogHeight ? parseInt(Roster.CustomActions.FindRepOptions.dialogHeight) : 800;
                            OpenPopUpPage(String.format('{0}{1}?SchiftId={4}&CalendarDate={5}&IsDlg=1#InplviewHash{2}={3}',
                                _spPageContextInfo.webAbsoluteUrl,
                                Roster.CustomActions.FindRepOptions.destinationURL,
                                Roster.CustomActions.FindRepOptions.viewGuid,
                                fields.join('-'), schiftId, startDate), null,
                                _width, _height);
                        }
                    });
                }
            }), Function.createDelegate(this, Roster.CustomActions.errorHandler));
        }
    }), Function.createDelegate(this, Roster.CustomActions.errorHandler));
}

Roster.CustomActions.PublishPlannedRoster = function (daysAhead, storeProcName) {
    
    if (Roster.CustomActions.publishingInProgress) {
        SP.UI.Notify.addNotification("Please wait until previous Publish operation completed!", false);
    } else {
        Roster.CustomActions.publishingInProgress = true;
        var waitScreen = SP.UI.ModalDialog.showWaitScreenWithNoClose("Publishing in progress", "Please wait...");

        var spName = storeProcName;
        var _id = GetUrlKeyValue('ID');
        var _idParamName = (_id.length > 20) ? '@eventIds' : '@id';
        if (SP.ScriptHelpers.isNullOrUndefinedOrEmpty(spName)) {
            spName = (_id.length > 20) ? '[dbo].[RorterEvents_PlannedPublish]' : '[dbo].[RorterEvents_MasterRosterPublish]';
        }

        var _params = [{ Name: "@daysAhead", Value: daysAhead }, { Name: _idParamName, Value: _id }, { Name: "@currentUser", Value: _spPageContextInfo.userId }];
        Roster.Common.ExecuteAction(spName, _params, function (result) {
            Roster.CustomActions.publishingInProgress = false;
            waitScreen.close();

            if (result == 1) {
                setTimeout(function () {
                    SP.UI.ModalDialog.RefreshPage(true);
                }, 1000);
            }
        });
    }
}

Roster.CustomActions.PublishPlannedRosterWithDateSelection = function () {

    var doc = Roster.CustomActions.getOriginalWindowObj().document; // helps in case of multiple iframes

    var htmlElement = doc.createElement("div");
    // append DatePicker script
    var s = doc.createElement('script');
    s.type = 'text/javascript';
    s.src = '/_layouts/15/datepicker.js';
    htmlElement.appendChild(s);
    // init table
    var _table = doc.createElement("table");
    _table.style.width = '330px';
    _table.cellPadding = 5;
    // row #1
    var row0 = _table.insertRow(0);
    var cell01 = row0.insertCell(0);
    cell01.innerHTML = "Publish till:";
    var cell02 = row0.insertCell(1);
    var dtInput = doc.createElement("input");
    dtInput.id = "txtEndDate"
    dtInput.type = "text";
    dtInput.readOnly = true;
    cell02.appendChild(dtInput);
    var dtFrame = doc.createElement("iframe");
    dtFrame.id = 'txtEndDateDatePickerFrame';
    dtFrame.src = '/_layouts/15/images/blank.gif?rev=23';
    dtFrame.className = 'owl-date-picker';
    dtFrame.style.display = 'none';
    dtFrame.style.position = 'absolute';
    dtFrame.style.width = '200px';
    dtFrame.style.zIndex = 101;
    cell02.appendChild(dtFrame);
    var dtPicker = doc.createElement("a");
    dtPicker.href = 'javascript:void()';
    dtPicker.style.verticalAlign = 'bottom';
    dtPicker.onclick = function () {
        var dpClickFunc = Roster.CustomActions.getOriginalWindowObj().clickDatePicker;
        dpClickFunc('txtEndDate', '/_layouts/15/iframe.aspx?&cal=1&lcid=3081&langid=3081&ww=0111110&fdow=0&fwoy=0&hj=0&swn=false&minjday=109207&maxjday=2666269&date=', '', event);
        return false;
    };
    var dtPickerImg = doc.createElement("img");
    dtPickerImg.id = 'txtEndDateDatePickerImage';
    dtPickerImg.style.borderWidth = 0;
    dtPickerImg.src = '/_layouts/15/images/calendar_25.gif?rev=23';
    dtPicker.appendChild(dtPickerImg);
    cell02.appendChild(dtPicker);
    // row #2 (buttons)
    var row2 = _table.insertRow(1);
    var cell21 = row2.insertCell(0);
    cell21.colSpan = 2;
    cell21.style.textAlign = 'right';
    var okButton = doc.createElement("input");
    okButton.type = "button";
    okButton.value = "Publish";
    okButton.onclick = function () {
        var selectedDate = doc.getElementById('txtEndDate').value;
        if (selectedDate) {
            var today = new Date();
            var pubDate = Date._parseExact(selectedDate, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
            var utc1 = Date.UTC(pubDate.getFullYear(), pubDate.getMonth(), pubDate.getDate());
            var utc2 = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
            var stProc_data = {
                DaysAhead: Math.floor((utc1 - utc2) / (1000 * 60 * 60 * 24))
            };
            if (stProc_data.DaysAhead < 0) {
                alert('Please don"t enter the date in a past!');
                return;
            }
            SP.UI.ModalDialog.commonModalDialogClose(1, stProc_data);
        } else {
            alert('Please enter a date!');
        }
    };
    cell21.appendChild(okButton);
    var cancelButton = doc.createElement("input");
    cancelButton.type = "button";
    cancelButton.value = "Cancel";
    cancelButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(0);
    };
    cell21.appendChild(cancelButton);
    // add table to container
    htmlElement.appendChild(_table);

    // init Modal Dialog options
    var options = {
        html: htmlElement,
        autoSize: true,
        width: 350,
        title: 'Roster publishing',
        showClose: true,
        dialogReturnValueCallback: function (dialogResult, returnValue) {
            if (dialogResult == SP.UI.DialogResult.OK) {

                Roster.CustomActions.PublishPlannedRoster(returnValue.DaysAhead + 1);

            }
        }
    };
    // show Modal dialog
    SP.UI.ModalDialog.showModalDialog(options);

}

Roster.CustomActions.RemoveRoster = function () {

    Roster.Common.RemoveRoster('DataService.svc/RemoveRoster', GetUrlKeyValue('ID'));

}

Roster.CustomActions.ManageRosterPermissions = function (customPageUrl) {

    //Roster.CustomActions.getOriginalWindowObj().open(
    //    String.format('{0}/_layouts/15/Roster.Presentation/DbListPermissions.aspx?ListId={1}&ElemId={2}',
    //                        _spPageContextInfo.webAbsoluteUrl, GetUrlKeyValue('ListId'), GetUrlKeyValue('ID')
    //                  ), '_blank');

    var defaultPageUrl = _spPageContextInfo.webAbsoluteUrl + '/_layouts/15/Roster.Presentation/DbListPermissions.aspx';
    var pageUrl = customPageUrl || defaultPageUrl;
    pageUrl += '?ListId=' + GetUrlKeyValue('ListId') + '&ElemId=' + GetUrlKeyValue('ID') + '&Source=' + GetSource();

    SP.UI.ModalDialog.OpenPopUpPage(pageUrl);
}

Roster.CustomActions.EditRoster = function () {

    var _rosterId = GetUrlKeyValue('ID');

    Roster.Common.GetContentTypeInfo(_rosterId, function (data) {
        SP.UI.ModalDialog.OpenPopUpPage(String.format('{0}/{1}&ListId={2}&ID={3}',
            _spPageContextInfo.webServerRelativeUrl,
            data.d.EditItemUrl,
            GetUrlKeyValue('ListId'),
            _rosterId
        ), function () { SP.UI.ModalDialog.RefreshPage(true); });
    });

}

Roster.CustomActions.TerminateRoster = function (storeProcName) {

    var doc = Roster.CustomActions.getOriginalWindowObj().document; // helps in case of multiple iframes

    var htmlElement = doc.createElement("div");
    // append DatePicker script
    var s = doc.createElement('script');
    s.type = 'text/javascript';
    s.src = '/_layouts/15/datepicker.js';
    htmlElement.appendChild(s);
    // init table
    var _table = doc.createElement("table");
    _table.style.width = '330px';
    _table.cellPadding = 5;
    // row #1
    var row0 = _table.insertRow(0);
    var cell01 = row0.insertCell(0);
    cell01.innerHTML = "Effective From:";
    var cell02 = row0.insertCell(1);
    var dtInput = doc.createElement("input");
    dtInput.id = "txtEffectiveFrom"
    dtInput.type = "text";
    dtInput.readOnly = true;
    cell02.appendChild(dtInput);
    var dtFrame = doc.createElement("iframe");
    dtFrame.id = 'txtEffectiveFromDatePickerFrame';
    dtFrame.src = '/_layouts/15/images/blank.gif?rev=23';
    dtFrame.className = 'owl-date-picker';
    dtFrame.style.display = 'none';
    dtFrame.style.position = 'absolute';
    dtFrame.style.width = '200px';
    dtFrame.style.zIndex = 101;
    cell02.appendChild(dtFrame);
    var dtPicker = doc.createElement("a");
    dtPicker.href = 'javascript:void()';
    dtPicker.style.verticalAlign = 'bottom';
    dtPicker.onclick = function () {
        var dpClickFunc = Roster.CustomActions.getOriginalWindowObj().clickDatePicker;
        dpClickFunc('txtEffectiveFrom', '/_layouts/15/iframe.aspx?&cal=1&lcid=3081&langid=3081&ww=0111110&fdow=0&fwoy=0&hj=0&swn=false&minjday=109207&maxjday=2666269&date=', '', event);
        return false;
    };
    var dtPickerImg = doc.createElement("img");
    dtPickerImg.id = 'txtEffectiveFromDatePickerImage';
    dtPickerImg.style.borderWidth = 0;
    dtPickerImg.src = '/_layouts/15/images/calendar_25.gif?rev=23';
    dtPicker.appendChild(dtPickerImg);
    cell02.appendChild(dtPicker);
    // row #2
    var row1 = _table.insertRow(1);
    var cell11 = row1.insertCell(0);
    cell11.innerHTML = "Reason:";
    var cell12 = row1.insertCell(1);
    var reasonInput = doc.createElement("textarea");
    reasonInput.id = "txtReason";
    reasonInput.type = "text";
    cell12.appendChild(reasonInput);
    // row #3 (buttons)
    var row2 = _table.insertRow(2);
    var cell21 = row2.insertCell(0);
    cell21.colSpan = 2;
    cell21.style.textAlign = 'right';
    var okButton = doc.createElement("input");
    okButton.type = "button";
    okButton.value = "Terminate";
    okButton.onclick = function () {
        var selectedDate = doc.getElementById('txtEffectiveFrom').value;
        if (selectedDate) {
            var effDate = Date._parseExact(selectedDate, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
            var stProc_data = {
                EffDate: effDate.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z',
                Reason: doc.getElementById('txtReason').value
            };
            SP.UI.ModalDialog.commonModalDialogClose(1, stProc_data);
        } else {
            alert('Please enter "Effective From" date!');
        }
    };
    cell21.appendChild(okButton);
    var cancelButton = doc.createElement("input");
    cancelButton.type = "button";
    cancelButton.value = "Cancel";
    cancelButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(0);
    };
    cell21.appendChild(cancelButton);
    // add table to container
    htmlElement.appendChild(_table);

    // init Modal Dialog options
    var options = {
        html: htmlElement,
        autoSize: true,
        width: 350,
        title: 'Roster termination',
        showClose: true,
        dialogReturnValueCallback: function (dialogResult, returnValue) {
                if (dialogResult == SP.UI.DialogResult.OK) {

                    var spName = storeProcName;
                    var _id = GetUrlKeyValue('ID');
                    var _idParamName = (_id.length > 20) ? '@eventIds' : '@id';
                    if (SP.ScriptHelpers.isNullOrUndefinedOrEmpty(spName)) {
                        spName = (_id.length > 20) ? '[dbo].[RorterEvents_PlannedTerminate]' : '[dbo].[RorterEvents_MasterRosterTerminate]';
                    }

                    Roster.Common.ExecuteAction(spName,
                            [{ Name: "@dateEffective", Value: returnValue.EffDate },
                             { Name: "@reason", Value: returnValue.Reason },
                             { Name: _idParamName, Value: _id },
                             { Name: "@currentUser", Value: _spPageContextInfo.userId }],
                             function (result) {
                                 if (result == 1) {
                                     setTimeout(function () { SP.UI.ModalDialog.RefreshPage(true); }, 1200);
                                 }
                             });

                }
            }
    };
    // show Modal dialog
    SP.UI.ModalDialog.showModalDialog(options);

}

Roster.CustomActions.SaveAsTemplate = function (groupName) {

    if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(groupName))
    {
        Roster.Common.CheckCurrentUserMembership(groupName, function (returnResult) {
            if (returnResult) {
                Roster.Common.SaveMasterAsTemplate('DataService.svc/SaveMasterAsTemplate', GetUrlKeyValue('ID'));
            } else {
                alert("You don't have permissions for this action");
            }
        });
    }
    else
    {
        // there is NO restriction by ShP group
        Roster.Common.SaveMasterAsTemplate('DataService.svc/SaveMasterAsTemplate', GetUrlKeyValue('ID'));
    }
}
Roster.CustomActions.SaveAsTemplate_EnableScript = function (groupName) {

    if (Roster.CustomActions.doesUserCanSaveAsTemplate === undefined) {
        Roster.Common.CheckCurrentUserMembership(groupName, function (returnResult) {
            Roster.CustomActions.doesUserCanSaveAsTemplate = returnResult;
            if (returnResult) { RefreshCommandUI(); }
        });
    } else {
        return Roster.CustomActions.doesUserCanSaveAsTemplate;
    }

    return false;
}

Roster.CustomActions.CopyFromTemplate = function (storeProcName) {

    var spName = storeProcName || '[dbo].[RorterEvents_MasterRosterEvents]';
    var doc = Roster.CustomActions.getOriginalWindowObj().document; // helps in case of multiple iframes

    var htmlElement = doc.createElement("div");
    // append jQuery script
    var jqScript = doc.createElement('script');
    jqScript.type = 'text/javascript';
    jqScript.src = '/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js';
    htmlElement.appendChild(jqScript);
    // init table
    var _table = doc.createElement("table");
    _table.style.width = '430px';
    _table.cellPadding = 5;
    // row #1
    var row0 = _table.insertRow(0);
    var cell01 = row0.insertCell(0);
    cell01.innerHTML = "Select template:";
    var cell02 = row0.insertCell(1);
    cell02.style.textAlign = 'right';
    var ddlTemplates = doc.createElement("select");
    ddlTemplates.id = "templateSelector";
    ddlTemplates.style.width = '250px';
    cell02.appendChild(ddlTemplates);
    // row #2 (buttons)
    var row2 = _table.insertRow(1);
    var cell21 = row2.insertCell(0);
    cell21.colSpan = 2;
    cell21.style.textAlign = 'right';
    var okButton = doc.createElement("input");
    okButton.type = "button";
    okButton.value = "Copy";
    okButton.onclick = function () {
        var selectedTmplId = doc.getElementById('templateSelector').value;
        if (selectedTmplId) {
            SP.UI.ModalDialog.commonModalDialogClose(1, selectedTmplId);
        } else {
            alert('Please select just one template!');
        }
    };
    cell21.appendChild(okButton);
    var cancelButton = doc.createElement("input");
    cancelButton.type = "button";
    cancelButton.value = "Cancel";
    cancelButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(0);
    };
    cell21.appendChild(cancelButton);
    // add table to container
    htmlElement.appendChild(_table);
    // append Custom script
    var tsScript = doc.createElement('script');
    tsScript.type = 'text/javascript';
    tsScript.src = '/_layouts/15/Roster.Presentation/js/templates.selector.js?rev=20151008i04';
    htmlElement.appendChild(tsScript);

    // init Modal Dialog options
    var options = {
        html: htmlElement,
        autoSize: true,
        width: 450,
        title: 'Templates',
        showClose: false,
        dialogReturnValueCallback: function (dialogResult, returnValue) {
            if (dialogResult == SP.UI.DialogResult.OK) {

                var _params = [{ Name: "@souceId", Value: returnValue }, { Name: "@targetId", Value: GetUrlKeyValue('ID') }];
                Roster.Common.ExecuteAction(spName, _params, function (result) {
                            if (result == 1) {
                                setTimeout(function () { SP.UI.ModalDialog.RefreshPage(true); }, 500);
                            }
                        });

            }
        }
    };
    // show Modal dialog
    SP.UI.ModalDialog.showModalDialog(options);

}

Roster.CustomActions.SubmitTimesheet = function (storeProcName) {

    var spName = storeProcName || '[dbo].[RorterEvents_TimesheetSubmit]'; // param or default value

    var connectedFilter = RosterContext.FilterFromConnectedWebPart;
    if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(connectedFilter) && connectedFilter.Field == 'WorkerPersonId') {
        var displayedRosterIds = jQuery('div.ms-acal-item').map(function (div) {
            var href = jQuery(this).find('a').attr('href');
            var _eventId = href.substring(href.indexOf('ID=') + 3);
            return (_eventId.indexOf('.') < 0) ? _eventId : _eventId.substr(0, _eventId.indexOf('.'));
        }).get().join(',');

        Roster.Common.SubmitTimesheet('DataService.svc/SubmitTimesheet',
            spName, connectedFilter.Value, RosterContext.CalendarProps.PeriodStart, RosterContext.CalendarProps.PeriodEnd, displayedRosterIds);
    } else {
        alert('Error: cannot identify Worker Id!');
    }

}

Roster.CustomActions.NewTimesheetRoster = function (contentTypeId) {

    Roster.CustomActions.CreateNewRoster('45C78A0447254B078AFAA638670C080E', contentTypeId);

}

Roster.CustomActions.NewWorkingRoster = function (contentTypeId) {

    Roster.CustomActions.CreateNewRoster('23E6F5B918434F9BAAAB20F45EBB946B', contentTypeId);

}

Roster.CustomActions.NewPlannedRoster = function (contentTypeId) {

    Roster.CustomActions.CreateNewRoster('5B7156BB84A54F8CAE2BBCDE4ED9C486', contentTypeId);

}

Roster.CustomActions.CreateNewRoster = function (listId, contentTypeId) {

    if (Roster.CustomActions.isPageRefreshingNow) {
        return false;
    }

    var contentTypes = jQuery('.ct-for-' + listId).find('option');
    if (contentTypes.length <= 1) {
        Roster.CustomActions.OpenNewRosterForm(contentTypes[0].value);
    } else if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(contentTypeId)) {
        var ctOption = contentTypes.filter(function () {
            return jQuery(this).val().indexOf('ContentTypeId=' + contentTypeId + '&') > 0;
        });
        if (ctOption.length == 0) {
            Roster.CustomActions.OpenNewRosterForm(contentTypes[0].value);
        } else {
            Roster.CustomActions.OpenNewRosterForm(ctOption.val());
        }
    } else {
        Roster.CustomActions.SelectContentType(contentTypes.clone());
    }

}

Roster.CustomActions.SelectContentType = function ($contentTypes) {

    var doc = Roster.CustomActions.getOriginalWindowObj().document; // helps in case of multiple iframes

    var htmlElement = doc.createElement("div");
    // init table
    var _table = doc.createElement("table");
    _table.style.width = '270px';
    _table.cellPadding = 5;
    // row #1
    var row0 = _table.insertRow(0);
    var cell01 = row0.insertCell(0);
    cell01.innerHTML = "Select content type:";
    var cell02 = row0.insertCell(1);
    var ddlContentTypes = doc.createElement("select");
    ddlContentTypes.id = "ctSelector";
    jQuery(ddlContentTypes).empty();
    jQuery(ddlContentTypes).append($contentTypes);
    cell02.appendChild(ddlContentTypes);
    // row #2 (buttons)
    var row2 = _table.insertRow(1);
    var cell21 = row2.insertCell(0);
    cell21.colSpan = 2;
    cell21.style.textAlign = 'right';
    var okButton = doc.createElement("input");
    okButton.type = "button";
    okButton.value = "Create";
    okButton.onclick = function () {
        var selectedCTurl = doc.getElementById('ctSelector').value;
        if (selectedCTurl) {
            SP.UI.ModalDialog.commonModalDialogClose(1, selectedCTurl);
        } else {
            alert('Please select just one content type!');
        }
    };
    cell21.appendChild(okButton);
    var cancelButton = doc.createElement("input");
    cancelButton.type = "button";
    cancelButton.value = "Cancel";
    cancelButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(0);
    };
    cell21.appendChild(cancelButton);
    // add table to container
    htmlElement.appendChild(_table);

    // init Modal Dialog options
    var options = {
        html: htmlElement,
        autoSize: true,
        width: 300,
        title: 'New item',
        showClose: false,
        dialogReturnValueCallback: function (dialogResult, returnValue) {
            if (dialogResult == SP.UI.DialogResult.OK) {
                Roster.CustomActions.OpenNewRosterForm(returnValue);
            }
        }
    };

    // show Modal dialog
    SP.UI.ModalDialog.showModalDialog(options);

}

Roster.CustomActions.OpenNewRosterForm = function (formUrl) {

    //SP.UI.ModalDialog.ShowPopupDialog(formUrl);
    SP.UI.ModalDialog.OpenPopUpPage(formUrl, function () {
        Roster.CustomActions.isPageRefreshingNow = true;
        SP.UI.ModalDialog.RefreshPage(true);
    });

}

Roster.CustomActions.EndorseRoster = function () {

    Roster.Common.EndorseRoster('DataService.svc/EndorseRoster', GetUrlKeyValue('ID'));

}

Roster.CustomActions.RejectRoster = function () {

    var doc = Roster.CustomActions.getOriginalWindowObj().document; // helps in case of multiple iframes

    var htmlElement = doc.createElement("div");
    // init table
    var _table = doc.createElement("table");
    _table.style.width = '260px';
    _table.cellPadding = 5;
    // row #1
    var row1 = _table.insertRow(0);
    var cell11 = row1.insertCell(0);
    cell11.innerHTML = "Reason:";
    var cell12 = row1.insertCell(1);
    var reasonInput = doc.createElement("textarea");
    reasonInput.id = "txtReason";
    reasonInput.type = "text";
    cell12.appendChild(reasonInput);
    // row #2 (buttons)
    var row2 = _table.insertRow(1);
    var cell21 = row2.insertCell(0);
    cell21.colSpan = 2;
    cell21.style.textAlign = 'right';
    var rejectButton = doc.createElement("input");
    rejectButton.type = "button";
    rejectButton.value = "Reject";
    rejectButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(1, doc.getElementById('txtReason').value);
    };
    cell21.appendChild(rejectButton);
    var cancelButton = doc.createElement("input");
    cancelButton.type = "button";
    cancelButton.value = "Cancel";
    cancelButton.onclick = function () {
        SP.UI.ModalDialog.commonModalDialogClose(0);
    };
    cell21.appendChild(cancelButton);
    // add table to container
    htmlElement.appendChild(_table);

    // init Modal Dialog options
    var options = {
        html: htmlElement,
        autoSize: true,
        width: 280,
        title: 'Roster rejection',
        showClose: true,
        dialogReturnValueCallback: function (dialogResult, returnValue) {
            if (dialogResult == SP.UI.DialogResult.OK) {
                Roster.Common.RejectRoster('DataService.svc/RejectRoster', GetUrlKeyValue('ID'), returnValue);
            }
        }
    };
    // show Modal dialog
    SP.UI.ModalDialog.showModalDialog(options);

}

Roster.CustomActions.ApproveRoster = function () {

    Roster.Common.ApproveRoster('DataService.svc/ApproveRoster', GetUrlKeyValue('ID'));

}

Roster.CustomActions.CancelRoster = function () {

    Roster.Common.CancelRoster('DataService.svc/CancelRoster', GetUrlKeyValue('ID'));

}

Roster.CustomActions.SubmitSingleTimesheet = function () {

    Roster.Common.SubmitSingleTimesheet('DataService.svc/SubmitSingleTimesheet', GetUrlKeyValue('ID'), GetUrlKeyValue('ListId'));

}

Roster.CustomActions.DisplayRosterVersionHistory = function (pageUrl) {

    SP.UI.ModalDialog.OpenPopUpPage(String.format('{0}?RosterId={1}', pageUrl, GetUrlKeyValue('ID')));

}

Roster.CustomActions.CallStoredProcedure = function (spName, customParams) {

    var rosterId = GetUrlKeyValue('ID');
    var _params = [{ Name: "@rosterId", Value: rosterId }, { Name: "@currentUser", Value: _spPageContextInfo.userId }];

    if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(customParams)) {
        try {
            var customParamsObj = JSON.parse(customParams);
            _params = _params.concat(customParamsObj);
        } catch (e) {
            alert('Cannot parse custom parameters: ' + customParams);
        }
    }

    Roster.Common.ExecuteAction(spName, _params);

}