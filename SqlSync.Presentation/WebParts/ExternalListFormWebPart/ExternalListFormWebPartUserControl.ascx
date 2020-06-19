<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExternalListFormWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.ExternalListFormWebPart.ExternalListFormWebPartUserControl" %>

<link href="/_layouts/15/1033/styles/forms.css" type="text/css" rel="stylesheet" />
<link href="/_layouts/15/1033/styles/Themable/corev15.css" type="text/css" rel="stylesheet" />
<link href="/_layouts/15/Roster.Presentation/js/select2.min.css" type="text/css" rel="stylesheet" />
<script src="/_layouts/15/Roster.Presentation/js/underscore-min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/select2.min.js"></script>

<script type="text/javascript">
    function RosterPreSaveAction() {
        var result = true;
        if ("function" == typeof SPClientPeoplePicker && typeof SPClientPeoplePicker.SPClientPeoplePickerDict != "undefined") {
            for (pickerId in SPClientPeoplePicker.SPClientPeoplePickerDict) {
                var picker = SPClientPeoplePicker.SPClientPeoplePickerDict[pickerId];
                picker.Validate();
                if (picker.HasInputError) {
                    result = false;
                    break;
                }
            }
        }
        return result;
    }

    function RosterPreSaveItem() {
        if ("function" == typeof RosterPreSaveAction) {
            return RosterPreSaveAction();
        }
        return true;
    }
</script>

<style type="text/css">
    #recurCustomDiv {
        display: none !important;
    }
    .ms-dialog-nr body #s4-ribbonrow, .ms-dialog-nr body #globalNavBox {
        display: block !important;
    }
    #contentBox {
        margin-left: 0px !important;
    }
    .ms-webpartPage-root {
        border-spacing: 2px;
    }
    #sideNavBox {
        margin: 0px !important;
        width: 0px !important;
        display: none !important;
    }
</style>

<asp:Table runat="server" ID="FormTable" BorderWidth="0" CellPadding="0" CellSpacing="0" CssClass="ms-formtable">
    <asp:TableFooterRow CssClass="toolbar-bottom-row">
        <asp:TableCell CssClass="ms-toolbar" ColumnSpan="2">
            <table cellpadding="0" cellspacing="0" width="100%">
                <tr>
                    <td>
                        <asp:Table runat="server" ID="tblCreatedModified" CellPadding="0" CellSpacing="0"></asp:Table>
                    </td>
                    <td style="text-align: right">
                        <asp:Button ID="btnSave" runat="server" Text="Ok" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" />
                        <asp:Button ID="btnCancel" runat="server" CausesValidation ="False" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClick="btnCancel_Click" />
                    </td>
                </tr>
            </table>
        </asp:TableCell>
    </asp:TableFooterRow>
</asp:Table>

<asp:PlaceHolder ID="ErrorHolder" runat="server"></asp:PlaceHolder>