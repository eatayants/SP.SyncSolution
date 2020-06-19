<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AvailabilityButtonWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.AvailabilityButtonWebPart.AvailabilityButtonWebPartUserControl" %>

<table border="0" width="100%">
    <tr><td style="text-align: right">
        <asp:HiddenField ID="hidHashValue" runat="server" />
        <asp:HiddenField ID="hidFilterOper" runat="server" />
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClientClick="close_window(); return false;" />
        <asp:Button ID="btnViewAvailability" runat="server" Text="View Availability" CssClass="ms-ButtonHeightWidth"
            OnClientClick="return SaveUrlHashToServerVariable();" OnClick="btnViewAvailability_Click" />
    </td></tr>
    <tr><td style="text-align: left">
        <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
    </td></tr>
</table>

<script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/replace_link.js"></script>

<script type="text/javascript">

    function SaveUrlHashToServerVariable() {
        var currentHash = ajaxNavigate.get_hash();
        if (currentHash) {
            document.getElementById('<%=hidHashValue.ClientID %>').value = currentHash;
            return true;
        } else {
            return confirm('There are no filters applied to the List view. Are you sure what you want to continue?');
        }
    }

    function close_window() {
        if (confirm("Are you sure that you want to close this window?")) {
            window.close();
        }
    }

</script>