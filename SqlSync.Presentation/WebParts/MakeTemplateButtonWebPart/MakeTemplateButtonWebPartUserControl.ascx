<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MakeTemplateButtonWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.MakeTemplateButtonWebPart.MakeTemplateButtonWebPartUserControl" %>

<script type="text/javascript">
    window.jQuery || document.write("<script src='/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js'>\x3C/script>")
</script>

<asp:Panel ID="createTemplatePanel" runat="server">

    <table border="0" cellpadding="0" cellspacing="0" class="ms-formtable">
        <tr>
            <td class="ms-formlabel" style="width: 113px;">&nbsp;</td>
            <td class="ms-formbody">
                <input type="button" id="btnMakeTemplateRoster" class="ms-ButtonHeightWidth" value="Save as template" onclick="javascript: SaveAsTemplate();" /> 
            </td>
        </tr>
    </table>

</asp:Panel>

<script type="text/javascript">

    (function () {
        var makeTemplBtn = document.getElementById("btnMakeTemplateRoster");
        var inDesignMode = document.forms[MSOWebPartPageFormName].MSOLayout_InDesignMode.value;

        if (inDesignMode != "1" && makeTemplBtn) {
            var toolbarRow = jQuery('.ms-formtoolbar:last');
            if (toolbarRow.length) {
                jQuery(makeTemplBtn).appendTo(toolbarRow.find('td:last'))
            }
        }
    }());

    function SaveAsTemplate() {
        Roster.Common.SaveMasterAsTemplate('DataService.svc/SaveMasterAsTemplate', GetUrlKeyValue('ID'));
    }

</script>