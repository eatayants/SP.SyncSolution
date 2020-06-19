<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VersionHistoryWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.VersionHistoryWebPart.VersionHistoryWebPartUserControl" %>

<table width="100%" border="0" cellpadding="1" class="ms-settingsframe">
    <thead>
        <tr>
            <th class="ms-vh2-nofilter">
                <abbr title="Version number">No.</abbr>
                <img alt="Sort Descending" src="/_layouts/15/images/rsort.gif" border="0">
            </th>
            <th class="ms-vh2-nofilter">
                Modified
            </th>
            <th class="ms-vh2-nofilter">
                Modified By
            </th>
        </tr>
    </thead>
    <tbody>

        <asp:Repeater ID="VersionsRep" runat="server">
            <ItemTemplate>
                <tr>
                    <td class="ms-vb2" style="vertical-align: top; width: 15px;"><%# Eval("Version") %></td>
                    <td class="ms-vb-title" style="vertical-align: top;"><%# Eval("Modified") %></td>
                    <td class="ms-vb2" style="vertical-align: top;"><%# Eval("ModifiedBy") %></td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">
                        <table>
                            <tbody>
                                <asp:Repeater runat="server" ID="ChangesRep" DataSource='<%# Eval("Changes") %>'>
                                    <ItemTemplate>
                                        <tr>
                                            <td class="ms-propertysheet" nowrap="nowrap" valign="top" style="padding: 0px 10px;" colspan="2">
                                                <%# Eval("FieldName") %>
											</td>
                                            <td class="ms-vb" valign="top">
											    <%# Eval("FieldValue") %>
											</td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>

    </tbody>

</table>