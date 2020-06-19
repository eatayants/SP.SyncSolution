<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RosterLockPage.aspx.cs" Inherits="Roster.Presentation.Layouts.RosterLockPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

    <style type="text/css">

        .main-tbl td { width: 150px; }

    </style>

    <script type="text/javascript">
        function ShowHideGroup(group, img) {
            if ((group == null) || browseris.mac)
                return;
            if (group.style.display != "none") {
                group.style.display = "none";
                img.src = "/_layouts/15/images/plus.gif?rev=23";
            }
            else {
                group.style.display = "";
                img.src = "/_layouts/15/images/minus.gif?rev=23";
            }
        }
    </script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">


    <table border="0" cellpadding="4" class="main-tbl">
        <tr>
            <td colspan="2">
                <label>Bulk lock Working Roster shifts that fall between</label>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px;" valign="top">Start date:</td>
            <td>
                <SharePoint:DateTimeControl ID="dtStart" runat="server" DateOnly="false" AutoPostBack="false" />
                <asp:RequiredFieldValidator ID="rfvStartDate" ControlToValidate="dtStart$dtStartDate" runat="server" Display="Dynamic"
                    ErrorMessage="Required field" >
                </asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px;" valign="top">End date:</td>
            <td>
                <SharePoint:DateTimeControl ID="dtEnd" runat="server" DateOnly="false" AutoPostBack="false" />
                <asp:RequiredFieldValidator ID="rfvEndDate" ControlToValidate="dtEnd$dtEndDate" runat="server" Display="Dynamic"
                    ErrorMessage="Required field" >
                </asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px;" valign="top">Reason:</td>
            <td><asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" Columns="44"></asp:TextBox></td>
        </tr>
        <tr>
            <td style="padding-left: 10px;" colspan="2">
                <h3 class="ms-standardheader">
                    <a onclick='javascript:ShowHideGroup(document.getElementById("advSettingsRow"),document.getElementById("imgViewAdvSet"));return false;' href="javascript:ShowHideGroup()">
					<img id="imgViewAdvSet" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0" />Advanced settings</a>
				</h3>
            </td>
        </tr>
        <tr style="display: none" id="advSettingsRow">
            <td colspan="2">
                <table border="0" cellpadding="2">
                    <tr>
                        <td style="padding-left: 6px;" valign="top">Stored Procedure name:</td>
                        <td><asp:TextBox ID="txtStoredProcName" runat="server" Text="dbo.RorterEvents_WorkingLock" Columns="44"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td style="padding-left: 6px;" valign="top">Reset rights:</td>
                        <td><asp:CheckBox ID="chNeedResetRights" runat="server" Checked="true" Text="Reset permissions for selected rosters?" /></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <table border="0" style="width: 160px;">
                    <tr>
                        <td><asp:Button ID="btnLock" runat="server" Text="Lock" OnClick="btnLock_Click" /></td>
                        <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack" /></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Panel ID="ErrorHolder" runat="server" ForeColor="Red"></asp:Panel>
            </td>
        </tr>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Bulk lock
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Bulk lock Working Roster shifts
</asp:Content>
