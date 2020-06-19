<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbListFromSettings.aspx.cs" Inherits="Roster.Presentation.Layouts.DbListFromSettings" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    
    <SharePoint:ScriptLink name="clienttemplates.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="clientforms.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="clientpeoplepicker.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="autofill.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="sp.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="sp.runtime.js" runat="server" LoadAfterUI="true" Localizable="false" />
    <SharePoint:ScriptLink name="sp.core.js" runat="server" LoadAfterUI="true" Localizable="false" />

    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/knockout-3.1.0.js"></script>
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>

    <style type="text/css">
        .alt {
            background-color: #f7f7f7;
        }
    </style>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Content Type Name</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Type a name for this Content Type.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <label>Name:</label><br />
                <asp:TextBox ID="txtCtName" runat="server" CssClass="ms-input" Width="400px"></asp:TextBox><br />
                <asp:RequiredFieldValidator ID="validatorCtName" runat="server" ControlToValidate="txtCtName" ErrorMessage="You must specify a value for this required field."
                                Display="Dynamic" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                <br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Display Form Url</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Specify default URL for list Display Form.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <label>Display Form Url:</label><br />
                <asp:TextBox ID="txtDisplayFormUrl" runat="server" CssClass="ms-input" Width="400px"></asp:TextBox><br /><br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Edit Form Url</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Specify default URL for list Edit Form.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <label>Edit Form Url:</label><br />
                <asp:TextBox ID="txtEditFormUrl" runat="server" CssClass="ms-input" Width="400px"></asp:TextBox><br /><br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">New Form Url</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Specify default URL for list New Form.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <label>New Form Url:</label><br />
                <asp:TextBox ID="txtNewFormUrl" runat="server" CssClass="ms-input" Width="400px"></asp:TextBox><br /><br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Is default content type</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Set current Content Type as default.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <asp:CheckBox ID="chIsDefault" runat="server" Text="Is default?" />
                <asp:Label ID="lblIsDefaultCT" runat="server" Text="Default Content Type" Visible="false"></asp:Label><br /><br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Is visible on New button</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Content types not marked as visible will not appear on the new button.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <asp:CheckBox ID="chIsVisibleOnNew" runat="server" Text="Is visible on New button?" /><br /><br />
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Custom actions</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Specify list of Custom Actions.</td></tr>
			    </table>
		    </td>
            <td valign="top">
                <table border="0" cellpadding="0" cellspacing="0">
                    <tbody data-bind='foreach: CustomActions'>
                        <tr><td>
                            <table width="100%" cellpadding="4" cellspacing="1" border="0" data-bind='css: { alt: $index()%2 }'>
                                <tr>
                                    <td valign="top"><span>Label text:</span></td>
                                    <td valign="top"><input type="text" data-bind='value: labelText' class="ms-input" maxlength="255" title="Label text" style="width: 318px" /></td>
                                    <td rowspan="4" valign="middle">
                                        <a title="Remove" href="#" data-bind='click: $parent.removeAction'>
                                            <img class="ms-advsrch-img" src="/_layouts/images/IMNDND.PNG" alt="" />
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top"><span>Description:</span></td>
                                    <td valign="top"><input type="text" data-bind='value: description' class="ms-input" maxlength="255" title="Description" style="width: 318px" /></td>
                                </tr>
                                <tr>
                                    <td valign="top"><span>Access Group:</span></td>
                                    <td valign="top"><div data-bind="clientPeoplePicker: accessGroupArr"></div></td>
                                </tr>
                                <tr>
                                    <td valign="top"><span>Image URL:</span></td>
                                    <td valign="top"><input type="text" data-bind='value: imageUrl' class="ms-input" maxlength="255" title="Image url" style="width: 318px" /></td>
                                </tr>
                                <tr>
                                    <td valign="top" valign="top"><span>Command:</span></td>
                                    <td valign="top"><textarea data-bind='value: command' class="ms-input" title="Command" rows="3" cols="43"></textarea></td>
                                </tr>
                            </table>
                        </td></tr>
                    </tbody>
                    <tfoot>
                        <tr><td class="ms-addnew">
                            <span style="POSITION: relative; WIDTH: 10px; DISPLAY: inline-block; HEIGHT: 10px; OVERFLOW: hidden" class="s4-clust">
                                <img style="POSITION: absolute; TOP: -128px !important; LEFT: 0px !important" alt="" src="/_layouts/images/fgimg.png" />
                            </span>&nbsp;<a title="Add action" href="#" data-bind='click: addAction' class="ms-addnew">Add Custom Action</a>
                        </td></tr>
                    </tfoot>
                </table>
                <input type="hidden" id="hidCustomActions" data-bind="value: ko.toJSON(CustomActions)" runat="server" />
                <script type="text/javascript">
                    var hidCustomActions_Value = JSON.parse(document.getElementById('<%= hidCustomActions.ClientID %>').value);
                    </script>
                <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/customActions.model.js?rev=20150521"></script>
            </td>
        </tr>
      </tbody>
      <tbody>
        <tr>
            <td>
                <asp:Panel ID="panelError" runat="server"></asp:Panel>
            </td>
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
Content Type settings
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Content Type settings
</asp:Content>
