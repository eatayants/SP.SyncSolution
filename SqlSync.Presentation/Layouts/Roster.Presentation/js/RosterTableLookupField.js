function $_global_RosterTableLookupControl() {
    (function () {
        if (typeof RosterTableLookupTemplate == "object") {
            return;
        }
        window.RosterTableLookupTemplate = (function () {
            return {
                RosterTableLookupDisplayControlId: "display_rostertable_lookup_field",
                RosterTableLookup_Display: function(rCtx) {
                    if (rCtx == null || rCtx.CurrentFieldValue == null || rCtx.CurrentFieldValue === "") {
                        return "";
                    }
                    var myData = SPClientTemplates.Utility.GetFormContextForCurrentField(rCtx);
                    var fldvalue = RosterTableLookupTemplate.decodeValue(rCtx.CurrentFieldValue);
                    var ret = [];
                    if (fldvalue != null) {
                        ret.push("<span class='rt-lookupfield' id='" + myData.fieldName + '_' + myData.fieldSchema.Id + "'>");
                        ret.push(STSHtmlEncode(fldvalue.Title));
                        ret.push("</span>");
                    }
                    return ret.join("");
                },
                wrapErrorArea : function (renderCtx) {
                    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(renderCtx);
                    var errorContainerId = formCtx.fieldName + "_Lookup_Error";
                    return "<span id='" + errorContainerId + "' class='ms-formvalidation ms-csrformvalidation'></span>";
                },

                RosterTableLookup_Edit: function(rCtx) {
                    if (rCtx == null) {
                        return "";
                    }
                    RosterTableLookupTemplate.initValidators(rCtx);
                    var myData = window.SPClientTemplates.Utility.GetFormContextForCurrentField(rCtx);
                    if (myData == null || myData.fieldSchema == null) {
                        return "";
                    }
                    var fieldProp = RosterTableLookupTemplate.GetFieldProperties(myData);
                    var ret = [];
                    ret.push("<select id='" + myData.fieldName + "' class ='rt-lookupfield'");
                    ret.push(" data-list='" + fieldProp.ListId + "'");
                    ret.push(" data-field='" + fieldProp.FieldId + "'>");
                    var fldvalue = RosterTableLookupTemplate.decodeValue(rCtx.CurrentFieldValue);
                    if (fldvalue != null) {
                        ret.push(" <option selected='selected' value='" + fldvalue.Id + "'>" + fldvalue.Title + "</option>");
                    }
                    ret.push(" </select>");
                    myData.registerInitCallback(myData.fieldName, function() {
                        RosterTableLookupTemplate.InitLookup(myData.fieldName);
                        var $fld = $j("#" + myData.fieldName);
                        if (fldvalue) {
                            $fld.data("selected", RosterTableLookupTemplate.encodeValue(fldvalue));
                        }
                        $fld.on("select2:select", function(e) {
                            $fld.data("selected",
                                RosterTableLookupTemplate.encodeValue({
                                    Id: e.params.data.id,
                                    Title: e.params.data.name
                                }));
                        });
                        $fld.on("select2:unselect", function(e) {
                            $fld.data("selected", null);
                        });
                    });
                    myData.registerGetValueCallback(myData.fieldName, function() {
                        var $fieldData = $j("#" + myData.fieldName).data("selected");
                        if (!$fieldData) {
                            return null;
                        }
                        return $fieldData;
                    });
                    ret.push(RosterTableLookupTemplate.wrapErrorArea(rCtx));
                    return ret.join("");
                },
                RosterTableLookup_View: function(inCtx, field, listItem, listSchema) {
                    if (field.XSLRender === "1") {
                        return listItem[field.Name].toString();
                    } else {
                        var fldvalue = RosterTableLookupTemplate.decodeValue(listItem[field.Name]);
                        if (ctx.inGridMode) {
                            RosterTableLookupTemplate.RegisterGridEditModeControl(ctx, field);
                            return fldvalue ? fldvalue.Title: "";
                        } else {
                            var ret = [];
                            if (fldvalue != null) {
                                ret.push("<span class='rt-lookupfield' id='" + GenerateIID(inCtx) + "'");
                                ret.push(" fld='" + field.Name + "'>");
                                ret.push(STSHtmlEncode(fldvalue.Title));
                                ret.push("</span>");
                            }
                            return ret.join("");
                        }
                    }
                },
                onError: function (fieldName,error) {
                     $j("#"+fieldName + "_Lookup_Error").html("<span role='alert'>" + error.errorMessage + "</span>");
                },
                initValidators : function (renderCtx) {
                    var self = this;
                    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(renderCtx);
                    var fieldValidators = new SPClientForms.ClientValidation.ValidatorSet();
                    if (formCtx.fieldSchema.Required) {
                        fieldValidators.RegisterValidator(new SPClientForms.ClientValidation.RequiredValidator());
                    }
                    formCtx.registerValidationErrorCallback(formCtx.fieldName, function (error) {
                        self.onError(formCtx.fieldName,error);
                    });
                    formCtx.registerClientValidator(formCtx.fieldName, fieldValidators);
                },
                RegisterGridEditModeControl:function(ctx, field) {
                    ExecuteOrDelayUntilScriptLoaded(function() {
                        SP.GanttControl.WaitForGanttCreation(function(ganttChart) {
                            var rosterTableLookupColumn = null;
                            var rosterTableLookupFieldEditId = "edit_rostertable_lookup_field_" + field.Name;
                           
                            var columns = ganttChart.get_Columns();
                            $j.each(columns, function(i, item) {
                                if (item.columnKey === field.Name) {
                                    rosterTableLookupColumn = item;
                                }
                            });
                            rosterTableLookupColumn.fnGetEditControlName = function(record, fieldKey) {
                                return rosterTableLookupFieldEditId;
                            };
                            rosterTableLookupColumn.fnGetDisplayControlName = function () {
                                return window.RosterTableLookupTemplate.RosterTableLookupDisplayControlId;
                            };
                            SP.JsGrid.PropertyType.Utils.RegisterDisplayControl(
                                window.RosterTableLookupTemplate.RosterTableLookupDisplayControlId,
                                window.RosterTableLookupDisplayControl, []);
                            
                            SP.JsGrid.PropertyType.Utils.RegisterEditControl(rosterTableLookupFieldEditId,
                                function(gridContext, cellControl) {
                                    var editorInstance = new RosterTableLookupEditControl();
                                    editorInstance.Init(gridContext, cellControl, field, ctx);
                                    return editorInstance;
                                }, []);
                        });
                    }, "spgantt.js");
                },
                GetFieldProperties:function(rCtx) {
                    var result = { ListId: "", FieldId: "" };
                    if (rCtx != null) {
                        result = $j.extend(result, {
                            ListId: rCtx.listAttributes.Id,
                            FieldId: rCtx.fieldSchema.Id
                        });
                    }
                    return result;
                },
                decodeValue: function (value) {
                    if (!value) {
                        return null;
                    }
                    var item = {};
                    var resultArr = value.split(";#");
                    item.Id = resultArr[0];
                    item.Title = resultArr[1];
                    return item;
                },
                encodeValue: function (value) {
                    if (!value) {
                        return "";
                    }
                    return value.Id + ";#" + value.Title;
                },
                loadFile: function (filename, filetype) {
                    var fileref;
                    if (filetype === "js"){ 
                        fileref = document.createElement("script");
                        fileref.setAttribute("type", "text/javascript");
                        fileref.setAttribute("src", filename);
                    }
                    else if (filetype === "css"){ 
                        fileref = document.createElement("link");
                        fileref.setAttribute("rel", "stylesheet");
                        fileref.setAttribute("type", "text/css");
                        fileref.setAttribute("href", filename);
                    }
                    if (typeof fileref != "undefined") {
                        document.getElementsByTagName("head")[0].appendChild(fileref);
                    }
                },
                LoadExternal: function() {
                    RosterTableLookupTemplate.loadFile("/_layouts/15/Roster.Presentation/js/select2.min.css", "css");
                },
                pathCombine: function (path1, path2) {
                    if (!path1)
                        return path2;
                    if (!path2)
                        return path1;
                    var delimiter = "/";
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
                getFullServiceURL: function (actionUrl) {
                    return RosterTableLookupTemplate.pathCombine(_spPageContextInfo.webServerRelativeUrl,
                        RosterTableLookupTemplate.pathCombine("_vti_bin/Roster.Services/", actionUrl));
                },
                formatResult: function (item) {
                    if (item.loading) {
                        return item.text;
                    }
                    var markup =
                    "<div class='clearfix'>" +
                        "<div clas='col-sm-10'>" +
                            "<div class='clearfix'>" +
                                "<div class='col-sm-12'>" + item.name + "</div>" +
                            "</div>"+
                        "</div>"+
                    "</div>";
                    return markup;
                },
                formatSelection:function(item) {
                    return item.name || item.text;
                },
                InitLookup: function (control, width) {
                    var $field = $j("#" + control);
                    var listId = $field.attr("data-list");
                    var fieldId = $field.attr("data-field");
                    var serviceName = "DataService.svc/LookupField";
                    (function($) {
                        $($field).select2({
                            width: width ? width : "300px", placeholder: "", allowClear: true, minimumInputLength: 0,
                            templateResult: RosterTableLookupTemplate.formatResult,
                            templateSelection: RosterTableLookupTemplate.formatSelection,
                            escapeMarkup: function (markup) {
                                return markup;
                            },
                            ajax: {
                                type: "POST",
                                url: RosterTableLookupTemplate.getFullServiceURL(serviceName),
                                contentType: "application/json; charset=utf-8",
                                processData: false, cache: false,
                                dataType: "json", delay: 250,
                                beforeSend: function (xhr) { xhr.withCredentials = true; },
                                data: function (params) {
                                    return JSON.stringify({
                                        query:
                                        {
                                            Query: params.term,
                                            ListId: listId,
                                            FieldId: fieldId,
                                            Page: params.page ? params.page : 0
                                        }
                                    });
                                },
                                processResults: function (data, page) {
                                    return { page: data.d.Page, results: data.d.Items };
                                },
                                error: function (xhr, status, error) {
                                    if ((xhr.status < 200 || xhr.status >= 400) && status != "abort") {
                                        alert("Sorry, we ran into a technical problem (" + status + ")"
                                            + xhr.statusText + ". Please try again later...");
                                    }
                                    return { results: [] };
                                }
                            }
                        });
                    })($j);
                },
                RosterTableLookup_PreRender: function (inCtx) {

                },
                RosterTableLookup_PostRender: function (inCtx) {
                }
            };
        })();
        window.RosterTableLookupEditControl = (function () {
            function RosterTableLookupEditControl() {
                this._gridContext = null;
                this._gridTextInputElement = null;
                this._cnt = null;
                this._cellContext = null;
                this._editControl = null;
                this._inEdit = false;
                this._dropDownHidden = true;
                this.SupportedWriteMode = SP.JsGrid.EditActorWriteType.LocalizedOnly;
                this.SupportedReadMode = SP.JsGrid.EditActorReadType.LocalizedOnly;
            }
            RosterTableLookupEditControl.prototype.Init = function (gridContext, gridTextInputElement, field, ctx) {
                var self = this;

                this._onActivateActor = function () {
                    self._gridContext.OnActivateActor();
                };
                this._onKeyDown = function (e) {
                    self.onKeyDown(e);
                };
                this._onKeyUp = function (e) {
                    self.onKeyUp(e);
                };
                this._onMouseDown = function (e) {
                    self.onMouseDown(e);
                };
                this._gridContext = gridContext;
                this._gridTextInputElement = gridTextInputElement;
                this._field = field;
                this._ctx = ctx;

                this._cnt = document.createElement("div");
                this._cnt.style.cssText = "visibility:hidden;position:absolute;top:0px;left:0px;";
                this._cnt.id = "wrapper_"+this._field.Name;
                this._gridContext.parentNode.appendChild(this._cnt);
            };

            RosterTableLookupEditControl.prototype.onKeyDown = function (eventInfo) {
                if (eventInfo.keyCode === Sys.UI.Key.tab) {
                    eventInfo.stopPropagation();
                } else {
                    this._gridContext.OnKeyDown(eventInfo);
                }
            };

            RosterTableLookupEditControl.prototype.onKeyUp = function (eventInfo) {
            };

            RosterTableLookupEditControl.prototype.onMouseDown = function (eventInfo) {
                eventInfo.stopPropagation();
            };

            RosterTableLookupEditControl.prototype._setupHandlers = function (attachActions) {
                var self = this;
                if (attachActions) {
                    $addHandler(self._editControl, 'focus', self._onActivateActor);
                    $addHandler(self._editControl, 'keydown', self._onKeyDown);
                    $addHandler(self._editControl, 'keyup', self._onKeyUp);
                    $addHandler(self._editControl, 'mousedown', self._onMouseDown);                
                } else {
                    $removeHandler(self._editControl, 'focus', self._onActivateActor);
                    $removeHandler(self._editControl, 'keydown', self._onKeyDown);
                    $removeHandler(self._editControl, 'keyup', self._onKeyUp);
                    $removeHandler(self._editControl, 'mousedown', self._onMouseDown);
                }
            };

            RosterTableLookupEditControl.prototype.BindToCell = function (cellContext) {
                this._cellContext = cellContext;
                this._cnt.style.width = this._cellContext.cellWidth + "px";
                this._cnt.style.height = this._cellContext.cellHeight + "px";
                this.OnBeginEdit();
            };

            RosterTableLookupEditControl.prototype.OnCellMove = function () {
            };

            RosterTableLookupEditControl.prototype.Focus = function (eventInfo) {
                try {
                    this._editControl.focus();
                } catch (error) {
                }
            };

            RosterTableLookupEditControl.prototype._initLookup = function (value) {
                var self = this;
                self._dropDownHidden = false;
                if (!this._editControl) {
                    this._editControl = document.createElement("select");
                    this._editControl.id = this._field.Name;
                    this._cnt.appendChild(this._editControl);
                    var $edit = $j("#" + this._editControl.id);
                    $edit.attr("data-list", this._ctx.listName);
                    $edit.attr("data-field", this._field.ID);
                    RosterTableLookupTemplate.InitLookup(this._editControl.id, this._cnt.style.width);
                    $edit.on("select2:close", function (e) {
                        self._dropDownHidden = true;
                    });
                    $edit.on("select2:open", function (e) {
                        self._dropDownHidden = false;
                    });
                    $edit.on("select2:select", function (e) {
                        self._dropDownHidden = true;
                        $edit.data("selected", RosterTableLookupTemplate.encodeValue({
                            Id: e.params.data.id,
                            Title: e.params.data.name
                        }));
                    });
                    $edit.on("select2:unselect", function (e) {
                        self._dropDownHidden = true;
                        $edit.data("selected", RosterTableLookupTemplate.encodeValue({
                            Id: 0,Title: ""
                        }));
                    });
                }

                var $editControl = $j("#" + this._editControl.id);
                $editControl.width(this._cnt.style.width);
                $editControl.empty();
                var $option = $j("<option selected></option>");
                var objValue = RosterTableLookupTemplate.decodeValue(value);
                if (objValue) {
                    $option.text(objValue.Title).val(objValue.Id);
                }
                $editControl.append($option).trigger("change");
            }

            RosterTableLookupEditControl.prototype.OnBeginEdit = function (eventInfo) {
                this._inEdit = true;
                this._initLookup(this._cellContext.originalValue.data);
                this._cellContext.Show(this._cnt);
                this._setupHandlers(true);
                this.Focus(eventInfo);
            };

            RosterTableLookupEditControl.prototype.OnEndEdit = function (eventInfo) {
                if (this._dropDownHidden) {
                    this._cellContext.Hide(this._cnt);
                    this._inEdit = false;
                    this._setupHandlers(false);
                    var $editControl = $j("#" + this._editControl.id);
                    var data = $editControl.data("selected");
                    if (data) {
                        var objectValue = RosterTableLookupTemplate.decodeValue(data);
                        if (objectValue != null && objectValue.Id === 0) {
                            this._cellContext.SetCurrentValue({ data: null, localized: null });
                        } else {
                            //this._ctx.jsGridObj.ClearAllErrorsOnCell(_cellContext.record.recordKey, columnName);
                            this._cellContext.SetCurrentValue({ data: data, localized: data });
                        }
                        this._cellContext.NotifyEditComplete();
                    }
                }
            };

            RosterTableLookupEditControl.prototype.Unbind = function () {
                var $edit = $j("#" + this._editControl.id);
                $edit.select2("close");
                this._dropDownHidden = true;
                this.OnEndEdit();
                $edit.data("selected", null);
            };
            RosterTableLookupEditControl.prototype.Dispose = function () {
            };
            return RosterTableLookupEditControl;
        })();
        window.RosterTableLookupDisplayControl = {
            Id: window.RosterTableLookupTemplate.RosterTableLookupDisplayControlId,
            Render: function (value, record, column, field, propType, style, jsGridObj, RTL, containerElem) {
                var result = "";
                if (value != null && value.localized) {
                    var objValue = RosterTableLookupTemplate.decodeValue(value.localized);
                    if (objValue != null && objValue.Title) {
                        result = objValue.Title;
                    }
                } 
                var span = document.createElement("span");
                span.textContent = result;
                return span;
            }
        };

        function registerRosterTableLookupTemplate() {
            var rosterTableLookupContext = {};
            RosterTableLookupTemplate.LoadExternal();
            rosterTableLookupContext.Templates = {};
            rosterTableLookupContext.OnPreRender = RosterTableLookupTemplate.RosterTableLookup_PreRender;
            rosterTableLookupContext.OnPostRender = RosterTableLookupTemplate.RosterTableLookup_PostRender;
            rosterTableLookupContext.Templates.Fields = {
                'RosterTableLookupField': {
                    'View': RosterTableLookupTemplate.RosterTableLookup_View,
                    'DisplayForm': RosterTableLookupTemplate.RosterTableLookup_Display,
                    'EditForm': RosterTableLookupTemplate.RosterTableLookup_Edit,
                    'NewForm': RosterTableLookupTemplate.RosterTableLookup_Edit
                }
            };
            window.SPClientTemplates.TemplateManager.RegisterTemplateOverrides(rosterTableLookupContext);
        }
        ExecuteOrDelayUntilScriptLoaded(registerRosterTableLookupTemplate, "clienttemplates.js");
        
    })();
}
window.$j = jQuery.noConflict(); 
$_global_RosterTableLookupControl();