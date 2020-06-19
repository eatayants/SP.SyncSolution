<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbListFieldsOrder.aspx.cs" Inherits="Roster.Presentation.Layouts.DbListFieldsOrder" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

    <script type="text/javascript">
        function Reorder(eSelect, iCurrentField, numSelects) {
            var iNewOrder = eSelect.selectedIndex + 1;
            var iPrevOrder;
            var positions = new Array(numSelects);
            var ix;
            for (ix = 0; ix < numSelects; ix++) {
                positions[ix] = 0;
            }

            var colsBlock = document.getElementById('list_columns_tbl');
            for (ix = 0; ix < numSelects; ix++) {
                positions[GetElementByClassName(colsBlock, "ViewOrder" + ix).selectedIndex] = 1;
            }
            for (ix = 0; ix < numSelects; ix++) {
                if (positions[ix] == 0) {
                    iPrevOrder = ix + 1;
                    break;
                }
            }
            if (iNewOrder != iPrevOrder) {
                var iInc = iNewOrder > iPrevOrder ? -1 : 1
                var iMin = Math.min(iNewOrder, iPrevOrder);
                var iMax = Math.max(iNewOrder, iPrevOrder);
                for (var iField = 0; iField < numSelects; iField++) {
                    if (iField != iCurrentField) {
                        if (GetElementByClassName(colsBlock, "ViewOrder" + iField).selectedIndex + 1 >= iMin &&
                            GetElementByClassName(colsBlock, "ViewOrder" + iField).selectedIndex + 1 <= iMax) {
                            GetElementByClassName(colsBlock, "ViewOrder" + iField).selectedIndex += iInc;
                        }
                    }
                }
            }
        }
    </script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
      <tbody>
        <tr>
            <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
			    <table border="0" cellspacing="0" cellpadding="1">
				    <tr><td height="28" class="ms-sectionheader" valign="top"><h3 class="ms-standardheader">Field order</h3></td></tr>
				    <tr><td class="ms-descriptiontext">Choose the order of the fields by selecting a number for each field under "Position".</td></tr>
			    </table>
		    </td>
            <td id="list_columns_tbl" valign="top" style="padding-left: 20px">
                <label></label><br />
                
                <asp:GridView ID="listColumnsGrid" runat="server" AutoGenerateColumns="false" OnRowDataBound="ListColumnsGrid_RowDataBound"
                    DataKeyNames="Id" CellPadding="2" BorderWidth="0" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="DisplayName" HeaderText="Column Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                        <asp:TemplateField HeaderText="Position" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="140px">
                            <ItemTemplate>
                                <asp:DropDownList runat="server" id="ddlColumnPosition" AutoPostBack="false"></asp:DropDownList> 
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView><br />
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
Change column ordering
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Change column ordering
</asp:Content>
