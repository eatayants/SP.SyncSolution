<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DatabaseSettingsPage.aspx.cs" Inherits="SqlSync.SP.Layouts.DatabaseSettingsPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:PlaceHolder ID="pannelError" runat="server"></asp:PlaceHolder>
    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
        <tbody>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
                    <table border="0" cellspacing="0" cellpadding="1">
                        <tr>
                            <td height="28" class="ms-sectionheader" valign="top">
                                <h3 class="ms-standardheader">SqlSync Database Connection</h3>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-descriptiontext" id="onetidNewColumnDescription">
                                Type a server name, database and user credentials information you want to store.
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td>
                        </tr>
                    </table>
                </td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtServer">Server:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtServer" runat="server" MaxLength="255" class="ms-input" ToolTip="Server" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorServer" runat="server" ControlToValidate="txtServer" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtDatabase">Database:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtDatabase" runat="server" MaxLength="255" class="ms-input" ToolTip="Database" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorDatabase" runat="server" ControlToValidate="txtDatabase" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtUser">User:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtUser" runat="server" MaxLength="255" class="ms-input" ToolTip="User" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorUser" runat="server" ControlToValidate="txtUser" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtUser">Password:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" class="ms-input" ToolTip="Password" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
        </tbody>
        <tbody>
            <tr>
                <td>&nbsp;</td>
                <td align="right">
                    <table border="0">
                        <tr>
                            <td>
                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" />
                            </td>
                            <td>
                                <asp:Button ID="Button1" runat="server" CausesValidation="False" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClick="btnCancel_Click" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Database settings
</asp:Content>
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
Database settings
</asp:Content>