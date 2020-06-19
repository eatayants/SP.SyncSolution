<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManagePermissionsWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.ManagePermissionsWebPart.ManagePermissionsWebPartUserControl" %>

<SharePoint:ScriptLink name="clienttemplates.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="clientforms.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="clientpeoplepicker.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="autofill.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="sp.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="sp.runtime.js" runat="server" LoadAfterUI="true" Localizable="false" />
<SharePoint:ScriptLink name="sp.core.js" runat="server" LoadAfterUI="true" Localizable="false" />

<script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/knockout-3.1.0.js"></script>

<asp:Panel runat="server" ID="statusPanel" Visible="false">
	<div class="ms-status-yellow" id="pageStatusBar" data-bind="visible: !canBeSaved()">
        <span class="ms-status-status" role="alert">
            <span class="ms-status-iconSpan">
                <img class="ms-status-iconImg" src="/_layouts/15/images/spcommon.png" alt="" />
            </span>
            <span class="ms-status-body" id="status_1_body">
                This element inherits permissions from its parent list. <a href="#" data-bind="click: stopInheriting"> Stop Inheriting Permisions?</a>
            </span>
            <br />
        </span>
	</div>
</asp:Panel>

<table border="0">
    <thead>
        <tr>
            <th align="left">Name</th>
            <th style="width: 70px;" align="left">Type</th>
            <th align="left">Permission Level</th>
            <th style="width: 30px;">&nbsp;</th>
        </tr>
    </thead>
    <tbody data-bind="foreach: Permissions">
        <tr>
            <td valign="top">
                <div data-bind="uniqueId: true, koPeoplePicker: key"></div>
            </td>
            <td valign="top" data-bind="text: principalType">
            </td>
            <td valign="top">
                <select data-bind="value: rights">
                    <option value="1">Read</option>
                    <option value="2">Write</option>
                    <option value="4">Delete</option>
                    <option value="8">List</option>
                    <option value="16">Full Control</option>
                </select>
            </td>
            <td align="middle">
                <a title="Remove" href="#" data-bind='click: $parent.removePermission, visible: $parent.canBeSaved'>
                    <img class="ms-advsrch-img" src="/_layouts/images/IMNDND.PNG" alt="" />
                </a>
            </td>
        </tr>
    </tbody>
    <tfoot>
        <tr><td class="ms-addnew" colspan="10" data-bind="visible: canBeSaved">
            <span style="POSITION: relative; WIDTH: 10px; DISPLAY: inline-block; HEIGHT: 10px; OVERFLOW: hidden" class="s4-clust">
                <img style="POSITION: absolute; TOP: -128px !important; LEFT: 0px !important" alt="" src="/_layouts/images/fgimg.png" />
            </span>&nbsp;<a title="Add rights" href="#" data-bind='click: addPermission' class="ms-addnew">Add rights</a>
        </td></tr>
        <tr>
            <td colspan="10" style="padding-top: 15px;" data-bind="visible: canBeSaved" align="right">
                <input type="hidden" id="hidDataInfo" runat="server" data-bind="value: ko.toJSON(ppModel.Permissions(), null, 1)" />
                <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
            </td>
        </tr>
        <tr>
            <td colspan="10">
                <asp:Panel ID="pnlErrorPanel" runat="server" ForeColor="Red"></asp:Panel>
            </td>
        </tr>
    </tfoot>
</table>

<!-- <textarea data-bind="value: ko.toJSON(ppModel, null, 1)" cols="80" rows="15"></textarea> -->

<script type="text/javascript">

    var ppModel;

    var ClientSidePrincipal = function (data) {
        var self = this;

        self.key = ko.observable(data ? data.key : '');
        self.id = ko.observable('0');
        self.principalType = ko.observable('');
        self.rights = ko.observable(data ? data.rights : '1');
    }
    ClientSidePrincipal.prototype.toJSON = function () {
        var copy = ko.toJS(this);
        delete copy.principalType;
        return copy;
    }

    var PermissionsFormModel = function () {
        var self = this;

        self.Permissions = ko.observableArray([]);
        self.canBeSaved = ko.observable(document.getElementById('<%= statusPanel.ClientID %>') == null);

        self.addPermission = function () {
            self.Permissions.push(new ClientSidePrincipal());
        }
        self.removePermission = function (row) {
            self.Permissions.remove(row);
        }
        self.stopInheriting = function () {
            if (confirm('You are about to create unique permissions for this element. Continue?')) {
                self.canBeSaved(true);
            }
        }

        // INIT
        var dataVal = document.getElementById('<%= hidDataInfo.ClientID %>').value;
        var data = dataVal ? JSON.parse(dataVal) : [];
        if (data.length) {
            for (var j = 0; j < data.length; j++) {
                self.Permissions.push(new ClientSidePrincipal({ key: data[j].key, rights: data[j].rights }));
            }
        } else {
            self.Permissions.push(new ClientSidePrincipal()); // add empty row
        }
    }
    PermissionsFormModel.prototype.toJSON = function () {
        var copy = ko.toJS(this);
        delete copy.canBeSaved;
        return copy;
    }

    ko.bindingHandlers.uniqueId = {
        init: function (element) {
            element.id = 'clientPeoplePicker' + (++ko.bindingHandlers.uniqueId.counter);
        },
        counter: 0
    };

    ko.bindingHandlers.koPeoplePicker = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var id_obs = bindingContext.$data.id;
            var type_obs = bindingContext.$data.principalType;

            var schema = {};
            schema['PrincipalAccountType'] = 'User,SPGroup,SecGroup'; //,DL
            schema['SearchPrincipalSource'] = 15;
            schema['ResolvePrincipalSource'] = 15;
            schema['ShowUserPresence'] = false;
            schema['AllowEmailAddresses'] = true;
            schema['AllowMultipleValues'] = false;
            schema['MaximumEntitySuggestions'] = 50;
            schema['Width'] = '280px';
            schema['Required'] = true;
            schema["OnUserResolvedClientScript"] = function (elemId, userKeys) {
                //  get reference of People Picker Control
                var pickerElement = SPClientPeoplePicker.SPClientPeoplePickerDict[elemId];
                var observable = valueAccessor();
                var ctrlVal = pickerElement.GetControlValueAsJSObject();
                if (ctrlVal.length) {
                    try {
                        observable(ctrlVal[0].Key);
                        id_obs(ctrlVal[0].EntityData.SPGroupID || 0);
                        type_obs(ctrlVal[0].EntityType || "ShP Group");
                        //console.log(JSON.stringify(ctrlVal[0]));
                    } catch (e) {}
                } else {
                    observable('');
                    id_obs('0');
                    type_obs('');
                }
            };

            //  Initialize the Control, MS enforces to pass the Element ID hence we need to provide ID to our element, no other options
            var options = allBindingsAccessor().schema || schema;
            this.SPClientPeoplePicker_InitStandaloneControlWrapper(element.id, null, options);

            //  Force to Ensure User
            var userValue = ko.utils.unwrapObservable(valueAccessor());
            var pickerControl = SPClientPeoplePicker.SPClientPeoplePickerDict[element.id + "_TopSpan"];
            pickerControl.AddUserKeys(userValue);
        }
    };

    SP.SOD.executeOrDelayUntilEventNotified(function () {
        // init model - apply binding
        ppModel = new PermissionsFormModel();
        ko.applyBindings(ppModel);

    }, "sp.bodyloaded");

</script>