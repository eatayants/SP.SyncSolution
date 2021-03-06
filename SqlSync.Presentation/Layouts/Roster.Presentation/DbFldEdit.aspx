﻿<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbFldEdit.aspx.cs" Inherits="Roster.Presentation.Layouts.DbFldEdit" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
				<table border="0" cellspacing="0" cellpadding="1">
                    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Name and Type</h3></td></tr>
					<tr><td class="ms-descriptiontext" id="onetidNewColumnDescription">Type a name for this column, and select the type of information you want to store in the column.</td></tr>
					<tr><td><img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
				</table>
			</td>
            <td class="ms-authoringcontrols" valign="top">
                <label for="idColName">Column name:</label><font size="3">&nbsp;</font><br />
                <asp:TextBox ID="txtColName" runat="server" MaxLength="255" class="ms-input" ToolTip="Column name:" /><br />
                <asp:RequiredFieldValidator ID="validatorColName" runat="server" ControlToValidate="txtColName" ErrorMessage="You must specify a value for this required field."
                    Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
			</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                <label>The type of information in this column is:</label><br />
                <asp:RadioButton ID="radioFieldType" runat="server" Checked="true" />
            </td>
        </tr>
        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Additional Column Settings</h3></td></tr>
				    <tr><td class="ms-descriptiontext" id="onetidNewColumnOption2">Specify detailed options for the type of information you selected.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <label>Description:</label><br />
                <asp:TextBox ID="txtDescription" runat="server" Columns="40" Rows="2" CssClass="ms-input" ToolTip="Description:" TextMode="MultiLine"></asp:TextBox><br /><br />

                <asp:PlaceHolder ID="holderExtraSettings" runat="server">

                </asp:PlaceHolder>
            </td>
        </tr>
        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top">
				        <h3 class="ms-standardheader">Database Column Settings</h3>
				    </td></tr>
				    <tr>
				        <td class="ms-descriptiontext">Specify column for storing information for the type you selected.</td>
				    </tr>
			    </table>
		    </td>
            <td valign="top">
                <asp:DropDownList ID="ddlTableFieldName" Width = "400px" runat="server" DataTextField="Title" DataValueField="Id"></asp:DropDownList>
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
				<table border="0" cellspacing="0" cellpadding="1">
                    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Used in</h3></td></tr>
					<tr><td class="ms-descriptiontext">List of Content types where current column is used.</td></tr>
					<tr><td><img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
				</table>
			</td>
            <td class="ms-authoringcontrols" valign="top">
                <label for="idColName">Content types:</label><br />
                <asp:Label ID="lblUsedInContentTypes" runat="server"></asp:Label>
			</td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td>&nbsp;</td>
            <td align="right">
                <table border="0">
                    <tr>
                        <td><asp:Button ID="btnSave" runat="server" Text="Ok" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" /></td>
                        <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                        <td><asp:Button ID="btnRemove" runat="server" Text="Remove" CssClass="ms-ButtonHeightWidth" OnClick="btnRemove_Click" /></td>
                    </tr>
                </table>
            </td>
        </tr>
      </tbody>
    </table>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Edit column
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Edit column
</asp:Content>
