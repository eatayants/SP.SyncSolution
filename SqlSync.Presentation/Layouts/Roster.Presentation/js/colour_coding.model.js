var ccModel;

var StaticCondition = function (data) {
    var self = this;

    if (data) {
        self.Id = ko.observable(data.Id);
        self.camlOperator = ko.observable(data.camlOperator);
        self.filterValue = ko.observable(data.filterValue);
        self.color = ko.observable(data.color);
        self.fontColor = ko.observable(data.fontColor);
    } else {
        self.Id = ko.observable('');
        self.camlOperator = ko.observable('');
        self.filterValue = ko.observable('');
        self.color = ko.observable('');
        self.fontColor = ko.observable('');
    }
}
StaticCondition.prototype.toJSON = function () {
    var copy = ko.toJS(this);
    delete copy.__ko_mapping__;
    return copy;
}

var DynamicCondition = function (data) {
    var self = this;

    if (data) {
        self.camlQuery = ko.observable(data.camlQuery);
        self.color = ko.observable(data.color);
        self.fontColor = ko.observable(data.fontColor);
    } else {
        self.camlQuery = ko.observable('');
        self.color = ko.observable('');
        self.fontColor = ko.observable('');
    }
}
DynamicCondition.prototype.toJSON = function () {
    var copy = ko.toJS(this);
    delete copy.__ko_mapping__;
    return copy;
}

var ColourCodingFormModel = function () {
    var self = this;

    self.availableFields = window.global_availableFields;                // array loaded from server code
    self.availableFields_dynCol = window.global_availableFields_dynCol;  // array loaded from server code
    self.availableOperators = [{ displayName: "is equal to", camlOper: "Eq" }, { displayName: "is not equal to", camlOper: "Neq" },
                               { displayName: "begins with", camlOper: "BeginsWith" }, { displayName: "contains", camlOper: "Contains" },
                               { displayName: "is empty", camlOper: "IsNull" }, { displayName: "is not empty", camlOper: "IsNotNull" }];

    self.StaticConditions = ko.observableArray([]);

    self.DynamicConditions = ko.observableArray([]);
    self.dynMatchingField = ko.observable('');
    self.dynFilterFieldsColl = ko.observable('');

    self.addStaticCondition = function () {
        if (self.DynamicConditions().length > 0) {
            if (confirm("Dynamic colour-settings already exist. Do you want to clear Dynamic settings?")) {
                self.DynamicConditions.removeAll();
            } else {
                return false;
            }
        }
        self.StaticConditions.push(new StaticCondition());
    }
    self.removeStaticCondition = function (row) {
        self.StaticConditions.remove(row);
    }
    self.addDynamicCondition = function () {
        if (self.StaticConditions().length > 0) {
            if (confirm("Static colour-settings already exist. Do you want to clear Static settings?")) {
                self.StaticConditions.removeAll();
            } else {
                return false;
            }
        }
        self.DynamicConditions.push(new DynamicCondition());
    }
    self.removeDynamicCondition = function (row) {
        self.DynamicConditions.remove(row);
    }

    // init STATIC data on PageLoad/PostBack
    if (window.hidStaticConditions_Value !== undefined) {
        for (var i = 0; i < hidStaticConditions_Value.length; i++) {
            self.StaticConditions.push(new StaticCondition(hidStaticConditions_Value[i]));
        }
    }
    // init DYNAMIC data on PageLoad/PostBack
    if (window.hidDynamicConditions_Value !== undefined) {
        if (window.hidDynamicMatchingFld_Value !== undefined) {
            self.dynMatchingField(window.hidDynamicMatchingFld_Value);
        }
        if (window.hidDynamicFilterFields_Value !== undefined) {
            self.dynFilterFieldsColl(window.hidDynamicFilterFields_Value);
        }

        for (var i = 0; i < hidDynamicConditions_Value.length; i++) {
            self.DynamicConditions.push(new DynamicCondition(hidDynamicConditions_Value[i]));
        }
    }
}

ko.bindingHandlers.jscolor = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        new jscolor.color(element, { hash: true, pickerMode: 'HVS', imagePath: '/_layouts/15/images/Roster.Presentation/' });
    }
}

ccModel = new ColourCodingFormModel();
ko.applyBindings(ccModel);