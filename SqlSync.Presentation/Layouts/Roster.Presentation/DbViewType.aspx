<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbViewType.aspx.cs" Inherits="Roster.Presentation.Layouts.DbViewType" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%" border="0" cellspacing="0" cellpadding="0">
	    <tr><td width="100%" valign="top" colspan="2">
		    <table width="100%" style="padding-top: 2px;" border="0" cellspacing="0" cellpadding="0">
		        <tr><td class="ms-linksectionheader" valign="top" style="padding: 4px;" colspan="2">
			        <h3 class="ms-standardheader">Choose a view type</h3>
		        </td></tr>
		        <tr><td><img width="1" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
		    </table>
		</td></tr>
    </table>

    <table>
        <tr>
            <td width="50%">
                <table>
                    <tr>
                      <td width="1%" valign="top">
				        <table style="padding-top: 2px;" border="0" cellspacing="0" cellpadding="0">
				           <tbody><tr><td>
						        <img alt="View data on a Web page. You can choose from a list of display styles. " src="/_layouts/15/images/ituser.gif?rev=23" border="0" />
					        </td><td width="4"><img width="4" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
				        </tbody></table>
				      </td>
				      <td width="99%" class="ms-vb" valign="top">
                          <asp:HyperLink ID="linkNewView" runat="server" Text="Standard View" NavigateUrl="DbViewNew.aspx?" onclick="GoToLink(this);return false;"></asp:HyperLink>
					      <br/>View data on a Web page. You can choose from a list of display styles.<br/>&nbsp;
			          </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr><td width="50%"><table>
            <tr><td width="1%" valign="top">
			    <table style="padding-top: 2px;" border="0" cellspacing="0" cellpadding="0">
				    <tr><td>
					    <img alt="View data as a daily, weekly, or monthly calendar." src="/_layouts/15/images/calview.gif?rev=23" border="0" />
					    </td><td width="4"><img width="4" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td>
				    </tr>
				</table></td>
				<td width="99%" class="ms-vb" valign="top">
                    <asp:HyperLink ID="linkNewViewCalendar" runat="server" Text="Calendar View" NavigateUrl="DbViewNew.aspx?Calendar=True&" onclick="GoToLink(this);return false;"></asp:HyperLink>
				    <br/>View data as a daily, weekly, or monthly calendar.<br/>&nbsp;
			    </td>
            </tr>
	    </table></td></tr>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
View Type
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
View Type
</asp:Content>
