if (typeof (Roster) == "undefined")
    Roster = {};

String.prototype.format = String.prototype.f = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
        if (m === "{{") { return "{"; }
        if (m === "}}") { return "}"; }
        return args[n];
    });
};

Roster.Common = {
    errorHandler: function (xhr, textStatus, error) {
        //alert("Sorry, we ran into a technical problem (" + xhr.status + ")" + xhr.statusText + ". Please try again...");
        alert("Error: " + xhr.statusText);
    },
    pathCombine: function (path1, path2) {
        if (!path1)
            return path2;
        if (!path2)
            return path1;

        var delimiter = '/';
        var result = path1;

        if (path1.substr(path1.length - 1) === "/" || path2.substr(0, 1) === "/") {
            delimiter = '';
            if (path1.substr(path1.length - 1) === "/" && path2.substr(0, 1) === "/") {
                result = result.substr(0, result.length - 1);
            }
        }

        result += delimiter + path2;

        return result;
    },
    getDictionaryValue: function (array, key) {
        // Get the dictionary value from the array at the specified key      	
        var keyValue = key;
        var result;
        jQuery.each(array, function () {
            if (this.Key == keyValue) {
                result = this.Value;
                return false;
            }
        });
        return result;
    },
    getFullServiceURL: function (actionUrl) {
        return Roster.Common.pathCombine(_spPageContextInfo.webServerRelativeUrl,
            Roster.Common.pathCombine("_vti_bin/Roster.Services/", actionUrl));
    }
};

Roster.IsGuid = function (value) {
    var self = this;
    var tryValue = value;
    var guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

    self.isValid = function () {
        return guidRegex.test(tryValue);
    }
};

Roster.NewGuid = function () {
    function s4() {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4());
}

function formatResult(item) {
    if (item.loading) {
        return item.text;
    }
    var markup =
    "<div class='clearfix'>" +
        "<div clas='col-sm-10'>" +
            "<div class='clearfix'>" +
                "<div class='col-sm-12'>" + item.name + "</div>" +
                "<div class='col-sm-12'>" + item.description + "</div>" +
            "</div>"+
        "</div>"+
    "</div>";
    return markup;
}
function formatSelection(item) {
    return item.name || item.text;
}

Roster.Common.LookupInit = function (controlName, valueName, serviceName,
    source, key, fields, listType, dependentId, subcontrols) {
    jQuery(controlName).select2({
        width: "300px",
        placeholder: "", allowClear: true,
        minimumInputLength: 0,
        escapeMarkup: function(markup) { return markup; },
        templateResult: formatResult,
        templateSelection: formatSelection,
        ajax: {
            type: "POST",
            url: Roster.Common.getFullServiceURL(serviceName),
            contentType: "application/json; charset=utf-8",
            processData: false,cache: false,
            dataType: "json",delay: 250,
            beforeSend: function(xhr) {
                xhr.withCredentials = true;
            },
            data: function (params) {
                var metadataIdValue = jQuery(controlName).attr("data-metadataId");
                var $dependent = jQuery("#" + dependentId + "_ddlValue");
                var parentKeyValue = $dependent.length > 0 ? $dependent.val() : "";
                return JSON.stringify({
                    query:
                    {
                        Query: params.term,
                        Source: source,
                        Key: key,
                        Fields: fields,
                        ListType: listType,
                        MetadataId: metadataIdValue,
                        ParentKeyValue: parentKeyValue,
                        Page: params.page ? params.page : 0
                    }
                });
            },
            processResults: function(data, page) {
                return {
                    page: data.d.Page,
                    results: data.d.Items
                };
            },
            error: function(xhr, status, error) {
                if (xhr.status < 200 && xhr.status >= 400) {
                    alert("Sorry, we ran into a technical problem (" + status + ")"
                        + xhr.statusText + ". Please try again later...");
                }
                return { results: [] };
            },
        }
    });
    jQuery(controlName).off("lookup.unselect");
    jQuery(controlName).on("lookup.unselect", function (e) {
        jQuery(valueName).val("");
        var $internalName = jQuery(controlName).attr("data-name");
        var $dependents = jQuery("[data-dependent='" + $internalName + "']");
        jQuery.each($dependents, function (i, item) {
            var $item = jQuery(item);
            if ($item.length > 0) {
                $item.val(null).trigger("change");
                $item.trigger("lookup.unselect");
            }
        });
        jQuery.each(subcontrols, function (i, item) {
            var $control = jQuery(item.controlId);
            if ($control.length > 0) {
                $control.html("");
            }
        });
    });

    jQuery(controlName).off("select2:select");
    jQuery(controlName).on("select2:select", function (e) {
        jQuery(valueName).val(e.params.data.id);
        var $internalName = jQuery(controlName).attr("data-name");
        var $dependents = jQuery("[data-dependent='" + $internalName + "']");
        jQuery.each($dependents, function (i, item) {
            var $item = jQuery(item);
            if ($item.length > 0) {
                $item.val(null).trigger("change");
                $item.trigger("lookup.unselect");
            }
        });
        jQuery.each(subcontrols, function (i, item) {
            var $control = jQuery(item.controlId);
            if ($control.length > 0) {
                var propertyItem = _.find(e.params.data.property, function(value) {
                    return value.Name === item.fieldName;
                });
                if (propertyItem != null) {
                    $control.html(propertyItem.Value);
                }
            }
        });
    });
    jQuery(controlName).off("select2:unselect");
    jQuery(controlName).on("select2:unselect", function (e) {
        jQuery(controlName).trigger("lookup.unselect");
    });
}

Roster.Common.ListInit = function (control, valueName, serviceName, viewId, fieldId, displayField) {
    jQuery(control).select2({
        width: "220px",
        placeholder: "",
        allowClear: true,
        minimumInputLength: 1,
        ajax: {
            type: "POST",
            url: Roster.Common.getFullServiceURL(serviceName),
            contentType: "application/json; charset=utf-8",
            processData: false,
            dataType: "json",
            delay: 250,
            beforeSend: function (xhr) {
                xhr.withCredentials = true;
            },
            data: function (params) {
                return JSON.stringify({
                    query:
                        {
                            Query: params.term,
                            ViewId: viewId,
                            FieldId: fieldId,
                            DisplayField: displayField,
                            Page: params.page ? params.page : 0
                        }
                });
            },
            processResults: function (data, page) {
                return {
                    page: data.d.Page,
                    results: data.d.Items
                };
            },
            error: function (xhr, status, error) {
                if (status < 200 && status >= 400) {
                    alert("Sorry, we ran into a technical problem (" + status + ")"
                        + xhr.statusText + ". Please try again later...");
                }
                return { results: [] };
            },
            cache: false
        }
    }).on("change", function (e) {
        //jQuery(valueName).val(jQuery(control).select2("val"));

    });
}

Roster.Common.FilterInit = function (control, filterProps, onMenuClick) {
    jQuery(control).select2({
        width: "220px",
        placeholder: "", allowClear: true, minimumInputLength: 1, cache: false,
        ajax: {
            type: "POST",
            url: Roster.Common.getFullServiceURL(filterProps.ServiceName),
            contentType: "application/json; charset=utf-8",
            processData: false, dataType: "json", delay: 250,
            beforeSend: function (xhr) {
                xhr.withCredentials = true;
            },
            data: function (params) {
                return JSON.stringify({
                    query: {
                        Query: params.term,
                        ViewId: filterProps.ViewId,
                        FieldId: filterProps.FieldId,
                        DisplayField: filterProps.DisplayField,
                        Page: params.page ? params.page : 0
                    }
                });
            },
            processResults: function (data, page) {
                return {
                    page: data.d.Page,
                    results: data.d.Items
                };
            },
            error: function (xhr, status, error) {
                if (status < 200 && status >= 400) {
                    alert("Sorry, we ran into a technical problem (" + status + ")" + xhr.statusText + ". Please try again later...");
                }
                return { results: [] };
            }
        }
    }).on("change", function (e) {
        eval(onMenuClick.replace("__viewAllFilters__", jQuery(control).select2("val")));
    });
}

Roster.Common.DateFilterInit = function (control, onMenuClick) {
    $j(control).datepicker({
        dateFormat: "yy-mm-dd",
        onSelect: function (dateText, inst) {
            eval(onMenuClick.replace("filterBySingleDate", escapeProperly(dateText)));
        },
        afterShow: function (inst) {
            $j('#ui-datepicker-div').on('mouseenter', function () {
                setTimeout(function () { ClearTimeOutToHideMenu(); }, 500); // cancel HideMenu call
            });
            $j('.ui-datepicker-next, .ui-datepicker-prev').on('click', function (e) {
                cancelDefault(e); // cancel HideMenu call
            });
        }
    });
}

Roster.Common.RosterItemData = function (actionUrl, itemId, listId, fields, callBackFunc) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
                query: {
                    ItemId: itemId,
                    ListId: listId,
                    Fields: fields
                }
        }),
        success: function (data) {
            callBackFunc(data.d.Props);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.TemplateLookupInit = function (controlName, serviceName, listId, callbackFunc) {
    jQuery(controlName).select2({
        width: "300px",
        placeholder: "", allowClear: true, minimumInputLength: 1,
        ajax: {
            type: "POST",
            url: Roster.Common.getFullServiceURL(serviceName),
            contentType: "application/json; charset=utf-8",
            processData: false, dataType: "json", delay: 250,
            beforeSend: function (xhr) {
                xhr.withCredentials = true;
            },
            data: function (params) {
                return JSON.stringify({
                    query:
                        {
                            Query: params.term,
                            ListId: listId,
                            Page: params.page ? params.page : 0
                        }
                });
            },
            processResults: function (data, page) {
                return {
                    page: data.d.Page,
                    results: data.d.Items
                };
            },
            error: function (xhr, status, error) {
                if (status < 200 && status >= 400) {
                    alert("Sorry, we ran into a technical problem (" + status + ")" + xhr.statusText + ". Please try again later...");
                }
                return { results: [] };
            },
            cache: false
        }
    }).on("change", function (e) {
        callbackFunc(jQuery(controlName).select2("val"));
    });
}

Roster.Common.ListTemplates = function (control, serviceName, callbackFunc) {
    jQuery(control).select2({
        width: "300px",
        placeholder: "", allowClear: true, minimumInputLength: 1,
        ajax: {
            type: "POST",
            url: Roster.Common.getFullServiceURL(serviceName),
            contentType: "application/json; charset=utf-8",
            processData: false, dataType: "json", delay: 250,
            beforeSend: function (xhr) {
                xhr.withCredentials = true;
            },
            data: function (params) {
                return JSON.stringify({
                    query:
                        {
                            Query: params.term,
                            Page: params.page ? params.page : 0
                        }
                });
            },
            processResults: function (data, page) {
                return {
                    page: data.d.Page,
                    results: data.d.Items
                };
            },
            error: function (xhr, status, error) {
                if (status < 200 && status >= 400) {
                    alert("Sorry, we ran into a technical problem (" + status + ")" + xhr.statusText + ". Please try again later...");
                }
                return { results: [] };
            },
            cache: false
        }
    }).on("change", function (e) {
        callbackFunc(jQuery(control).select2("val"));
    });
}

Roster.Common.ListTemplatesAll = function (serviceName, callbackFunc) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(serviceName),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                Query: '',
                Page: 0
            }
        }),
        success: function (data) {
            callbackFunc(data.d.Items);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.SaveRosterAsTemplate = function (actionUrl, itemId, listId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId,
                ListId: listId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Roster successfully saved as template!", false);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.SaveMasterAsTemplate = function (actionUrl, itemId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                MasterId: itemId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Master successfully saved as template!", false);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.RemoveRoster = function (actionUrl, itemId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId
            }
        }),
        success: function (data) {
            SP.UI.ModalDialog.commonModalDialogClose(SP.UI.DialogResult.OK);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.ExecuteAction = function (spName, parameters, callbackFunc) {
    // if URl contains 'ID' param -> add it to Parameters list as '__item_ID'
    var _id = GetUrlKeyValue('ID');
    if (_id) {
        parameters.push({ Name: "@__item_ID", Value: _id });
    }

    var executeActionQuery = JSON.stringify({
        query: {
            Name: spName,
            Parameters: parameters
        }
    });
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL("DataService.svc/ExecuteAction"),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: executeActionQuery,
        success: function (data) {
            SP.UI.Notify.addNotification("Done!", false);

            if (typeof callbackFunc == "function") {
                callbackFunc(1);
            }
        },
        error: function (xhr, status, error) {
            if (typeof callbackFunc == "function") {
                callbackFunc(0);
            }

            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.CheckCurrentUserMembership = function (groupName, callbackFunc) {

    this.grName = groupName.toLowerCase();
    this.callBackFunction = callbackFunc;
    var clientContext = SP.ClientContext.get_current();
    this.currentUser = clientContext.get_web().get_currentUser();
    clientContext.load(this.currentUser);

    this.userGroups = this.currentUser.get_groups();
    clientContext.load(this.userGroups);
    clientContext.executeQueryAsync(Function.createDelegate(this, function (sender, args) {
        var isMember = false;
        var groupsEnumerator = this.userGroups.getEnumerator();
        while (groupsEnumerator.moveNext()) {
            var group = groupsEnumerator.get_current();
            if (group.get_title().toLowerCase() == this.grName) {
                isMember = true;
                break;
            }
        }
        this.callBackFunction(isMember);
    }), null);

}

Roster.Common.SaveRosterItemData = function (listId, itemId, fieldName, value) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL('DataService.svc/SaveRosterItemData'),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId,
                ListId: listId,
                FieldName: fieldName,
                Value: value
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Roster with ID '" + itemId + "' successfully updated!", false);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.SubmitTimesheet = function (actionUrl, spName, workerId, periodStart, periodEnd, rosterIds) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                StoredProcedureName: spName,
                WorkerId: workerId,
                PeriodStart: periodStart,
                PeriodEnd: periodEnd,
                RosterIDs: rosterIds
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Success", false);
            SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.GetPopupContent = function (listId, viewId, rosterId, callbackFunc) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL("DataService.svc/PopupContent"),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ListId: listId,
                ViewId: viewId,
                ItemId: rosterId
            }
        }),
        success: function (data) {
            callbackFunc(data);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.GetContentTypeInfo = function (rosterId, callbackFunc) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL("DataService.svc/GetContentTypeByRosterId"),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                RosterId: rosterId
            }
        }),
        success: function (data) {
            callbackFunc(data);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.ShowHideTime = function (isAllDayEvent, fields) {
    
    for (var k = 0; k < fields.length; k++) {
        jQuery('td[fieldname="' + fields[k] + '"] .ms-dttimeinput').toggle(!isAllDayEvent);

        // in case of Time part is in a separate row
        jQuery('td[fieldname="' + fields[k] + '_time"]').parent().toggle(!isAllDayEvent);
    }

}

Roster.Common.RecurrenceCheckBoxClickEvent = function (isRecurrenceEvent, recWrapper, timeFields) {

    // show/hide recurrence block
    document.getElementById(recWrapper).style.display = isRecurrenceEvent ? 'block' : 'none'

    var formRow = null;
    var timeRows = [];
    if (isRecurrenceEvent) {
        // move TIME to next row
        for (var k = 0; k < timeFields.length; k++) {
            var timePart = jQuery('td[fieldname="' + timeFields[k] + '"] .ms-dttimeinput');
            var timePartVisibility = timePart.is(':visible');
            if (timePart.length) {
                formRow = timePart.closest('table').closest('tr');
                formRow.after(String.format(
                    '<tr><td class="ms-formlabel"><span>{0}</span></td><td class="ms-formbody" style="width: 113px;vertical-align:top" fieldname="{1}"><table><tr></tr></table></td></tr>',
                    formRow.find('td:first').text().replace(new RegExp("date", "gi"), "time"), timeFields[k] + "_time"));
                timePart.show();
                var newTimeRow = formRow.next();
                timePart.appendTo(newTimeRow.find('tr:last'));
                newTimeRow.toggle(timePartVisibility);
                timeRows.push(newTimeRow);
            }
        }
    } else {
        // move TIME part back to Date block
        for (var k = 0; k < timeFields.length; k++) {
            var timePart = jQuery('td[fieldname="' + timeFields[k] + '_time"]');
            var timePartVisibility = timePart.is(':visible');
            if (timePart.length) {
                var currentTimeRow = timePart.closest('tr');
                var dateRow = jQuery('td[fieldname="' + timeFields[k] + '"]').closest('tr');
                currentTimeRow.find('.ms-dttimeinput').appendTo(dateRow.find('tr:last'));
                dateRow.find('.ms-dttimeinput').toggle(timePartVisibility);
                currentTimeRow.remove();
            }
        }
    }

    if (timeRows.length && formRow != null) {
        // move all TIME rows after the last DATE row
        for (var j = timeRows.length - 1; j >= 0; j--) {
            timeRows[j].insertAfter(formRow);
        }
    }
}

Roster.Common.JSonGetWebAction = function (actionUrl, actionParams, success, error) {
    var webActionUrl = Roster.Common.getFullServiceURL(actionUrl);
    var webActionParams = actionParams;
    var self = this;
    return jQuery.getJSON(webActionUrl,
			webActionParams)
		.then(
		function (data) {
		    if (typeof (success) == "function") {
		        success(data);
		    }
		    return data;
		})
		.fail(function (jqXHR, textStatus, errorThrown) {
		    if (typeof (error) == 'function')
		        error(jqXHR, textStatus, errorThrown);
		    else
		        Roster.Common.errorHandler(jqXHR, textStatus, errorThrown);
		}).promise();
}

Roster.Common.JSonPostWebAction = function (actionUrl, actionParams, success, error) {
    var webActionUrl = Roster.Common.getFullServiceURL(actionUrl);
    var webActionParams = actionParams;
    var self = this;
    self.Execute = function () {
        jQuery.ajax({
            type: 'POST',
            url: webActionUrl,
            contentType: 'application/json',
            processData: false,
            dataType: 'json',
            data: JSON.stringify(webActionParams),
            success: function (response, status, jqXHR) {
                if (typeof (success) == "function") {
                    success(response);
                }
            }
        }).error(function (jqXHR, textStatus, errorThrown) {
            if (typeof (error) == "function")
                error(jqXHR, textStatus, errorThrown);
            else
                Roster.Common.errorHandler(jqXHR, textStatus, errorThrown);
        });
    }

}

Roster.Common.QueryString = (function (a) {
    if (a == "") return {};
    var b = {};
    for (var i = 0; i < a.length; ++i) {
        var p = a[i].split('=');
        if (p.length != 2) continue;
        try {
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
        } catch (e) { }
    }
    return b;
})(window.location.search.substr(1).split('&'))

Roster.Common.EndorseRoster = function (actionUrl, itemId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Success", false);
            setTimeout(function () { SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK); }, 1234);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.RejectRoster = function (actionUrl, itemId, reason) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId,
                Reason: reason
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Roster has been rejected!", false);
            setTimeout(function () { SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK); }, 1234);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.ApproveRoster = function (actionUrl, itemId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Success", false);
            setTimeout(function () { SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK); }, 1234);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.CancelRoster = function (actionUrl, itemId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Roster canceled successfully", false);
            setTimeout(function () { SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK); }, 1234);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.SubmitSingleTimesheet = function (actionUrl, itemId, listId) {
    jQuery.ajax({
        type: "POST",
        url: Roster.Common.getFullServiceURL(actionUrl),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                ItemId: itemId,
                ListId: listId
            }
        }),
        success: function (data) {
            SP.UI.Notify.addNotification("Roster submitted successfully", false);
            setTimeout(function () { SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK); }, 1234);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}

Roster.Common.InitStandalonePicker = function (controlName, valueName, allowSelection, chooseFromGroup, required) {
    
    SP.SOD.executeFunc('sp.js', 'SP.ClientContext', function() {
        var schema = new Array();
        schema['SearchPrincipalSource'] = 15;
        schema['ResolvePrincipalSource'] = 15;
        schema['AllowMultipleValues'] = false;
        schema['MaximumEntitySuggestions'] = 50;
        schema['ShowUserPresence'] = false;
        schema['Required'] = required;

        schema['PrincipalAccountType'] = (allowSelection == "RadSelectionPeopleOnly" || chooseFromGroup != '') ? "User" : "User,DL,SecGroup,SPGroup",
        schema['SharePointGroupID'] = chooseFromGroup == '' ? null : chooseFromGroup;
        schema['Web'] = chooseFromGroup == '' ? null : _spPageContextInfo.webServerRelativeUrl;
        schema['OnUserResolvedClientScript'] = function () {
            var pickerObj = SPClientPeoplePicker.SPClientPeoplePickerDict[controlName + '_TopSpan'];
            var users = pickerObj.GetAllUserInfo();
            document.getElementsByName(valueName)[0].value = JSON.stringify(users);
        };

        var initialData = null;
        var initialDataAsString = document.getElementsByName(valueName)[0].value;
        if (initialDataAsString != '') {
            initialData = JSON.parse(initialDataAsString);
        }

        SP.SOD.executeFunc("clienttemplates.js", "SPClientTemplates", function () {
            SP.SOD.executeFunc("clientforms.js", "SPClientPeoplePicker_InitStandaloneControlWrapper", function () {
                SPClientPeoplePicker_InitStandaloneControlWrapper(controlName, initialData, schema);
            });
        });
    });

}

Roster.Common.EnsureUser = function (fieldId, xmlText, callbackFunc) {
    jQuery.ajax({
        type: "POST", async: false,
        url: Roster.Common.getFullServiceURL("DataService.svc/EnsureUser"),
        contentType: "application/json; charset=utf-8",
        processData: false, dataType: "json",
        beforeSend: function (xhr) {
            xhr.withCredentials = true;
        },
        data: JSON.stringify({
            query: {
                FieldMetedataId: fieldId,
                XmlText: xmlText
            }
        }),
        success: function (data) {
            callbackFunc(data);
        },
        error: function (xhr, status, error) {
            Roster.Common.errorHandler(xhr, status, error);
        },
        cache: false
    });
}