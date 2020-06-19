var cuaModel;

var CustomActionButton = function (data) {
    var self = this;

    if (data) {
        self.labelText = ko.observable(data.LabelText);
        self.description = ko.observable(data.Description);
        self.imageUrl = ko.observable(data.ImageUrl);
        self.command = ko.observable(data.Command);
        self.accessGroupArr = (data.AccessGroup) ? ko.observableArray(data.AccessGroup.split(';')) : ko.observableArray([]);
    } else {
        self.labelText = ko.observable('');
        self.description = ko.observable('');
        self.imageUrl = ko.observable('');
        self.command = ko.observable('');
        self.accessGroupArr = ko.observable('');
    }

    this.accessGroup = ko.computed(function () {
        var grArr = this.accessGroupArr();
        return grArr ? this.accessGroupArr().join(';') : '';
    }, this);
}

var CustomActionsModel = function () {
    var self = this;

    self.CustomActions = ko.observableArray([]);

    self.addAction = function () {
        self.CustomActions.push(new CustomActionButton());
    }
    self.removeAction = function (row) {
        self.CustomActions.remove(row);
    }

    // init data on PageLoad/PostBack
    if (window.hidCustomActions_Value !== undefined) {
        for (var i = 0; i < hidCustomActions_Value.length; i++) {
            self.CustomActions.push(new CustomActionButton(hidCustomActions_Value[i]));
        }
    }
}

function SPSODAction(sodScripts, onLoadAction) {
    if (SP.SOD.loadMultiple) {
        for (var x = 0; x < sodScripts.length; x++) {
            if (!_v_dictSod[sodScripts[x]]) {
                SP.SOD.registerSod(sodScripts[x], '/_layouts/15/' + sodScripts[x]);
            }
        }
        SP.SOD.loadMultiple(sodScripts, onLoadAction);
    } else
        ExecuteOrDelayUntilScriptLoaded(onLoadAction, sodScripts[0]);
}

ko.bindingHandlers.clientPeoplePicker = {
    currentId: 0,
    init: function (element, valueAccessor) {
        var obs = valueAccessor();
        if (!ko.isObservable(obs)) {
            throw "clientPeoplePicker binding requires an observable";
        }

        var currentId = ko.bindingHandlers.clientPeoplePicker.currentId++;
        var currentElemId = "ClientPeoplePicker" + currentId;
        element.setAttribute("id", currentElemId);
        obs._peoplePickerId = currentElemId + "_TopSpan";
        ko.bindingHandlers.clientPeoplePicker.
            initPeoplePicker(currentElemId).done(function (picker) {
                picker.OnValueChangedClientScript = function (elementId, userInfo) {
                    var temp = new Array();
                    for (var x = 0; x < userInfo.length; x++) {
                        temp[temp.length] = userInfo[x].Key;
                    }
                    obs(temp);
                };
                ko.bindingHandlers.clientPeoplePicker.update(element, valueAccessor);
            });
    },
    update: function (element, valueAccessor) {
        var obs = valueAccessor();
        if (!ko.isObservable(obs)) {
            throw "clientPeoplePicker binding requires an observable array";
        }
        if (typeof SPClientPeoplePicker === 'undefined')
            return;

        var peoplePicker =
            SPClientPeoplePicker.SPClientPeoplePickerDict[obs._peoplePickerId];
        if (peoplePicker) {
            var keys = peoplePicker.GetAllUserKeys();
            keys = keys.length > 0 ? keys.split(";") : [];
            var updateKeys = obs() && obs().length ? obs() : [];
            var newKeys = new Array();
            for (var x = 0; x < updateKeys.length; x++) {
                for (var y = 0; y < keys.length && updateKeys[x] != keys[y]; y++) { }
                if (y >= keys.length) {
                    newKeys[newKeys.length] = updateKeys[x];
                }
            }

            if (newKeys.length > 0) {
                peoplePicker.AddUserKeys(newKeys.join(";"));
            }
        }
    },
    initPeoplePicker: function (elementId) {
        var schema = {};
        schema['PrincipalAccountType'] = 'SPGroup';
        schema['SearchPrincipalSource'] = 15;
        schema['ResolvePrincipalSource'] = 15;
        schema['AllowMultipleValues'] = true;
        schema['MaximumEntitySuggestions'] = 50;
        schema['Width'] = '300px';

        var dfd = jQuery.Deferred();

        SPSODAction(["sp.js", "clienttemplates.js", "clientforms.js", "clientpeoplepicker.js", "autofill.js"], function () {
                SPClientPeoplePicker_InitStandaloneControlWrapper(elementId, null, schema);
                dfd.resolve(
                    SPClientPeoplePicker.SPClientPeoplePickerDict[elementId + "_TopSpan"]);
            });

        return dfd.promise();
    }
};

cuaModel = new CustomActionsModel();
ko.applyBindings(cuaModel);