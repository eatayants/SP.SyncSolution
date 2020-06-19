<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbListAddFieldToContentType.aspx.cs" Inherits="Roster.Presentation.Layouts.DbListAddFieldToContentType" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
				<table border="0" cellspacing="0" cellpadding="1">
                    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Select Columns</h3></td></tr>
					<tr><td class="ms-descriptiontext" id="onetidNewColumnDescription">Select from the list of available columns to add them to this content type.</td></tr>
					<tr><td><img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
				</table>
			</td>
            <td class="ms-authoringcontrols" valign="top">
                <label for="idColName">Select multiple columns using Ctrl button:</label><br />
                <asp:ListBox ID="listBoxOfColumns" runat="server" Rows="8" DataTextField="FieldName" DataValueField="Id" SelectionMode="Multiple"></asp:ListBox><br />
                <asp:RequiredFieldValidator ID="validatorColName" runat="server" ControlToValidate="listBoxOfColumns" ErrorMessage="You must specify a value for this required field."
                    Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
			</td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td><asp:Panel ID="errorHolder" runat="server"></asp:Panel></td>
            <td align="right">
                <table border="0">
                    <tr>
                        <td><asp:Button ID="btnSave" runat="server" Text="Ok" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" /></td>
                        <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                    </tr>
                </table>
            </td>
        </tr>
      </tbody>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Add existing column to Content Type
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Add existing column to Content Type
</asp:Content>
