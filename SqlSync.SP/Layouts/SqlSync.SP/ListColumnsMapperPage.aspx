<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListColumnsMapperPage.aspx.cs" Inherits="SqlSync.SP.Layouts.ListColumnsMapperPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="600px" border="0" cellpadding="2">
      <thead>
        <tr>
            <th align="left">
                <asp:Label ID="lblCurrentListName" runat="server"></asp:Label>
                <span>Key - 'ID'</span>
            </th>
            <th style="width: 15px">&nbsp;</th>
            <th align="left">
                <asp:DropDownList ID="ddlDbTables" runat="server" AutoPostBack="true" DataTextField="Title" DataValueField="Id" OnSelectedIndexChanged="ddlDbTables_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:Label ID="lblTableName" runat="server"></asp:Label><span> table</span>
                <span>Key - </span>
                <asp:DropDownList ID="ddlTableKey" runat="server" AutoPostBack="true" DataTextField="Title" DataValueField="Id" OnSelectedIndexChanged="ddlTableKey_SelectedIndexChanged">
                </asp:DropDownList>
            </th>
      </thead>
      <tbody>
          <asp:Repeater ID="ColumnsMapperRep" runat="server" OnItemDataBound="ColumnsMapperRep_ItemDataBound">
            <ItemTemplate>
                <tr>
                    <td align="left">
                        <asp:DropDownList ID="ddlListFields" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                            <asp:ListItem Text="" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td align="center" style="padding: 1px 20px"><=></td>
                    <td align="left">
                        <span>[<%# Eval("DbColumnName") %>]</span>
                        <asp:HiddenField ID="hidDbColumnId" runat="server" Value='<%# Eval("DbColumnId") %>' />
                    </td>
                </tr>
            </ItemTemplate>
          </asp:Repeater>

      </tbody>
      <tfoot>
          <tr><td></td><td></td><td></td></tr>
          <tr><td colspan="3"><asp:Panel ID="pnlError" runat="server"></asp:Panel></td></tr>
          <tr><td colspan="3" align="right">
              <table border="0">
                <tr>
                    <td><asp:Button ID="btnSave" runat="server" Text="Save mapping" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" /></td>
                    <td><asp:Button ID="btnSaveAndSync" runat="server" Text="Save and Sync" CssClass="ms-ButtonHeightWidth" OnClick="btnSaveSync_Click" /></td>
                    <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                    <td><asp:Button ID="btnRemove" runat="server" Text="Remove mapping" CssClass="ms-ButtonHeightWidth" OnClick="btnRemove_Click" Visible="false" /></td>
                </tr>
              </table>
          </td></tr>
      </tfoot>
    </table>
    

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Map page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Map List fields to DB table columns
</asp:Content>
