(function (Handsontable) {
    //"use strict";

    //-----------------------
    // ------- SELECT2 ------
    //-----------------------
    var Select2Editor = Handsontable.editors.TextEditor.prototype.extend();

    Select2Editor.prototype.prepare = function (row, col, prop, td, originalValue, cellProperties) {
        var $instance = this;
        Handsontable.editors.TextEditor.prototype.prepare.apply(this, arguments);

        this.options = {
            width: 'resolve',
            placeholder: "",
            allowClear: true,
            minimumInputLength: 1,
            escapeMarkup: function (markup) { return markup; },
            templateResult: function (item) {
                if (item.loading) {
                    return item.text;
                }
                var markup =
                "<div class='clearfix'>" +
                    "<div clas='col-sm-10'>" +
                        "<div class='clearfix'>" +
                            "<div class='col-sm-12'>" + item.text + "</div>" +
                        "</div>" +
                    "</div>" +
                "</div>";
                return markup;
            },
            templateSelection: function (item) { return item.name || item.text; },
            ajax: {
                type: "POST",
                url: Roster.Common.getFullServiceURL('DataService.svc/Lookup'),
                contentType: "application/json; charset=utf-8",
                processData: false, dataType: "json", delay: 250, cache: false,
                data: function (params) {
                    var queryData = {
                        Query: params.term,
                        Page: params.page ? params.page : 0
                    };
                    var valueParts = Handsontable.helper.stringify(cellProperties.instance.getDataAtRowProp(row, cellProperties.select2LookupOptions.ParentData)).split(';#');
                    var postData = jQuery.extend(jQuery.extend(queryData, cellProperties.select2LookupOptions), { ParentKeyValue: valueParts.length > 0 ? valueParts[0] : "" });
                    return JSON.stringify({ query: postData });
                },
                beforeSend: function (xhr) {
                    xhr.withCredentials = true;
                },
                processResults: function (data, page) {
                    return {
                        results: jQuery.map(data.d.Items, function (obj) { return { id: obj.id, text: obj.name }})
                    };
                }
            }
        };

        //if (this.cellProperties.select2Options) {
        //    this.options = jQuery.extend(this.options, cellProperties.select2Options);
        //    //jQuery.extend(true,{}, defaults, actual)
        //}
    };

    Select2Editor.prototype.createElements = function () {
        this.$body = $(document.body);

        this.TEXTAREA = document.createElement('select');
        this.$textarea = $(this.TEXTAREA);

        Handsontable.Dom.addClass(this.TEXTAREA, 'handsontableInput');

        this.textareaStyle = this.TEXTAREA.style;
        this.textareaStyle.width = 0;
        this.textareaStyle.height = 0;

        this.TEXTAREA_PARENT = document.createElement('DIV');
        Handsontable.Dom.addClass(this.TEXTAREA_PARENT, 'handsontableInputHolder');

        this.textareaParentStyle = this.TEXTAREA_PARENT.style;
        this.textareaParentStyle.top = 0;
        this.textareaParentStyle.left = 0;
        this.textareaParentStyle.display = 'none';

        this.TEXTAREA_PARENT.appendChild(this.TEXTAREA);

        this.instance.rootElement.appendChild(this.TEXTAREA_PARENT);

        var that = this;
        this.instance._registerTimeout(setTimeout(function () {
            that.refreshDimensions();
        }, 0));
    };

    var onSelect2Changed = function () {
        this.close();
        this.finishEditing();
    };
    var onSelect2Closed = function () {
        this.close();
        this.finishEditing();
    };
    var onBeforeKeyDownSelect2 = function (event) {
        var instance = this;
        var that = instance.getActiveEditor();

        var keyCodes = Handsontable.helper.keyCode;
        var ctrlDown = (event.ctrlKey || event.metaKey) && !event.altKey; //catch CTRL but not right ALT (which in some systems triggers ALT+CTRL)


        //Process only events that have been fired in the editor
        if (!$(event.target).hasClass('select2-input') || event.isImmediatePropagationStopped()) {
            return;
        }
        if (event.keyCode === 17 || event.keyCode === 224 || event.keyCode === 91 || event.keyCode === 93) {
            //when CTRL or its equivalent is pressed and cell is edited, don't prepare selectable text in textarea
            event.stopImmediatePropagation();
            return;
        }

        var target = event.target;

        switch (event.keyCode) {
            case keyCodes.ARROW_RIGHT:
                if (Handsontable.Dom.getCaretPosition(target) !== target.value.length) {
                    event.stopImmediatePropagation();
                } else {
                    that.$textarea.select2('close');
                }
                break;

            case keyCodes.ARROW_LEFT:
                if (Handsontable.Dom.getCaretPosition(target) !== 0) {
                    event.stopImmediatePropagation();
                } else {
                    that.$textarea.select2('close');
                }
                break;

            case keyCodes.ENTER:
                var selected = that.instance.getSelected();
                var isMultipleSelection = !(selected[0] === selected[2] && selected[1] === selected[3]);
                if ((ctrlDown && !isMultipleSelection) || event.altKey) { //if ctrl+enter or alt+enter, add new line
                    if (that.isOpened()) {
                        that.val(that.val() + '\n');
                        that.focus();
                    } else {
                        that.beginEditing(that.originalValue + '\n')
                    }
                    event.stopImmediatePropagation();
                }
                event.preventDefault(); //don't add newline to field
                break;

            case keyCodes.A:
            case keyCodes.X:
            case keyCodes.C:
            case keyCodes.V:
                if (ctrlDown) {
                    event.stopImmediatePropagation(); //CTRL+A, CTRL+C, CTRL+V, CTRL+X should only work locally when cell is edited (not in table context)
                }
                break;

            case keyCodes.BACKSPACE:
            case keyCodes.DELETE:
            case keyCodes.HOME:
            case keyCodes.END:
                event.stopImmediatePropagation(); //backspace, delete, home, end should only work locally when cell is edited (not in table context)
                break;
        }

    };

    Select2Editor.prototype.open = function (keyboardEvent) {
        this.refreshDimensions();
        this.textareaParentStyle.display = 'block';
        this.instance.addHook('beforeKeyDown', onBeforeKeyDownSelect2);

        this.$textarea.css({
            height: $(this.TD).height() + 4,
            'min-width': $(this.TD).outerWidth() - 4
        });

        //display the list
        this.$textarea.show();

        var self = this;
        this.$textarea.select2(this.options)
            .on('change', onSelect2Changed.bind(this))
            .on('select2-close', onSelect2Closed.bind(this));

        self.$textarea.select2('open');

        // Pushes initial character entered into the search field, if available
        if (keyboardEvent && keyboardEvent.keyCode) {
            var key = keyboardEvent.keyCode;
            var keyText = (String.fromCharCode((96 <= key && key <= 105) ? key - 48 : key)).toLowerCase();
            self.$textarea.select2('search', keyText);
        }
    };

    Select2Editor.prototype.init = function () {
        Handsontable.editors.TextEditor.prototype.init.apply(this, arguments);
    };

    Select2Editor.prototype.close = function () {
        this.instance.listen();
        this.instance.removeHook('beforeKeyDown', onBeforeKeyDownSelect2);
        this.$textarea.off();
        this.$textarea.hide();
        Handsontable.editors.TextEditor.prototype.close.apply(this, arguments);
    };

    Select2Editor.prototype.val = function (value) {
        if (typeof value == 'undefined') {
            return this.$textarea.val();
        } else {
            this.$textarea.val(value);
        }
    };
    Select2Editor.prototype.getValue = function () {
        var _data = this.$textarea.select2('data');
        if (Array.isArray(_data) && _data.length)
            return String.format("{0};#{1}", _data[0].id, _data[0].text);
        else
            return '';
    }
    Select2Editor.prototype.setValue = function (newValue) {
        this.$textarea.val(!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(newValue) ?
            newValue.split(';#')[0] : '-1');
    }

    Select2Editor.prototype.focus = function () {

        this.instance.listen();

        // DO NOT CALL THE BASE TEXTEDITOR FOCUS METHOD HERE, IT CAN MAKE THIS EDITOR BEHAVE POORLY AND HAS NO PURPOSE WITHIN THE CONTEXT OF THIS EDITOR
        //Handsontable.editors.TextEditor.prototype.focus.apply(this, arguments);
    };

    Select2Editor.prototype.beginEditing = function (initialValue) {
        var onBeginEditing = this.instance.getSettings().onBeginEditing;
        if (onBeginEditing && onBeginEditing() === false) {
            return;
        }

        // init default value
        Handsontable.Dom.empty(this.TEXTAREA);
        var origVal = typeof initialValue == 'string' ? initialValue : this.originalValue;
        if (origVal) {
            var parts = origVal.split(';#');
            var option = new Option(parts[1], parts[0], true, true);
            this.$textarea.append(option);
        }

        Handsontable.editors.TextEditor.prototype.beginEditing.apply(this, arguments);
    };

    Select2Editor.prototype.finishEditing = function (isCancelled, ctrlDown) {
        this.instance.listen();
        return Handsontable.editors.TextEditor.prototype.finishEditing.apply(this, arguments);
    };

    Handsontable.editors.Select2Editor = Select2Editor;
    Handsontable.editors.registerEditor('select2', Select2Editor);

    //-----------------------
    // ------ CHECKLIST -----
    //-----------------------
    var ChecklistEditor = Handsontable.editors.TextEditor.prototype.extend();

    ChecklistEditor.prototype.prepare = function (row, col, prop, td, originalValue, cellProperties) {

        Handsontable.editors.TextEditor.prototype.prepare.apply(this, arguments);

        Handsontable.Dom.empty(this.ChecklistDIV);

        if (cellProperties.selectOptions != null) {
            for (var i = 0; i < cellProperties.selectOptions.length; i++) {
                var chElem = document.createElement('input');
                chElem.type = 'checkbox';
                chElem.value = cellProperties.selectOptions[i];
                var spanElem = document.createElement('span');
                Handsontable.Dom.fastInnerHTML(spanElem, chElem.value);
                var lblElem = document.createElement('label');
                Handsontable.Dom.addClass(lblElem, 'checkboxlist-row');

                lblElem.appendChild(chElem);
                lblElem.appendChild(spanElem);
                this.ChecklistDIV.appendChild(lblElem);
            }
        }
    };

    var onBeforeKeyDownCheckList = function (event) {
        if (event != null && event.isImmediatePropagationEnabled == null) {
            event.stopImmediatePropagation = function () {
                this.isImmediatePropagationEnabled = false;
                this.cancelBubble = true;
            };
            event.isImmediatePropagationEnabled = true;
            event.isImmediatePropagationStopped = function () {
                return !this.isImmediatePropagationEnabled;
            };
        }
        if (event.isImmediatePropagationStopped()) {
            return;
        }
        var editor = this.getActiveEditor();
        var innerHOT = editor.htEditor.getInstance();
        var rowToSelect;
        if (event.keyCode == helper.keyCode.ARROW_DOWN) {
            if (!innerHOT.getSelected()) {
                rowToSelect = 0;
            } else {
                var selectedRow = innerHOT.getSelected()[0];
                var lastRow = innerHOT.countRows() - 1;
                rowToSelect = Math.min(lastRow, selectedRow + 1);
            }
        } else if (event.keyCode == helper.keyCode.ARROW_UP) {
            if (innerHOT.getSelected()) {
                var selectedRow = innerHOT.getSelected()[0];
                rowToSelect = selectedRow - 1;
            }
        }
        if (rowToSelect !== void 0) {
            if (rowToSelect < 0) {
                innerHOT.deselectCell();
            } else {
                innerHOT.selectCell(rowToSelect, 0);
            }
            if (innerHOT.getData().length) {
                event.preventDefault();
                event.stopImmediatePropagation();
                editor.instance.listen();
                editor.TEXTAREA.focus();
            }
        }
    };

    var onChecklistSubmit = function () {
        this.close();
        this.finishEditing();
    };

    ChecklistEditor.prototype.createElements = function () {
        this.$body = $(document.body);

        this.TEXTAREA = document.createElement('input');
        this.TEXTAREA.type = 'text';
        Handsontable.Dom.addClass(this.TEXTAREA, 'handsontableInput');
        this.textareaStyle = this.TEXTAREA.style;
        this.textareaStyle.width = 0;
        this.textareaStyle.height = 0;

        this.ChecklistDIV = document.createElement('DIV');

        this.SumbitButton = document.createElement('input');
        this.SumbitButton.value = 'ok';
        this.SumbitButton.type = 'button';
        Handsontable.Dom.addClass(this.SumbitButton, 'checklistBtn');
        var self = this;
        jQuery(this.SumbitButton).on('click', onChecklistSubmit.bind(this));

        this.TEXTAREA_PARENT = document.createElement('DIV');
        Handsontable.Dom.addClass(this.TEXTAREA_PARENT, 'handsontableInputHolder');
        Handsontable.Dom.addClass(this.TEXTAREA_PARENT, 'checklistHolder');

        this.textareaParentStyle = this.TEXTAREA_PARENT.style;
        this.textareaParentStyle.top = 0;
        this.textareaParentStyle.left = 0;
        this.textareaParentStyle.display = 'none';

        this.TEXTAREA_PARENT.appendChild(this.ChecklistDIV);
        this.TEXTAREA_PARENT.appendChild(this.SumbitButton);

        this.instance.rootElement.appendChild(this.TEXTAREA_PARENT);

        var that = this;
        this.instance._registerTimeout(setTimeout(function () {
            that.refreshDimensions();
        }, 0));
    };

    ChecklistEditor.prototype.open = function (keyboardEvent) {
        this.instance.addHook('beforeKeyDown', onBeforeKeyDownCheckList);
        this.refreshDimensions();
        this.textareaParentStyle.display = 'block';
    };

    ChecklistEditor.prototype.init = function () {
        Handsontable.editors.TextEditor.prototype.init.apply(this, arguments);
    };

    ChecklistEditor.prototype.close = function () {
        this.instance.removeHook('beforeKeyDown', onBeforeKeyDownCheckList);
        this.instance.listen();
        this.textareaParentStyle.display = 'none';
        Handsontable.editors.TextEditor.prototype.close.apply(this, arguments);
    };

    ChecklistEditor.prototype.getValue = function () {
        return jQuery(this.ChecklistDIV).find('input:checked').map(function () {
            return jQuery(this).val();
        }).get().join(', ');
    }
    ChecklistEditor.prototype.setValue = function (newValue) {
        if (!SP.ScriptHelpers.isNullOrUndefinedOrEmpty(newValue)) {
            var vals = newValue.split(', ');
            jQuery(this.ChecklistDIV).find('input').filter(function () {
                return jQuery.inArray(jQuery(this).val(), vals) >= 0;
            }).prop('checked', true);
        }
    }

    ChecklistEditor.prototype.focus = function () {
        this.instance.listen();

        //Handsontable.editors.TextEditor.prototype.focus.apply(this, arguments);
    };

    ChecklistEditor.prototype.beginEditing = function (initialValue) {
        var onBeginEditing = this.instance.getSettings().onBeginEditing;
        if (onBeginEditing && onBeginEditing() === false) {
            return;
        }

        Handsontable.editors.TextEditor.prototype.beginEditing.apply(this, arguments);
    };

    ChecklistEditor.prototype.finishEditing = function (isCancelled, ctrlDown) {
        this.instance.listen();
        return Handsontable.editors.TextEditor.prototype.finishEditing.apply(this, arguments);
    };

    Handsontable.editors.ChecklistEditor = ChecklistEditor;
    Handsontable.editors.registerEditor('checklist', ChecklistEditor);

    //---------------------------
    // ------- UserOrGroup ------
    //---------------------------
    var UserEditor = Handsontable.editors.TextEditor.prototype.extend();

    UserEditor.prototype.prepare = function (row, col, prop, td, originalValue, cellProperties) {
        var $instance = this;
        Handsontable.editors.TextEditor.prototype.prepare.apply(this, arguments);

        var dialogUrl = String.format("{0}?MultiSelect={1}&CustomProperty={2}&DialogImage={3}&PickerDialogType={4}&ForceClaims={5}&DisableClaims={6}&EnabledClaimProviders={7}&EntitySeparator={8}",
                Roster.Common.pathCombine(_spPageContextInfo.webServerRelativeUrl, '/_layouts/15/Picker.aspx'),
                cellProperties.userLookupOptions.MultiSelect,
                cellProperties.userLookupOptions.CustomProperty,
                cellProperties.userLookupOptions.DialogImage,
                cellProperties.userLookupOptions.PickerDialogType,
                cellProperties.userLookupOptions.ForceClaims,
                cellProperties.userLookupOptions.DisableClaims,
                cellProperties.userLookupOptions.EnabledClaimProviders,
                cellProperties.userLookupOptions.EntitySeparator
            );
        this.options = {
            _fieldId: cellProperties.userLookupOptions.MetadataId,
            width: 575,
            height: 500,
            resizeable: true,
            url: dialogUrl,
            dialogReturnValueCallback: Function.createDelegate(this, OnAddressBookDismissed)
        };
    };

    UserEditor.prototype.createElements = function () {
        this.$body = $(document.body);

        this.TEXTAREA = document.createElement('input');
        this.TEXTAREA.setAttribute('type', 'text');
        this.TEXTAREA.setAttribute('disabled', 'disabled');
        this.TEXTAREA.className = 'handsontableInput';
        this.$textarea = $(this.TEXTAREA);
        
        this.TEXTAREA_PARENT = document.createElement('DIV');
        Handsontable.Dom.addClass(this.TEXTAREA_PARENT, 'handsontableInputHolder');

        this.textareaParentStyle = this.TEXTAREA_PARENT.style;
        this.textareaParentStyle.top = 0;
        this.textareaParentStyle.left = 0;
        this.textareaParentStyle.display = 'none';

        this.TEXTAREA_PARENT.appendChild(this.TEXTAREA);

        this.instance.rootElement.appendChild(this.TEXTAREA_PARENT);

        var that = this;
        this.instance._registerTimeout(setTimeout(function () {
            that.refreshDimensions();
        }, 0));
    };

    var OnAddressBookDismissed = function (dialogResult, xmlText) {
        var that = this;
        if (xmlText != null) {
            Roster.Common.EnsureUser(this.options._fieldId, xmlText, function (data) {
                that.$textarea.val(String.format("{0};#{1}", data.d.RosterLookupId, data.d.DisplayText));
            });
        }

        this.close();
        this.finishEditing();
    };
    var onBeforeKeyDownUser = function (event) {
        var instance = this;
        var that = instance.getActiveEditor();

        var keyCodes = Handsontable.helper.keyCode;
        var ctrlDown = (event.ctrlKey || event.metaKey) && !event.altKey; //catch CTRL but not right ALT (which in some systems triggers ALT+CTRL)

        //Process only events that have been fired in the editor
        if (!$(event.target).hasClass('select2-input') || event.isImmediatePropagationStopped()) {
            return;
        }
        if (event.keyCode === 17 || event.keyCode === 224 || event.keyCode === 91 || event.keyCode === 93) {
            //when CTRL or its equivalent is pressed and cell is edited, don't prepare selectable text in textarea
            event.stopImmediatePropagation();
            return;
        }

        var target = event.target;

        switch (event.keyCode) {
            case keyCodes.ARROW_RIGHT:
                if (Handsontable.Dom.getCaretPosition(target) !== target.value.length) {
                    event.stopImmediatePropagation();
                } else {
                    //that.$textarea.select2('close');
                }
                break;

            case keyCodes.ARROW_LEFT:
                if (Handsontable.Dom.getCaretPosition(target) !== 0) {
                    event.stopImmediatePropagation();
                } else {
                    //that.$textarea.select2('close');
                }
                break;

            case keyCodes.ENTER:
                var selected = that.instance.getSelected();
                var isMultipleSelection = !(selected[0] === selected[2] && selected[1] === selected[3]);
                if ((ctrlDown && !isMultipleSelection) || event.altKey) { //if ctrl+enter or alt+enter, add new line
                    if (that.isOpened()) {
                        that.val(that.val() + '\n');
                        that.focus();
                    } else {
                        that.beginEditing(that.originalValue + '\n')
                    }
                    event.stopImmediatePropagation();
                }
                event.preventDefault(); //don't add newline to field
                break;

            case keyCodes.A:
            case keyCodes.X:
            case keyCodes.C:
            case keyCodes.V:
                if (ctrlDown) {
                    event.stopImmediatePropagation(); //CTRL+A, CTRL+C, CTRL+V, CTRL+X should only work locally when cell is edited (not in table context)
                }
                break;

            case keyCodes.BACKSPACE:
            case keyCodes.DELETE:
            case keyCodes.HOME:
            case keyCodes.END:
                event.stopImmediatePropagation(); //backspace, delete, home, end should only work locally when cell is edited (not in table context)
                break;
        }

    };

    UserEditor.prototype.open = function (keyboardEvent) {
        this.refreshDimensions();
        this.textareaParentStyle.display = 'block';
        //this.instance.addHook('beforeKeyDown', onBeforeKeyDownUser);

        this.$textarea.css({
            height: $(this.TD).height() + 4,
            'min-width': $(this.TD).outerWidth() - 4
        });

        EnsureScriptParams("SP.UI.Dialog.js", "SP.UI.ModalDialog.showModalDialog", this.options);
    };

    UserEditor.prototype.init = function () {
        Handsontable.editors.TextEditor.prototype.init.apply(this, arguments);
    };

    UserEditor.prototype.close = function () {
        this.instance.listen();
        this.instance.removeHook('beforeKeyDown', onBeforeKeyDownSelect2);
        this.textareaParentStyle.display = 'none';
        Handsontable.editors.TextEditor.prototype.close.apply(this, arguments);
    };

    UserEditor.prototype.val = function (value) {
        if (typeof value == 'undefined') {
            return this.$textarea.val();
        } else {
            this.$textarea.val(value);
        }
    };
    UserEditor.prototype.getValue = function () {
        var newVal = this.$textarea.val();
        if (newVal) {
            return this.$textarea.val();
        } else {
            return this.$textarea.val("-1;#");
        }
        
    }
    UserEditor.prototype.setValue = function (newValue) {
        if (newValue != "-1;#") {
            this.$textarea.val(newValue);
        } else {
            this.$textarea.val('');
        }
    }

    UserEditor.prototype.focus = function () {

        this.instance.listen();

        // DO NOT CALL THE BASE TEXTEDITOR FOCUS METHOD HERE, IT CAN MAKE THIS EDITOR BEHAVE POORLY AND HAS NO PURPOSE WITHIN THE CONTEXT OF THIS EDITOR
        //Handsontable.editors.TextEditor.prototype.focus.apply(this, arguments);
    };

    UserEditor.prototype.beginEditing = function (initialValue) {
        var onBeginEditing = this.instance.getSettings().onBeginEditing;
        if (onBeginEditing && onBeginEditing() === false) {
            return;
        }

        // init default value
        Handsontable.Dom.empty(this.TEXTAREA);
        var origVal = typeof initialValue == 'string' ? initialValue : this.originalValue;
        if (origVal && origVal != "-1;#") {
            this.$textarea.val(origVal);
        }

        Handsontable.editors.TextEditor.prototype.beginEditing.apply(this, arguments);
    };

    UserEditor.prototype.finishEditing = function (isCancelled, ctrlDown) {
        this.instance.listen();
        return Handsontable.editors.TextEditor.prototype.finishEditing.apply(this, arguments);
    };

    Handsontable.editors.UserEditor = UserEditor;
    Handsontable.editors.registerEditor('userorgroup', UserEditor);

})(Handsontable);