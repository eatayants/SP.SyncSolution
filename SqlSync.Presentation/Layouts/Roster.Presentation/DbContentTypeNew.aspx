<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbContentTypeNew.aspx.cs" Inherits="Roster.Presentation.Layouts.DbContentTypeNew" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%" border="0" cellpadding="0" cellspacing="0">
        <tbody>
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2"><h3 class="ms-standardheader">Name</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody>
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Type a name for this content type. Make the name descriptive, so that site visitors will know what to expect.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellspacing="1" cellpadding="1">
			            <tr><td class="ms-authoringcontrols" nowrap="nowrap">Name:</td></tr>
                        <tr><td>
                            <asp:TextBox runat="server" ID="txtContentTypeName" Width="234px"></asp:TextBox><br />
                            <asp:RequiredFieldValidator ID="validatorCtName" runat="server" ControlToValidate="txtContentTypeName" ErrorMessage="You must specify a value for this required field."
                                Display="Dynamic" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                        </td></tr>
			        </table>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tfoot>
            <tr>
                <td class="ms-sectionheader" colspan="2">
                    <asp:PlaceHolder ID="errorHolder" runat="server"></asp:PlaceHolder>
                </td>
			    <td class="ms-authoringcontrols" colspan="2">
                    <table border="0">
                        <tr>
                            <td><asp:Button ID="btnSave" runat="server" Text="Ok" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" /></td>
                            <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                        </tr>
                    </table>
			    </td>
            </tr>
        </tfoot>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Add Content Type
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Add Content Type
</asp:Content>
