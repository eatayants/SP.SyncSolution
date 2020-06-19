<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TemplateSelectorWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.TemplateSelectorWebPart.TemplateSelectorWebPartUserControl" %>

<link href="/_layouts/15/Roster.Presentation/js/select2.min.css" type="text/css" rel="stylesheet" />
<script src="/_layouts/15/Roster.Presentation/js/underscore-min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/select2.min.js"></script>

<table border="0" cellpadding="0" cellspacing="0" class="ms-formtable">
    <tr>
        <td class="ms-formlabel" style="width: 113px;">Select template:</td>
        <td class="ms-formbody">
            <table border="0" cellpadding="0" cellspacing="0">
                <tr>
                    <td>
                        <select id="templateSelector"></select>
                        <asp:HiddenField ID="hidValueHolder" runat="server" />
                    </td>
                    <td style="padding-left: 10px">
                        <asp:ImageButton ID="btnCopyRosters" runat="server" ToolTip="Copy rosters from template"
                            OnClick="btnApplyTemplate_Click" ImageUrl="/_layouts/15/images/copy.gif" />
                    </td>
                </tr>
            </table>
        </td>
    </tr>
    <tr>
        <td class="ms-formlabel" style="width: 113px;">&nbsp;</td>
        <td class="ms-formbody">
            <asp:Panel ID="pnlErrorInfo" runat="server"></asp:Panel>
        </td>
    </tr>
</table>

<script type="text/javascript">

    SP.SOD.executeOrDelayUntilEventNotified(function () {
        Roster.Common.ListTemplates('#templateSelector', 'DataService.svc/ListTemplates', function (tmplId) {
            $('#' + '<%= hidValueHolder.ClientID %>').val(tmplId);
        });
    }, "sp.bodyloaded");

</script>