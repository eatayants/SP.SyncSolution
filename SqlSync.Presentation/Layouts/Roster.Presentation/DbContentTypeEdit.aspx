<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbContentTypeEdit.aspx.cs" Inherits="Roster.Presentation.Layouts.DbContentTypeEdit" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table class="ms-settingsframe ms-listedit" style="width: 100%; height: 100%;" border="0" cellpadding="0" cellspacing="0">
      <tbody>
        <tr>
            <td width="100%" style="padding-top: 0px;" colspan="4">
                <table width="300px" class="ms-pageinformation" cellspacing="0" cellpadding="0">
	                <tr>
	                    <td width="100%" class="ms-listeditheader" valign="top">
	                        <table width="100%" id="idItemHoverTable">
		                        <tr>
			                        <th class="ms-listedit-listinfo" valign="top" style="padding-bottom: 4px;" colspan="2" scope="col">
                                        <h3 class="ms-standardheader"><span class="ms-linksectionheader">Content Type Information</span></h3>
			                        </th>
		                        </tr>
                                <tr>
			                        <th valign="top" scope="row">Name:</th>
			                        <td valign="top"><asp:Label ID="lblContentTypeName" runat="server"></asp:Label></td>
		                        </tr>
		                    </table>
	                    </td>
	                </tr>
	            </table>
           </td>
        </tr>
        <tr height="24"><td>&nbsp;</td></tr>
        <tr>
            <td width="100%" class="ms-listeditnav" colspan="3">
	            <table width="100%" border="0" cellspacing="2" cellpadding="1">
	                <tr>
	                    <td width="34%" valign="top">
	                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
		                        <tr>
		                            <td width="100%" class="ms-linksectionheader" nowrap="nowrap" style="padding: 4px;">
			                            <h3 class="ms-standardheader">General Settings</h3>
		                            </td>
	                            </tr>
	                            <tr><td height="1" class="ms-1pxfont"><img width="1" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td></tr>
	                            <tr>
                                    <td width="100%" style="padding: 0px 4px 4px;" class="ms-propertysheet">
                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
								            <tr>
									            <td width="8" class="ms-descriptiontext ms-linksectionitembullet" nowrap="nowrap" valign="top" style="padding-top: 5px;">
										            <img width="5" height="5" alt="" src="/_layouts/15/images/setrect.gif?rev=23" />&nbsp;
									            </td>
									            <td class="ms-descriptiontext ms-linksectionitemdescription" valign="top">
                                                    <asp:HyperLink ID="linkFormSettings" runat="server" Text="General settings"
                                                        NavigateUrl="DbListFromSettings.aspx" onclick="GoToLink(this);return false;"></asp:HyperLink>
								                </td>
								            </tr>
                                        </table>
	                                </td>
	                            </tr>
	                        </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr height="24"><td>&nbsp;</td></tr>
        <tr>
            <td width="100%">
                <table width="100%" cellspacing="3" cellpadding="0">
	                <tr height="10"><td class="ms-linksectionheader" style="padding: 4px;" colspan="4"><h3 class="ms-standardheader">Columns</h3></td></tr>
					<tr>
					    <td class="ms-descriptiontext ms-listedit-sectiondescription" valign="top" colspan="4">
                            A column stores information about each item. The following columns are currently available in this list:
						</td>
					</tr>
                    <tr><td class="ms-gb" colspan="4">
                        <asp:Repeater ID="ColumnsRepeater" runat="server">
                            <HeaderTemplate>
                                <table width="100%" border="0" cellspacing="0" cellpadding="0" summary="TmpTsk">
						            <tr>
								        <th width="25%" class="ms-vh2-nofilter" scope="col">Column (click to edit)</th>
								        <th width="25%" class="ms-vh2-nofilter" scope="col">Type</th>
								        <th class="ms-vh2-nofilter" scope="col">Required</th>
						            </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                    <tr>
                                        <td class="ms-vb2">
                                            <asp:HyperLink ID="linkToField" runat="server" Text='<%# Eval("FieldName") %>' onclick="GoToLink(this);return false;"
                                                NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Id", "DbFldEdit.aspx?Field={0}") %>'></asp:HyperLink>
                                        </td>
					                    <td class="ms-vb2"><%# Eval("FieldType") %></td>
					                    <td class="ms-vb2">
                                            <span class="ms-updatelink-span" runat="server" visible='<%# (Convert.ToBoolean(Eval("Required").ToString())) ? true : false %>'>
                                                <img class="ms-updatelink-icon" src="/_layouts/15/images/spcommon.png?rev=23" alt="Required"/>
                                            </span>
					                    </td>
					                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </td></tr>
					<tr>
                         <td colspan="4">
                             <table style="padding-left: 5px;" cellspacing="0" cellpadding="0">
						        <tr>
								    <td class="ms-descriptiontext ms-foo"><img alt="" src="/_layouts/15/images/setrect.gif?rev=23"/>&nbsp;</td>
								    <td class="ms-descriptiontext ms-linksectionitemdescription">
                                        <asp:HyperLink ID="linkNewField" runat="server" Text="Create column"
                                                        NavigateUrl="DbFldNew.aspx" onclick="GoToLink(this);return false;"></asp:HyperLink>
								    </td>
						        </tr>
                                <tr>
								    <td class="ms-descriptiontext ms-foo"><img alt="" src="/_layouts/15/images/setrect.gif?rev=23"/>&nbsp;</td>
								    <td class="ms-descriptiontext ms-linksectionitemdescription">
                                        <asp:HyperLink ID="linkAddField" runat="server" Text="Add from existing columns"
                                                        NavigateUrl="DbListAddFieldToContentType.aspx" onclick="GoToLink(this);return false;"></asp:HyperLink>
								    </td>
						        </tr>
                                <tr>
								    <td class="ms-descriptiontext ms-foo"><img alt="" src="/_layouts/15/images/setrect.gif?rev=23"/>&nbsp;</td>
								    <td class="ms-descriptiontext ms-linksectionitemdescription">
                                        <asp:HyperLink ID="linkFieldsOrdering" runat="server" Text="Column ordering"
                                                        NavigateUrl="DbListFieldsOrder.aspx" onclick="GoToLink(this);return false;"></asp:HyperLink>
								    </td>
						        </tr>
                             </table>
					     </td>
					</tr>
                </table>
            </td>
        </tr>
      </tbody>
      <tfoot>
        <tr>
            <td align="right">
                <table border="0">
                    <tr>
                        <td><asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="ms-ButtonHeightWidth" OnClick="btnDelete_Click" /></td>
                        <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                    </tr>
                </table>
            </td>
        </tr>
      </tfoot>
    </table>

    <asp:Panel ID="panelErrror" runat="server" ForeColor="Red"></asp:Panel>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Edit Content Type
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Edit Content Type
</asp:Content>
