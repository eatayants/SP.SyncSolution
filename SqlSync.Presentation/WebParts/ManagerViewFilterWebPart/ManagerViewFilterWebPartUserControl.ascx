<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManagerViewFilterWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.ManagerViewFilterWebPart.ManagerViewFilterWebPartUserControl" %>

<SharePoint:ScriptLink Name="sp.js" LoadAfterUI="true" OnDemand="false" Localizable="false" runat="server" />
<SharePoint:ScriptLink Name="CUI.js" LoadAfterUI="true" OnDemand="false" Localizable="false" runat="server" />

<script type="text/javascript">

    SP.SOD.executeOrDelayUntilEventNotified(function () {
        var _errorCtrl = document.getElementById('<%= txtErrorMsg.ClientID %>');
        var _notifyCtrl = document.getElementById('<%= txtNotification.ClientID %>');
        var _errorMsg = _errorCtrl.value;
        var _notifyTxt = _notifyCtrl.value;
        _errorCtrl.value = '';
        _notifyCtrl.value = '';
        
        if (_errorMsg) {
            ShowErrorMsg(_errorMsg);
        } else if (_notifyTxt) {
            ShowNotification(_notifyTxt);
        }
    }, "sp.bodyloaded");

    var statusID;
    function ShowErrorMsg(msg) {
        statusID = SP.UI.Status.addStatus("Error:", msg);
        SP.UI.Status.setStatusPriColor(statusID, 'red');
        setTimeout(function () { SP.UI.Status.removeStatus(statusID); }, 60000);
    }
    function ShowNotification(msg) {
        try
        {
            SP.UI.Notify.addNotification(msg, false);
        } catch (e) {
            okStatusID = SP.UI.Status.addStatus("Status:", msg);
            SP.UI.Status.setStatusPriColor(okStatusID, 'green');
            setTimeout(function () { SP.UI.Status.removeStatus(okStatusID); }, 5000);
        }
    }

    ExecuteOrDelayUntilScriptLoaded(function () {

        CUI.Ribbon.prototype.$1q_0_old = CUI.Ribbon.prototype.$1q_0;
        CUI.Ribbon.prototype.$1q_0 = function () {
            this.$1q_0_old();
            TabChangedCustomEvent(this.$9_3);
        };

    }, 'cui.js');
    ExecuteOrDelayUntilScriptLoaded(function () {
        var pm = SP.Ribbon.PageManager.get_instance();
        pm.add_ribbonInited(function () {
            TabChangedCustomEvent(SP.Ribbon.PageManager.get_instance().get_ribbon().$9_3);
        });
    }, 'sp.ribbon.js');

    function TabChangedCustomEvent(tab) {
        if (!CUI.ScriptUtility.isNullOrUndefined(tab)) {
            ServerSideHideFunc(tab.$1W_0, tab.$2s_0); // $1W_0 - Tab Title, $2s_0 - Tab html element
        }
    }
    function HideTabAction(tabElem, actionTitle) {
        jQuery(tabElem).find('span').filter(function () {
            var _txt = jQuery(this).html().toLowerCase();
            _txt = _txt.replace('<br>', ' ').replace('<br/>', ' ').replace('<br />', ' ');
            return _txt == actionTitle.toLowerCase();
        }).closest('a').hide();
    }

</script>

<div style="height: 0px; display: none;">
    <asp:HiddenField ID="txtNotification" runat="server" />
    <asp:HiddenField ID="txtErrorMsg" runat="server" />
    <asp:Button ID="btnBulkApprove" runat="server" Text="Bulk Approve" OnClick="btnBulkApprove_Click" UseSubmitBehavior="false" />
    <asp:Button ID="btnBulkSendToNAV" runat="server" Text="Send to NAV" OnClick="btnBulkSendToNAV_Click" UseSubmitBehavior="false" />
    <asp:Button ID="btnBulkEndorse" runat="server" Text="Bulk Endorse" OnClick="btnBulkEndorse_Click" UseSubmitBehavior="false" />
</div>