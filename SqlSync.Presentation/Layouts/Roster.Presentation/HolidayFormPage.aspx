<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HolidayFormPage.aspx.cs" Inherits="Roster.Presentation.Layouts.HolidayFormPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

    <style type="text/css">
        .ms-dialog-nr body #s4-ribbonrow, .ms-dialog-nr body #globalNavBox {
            display: block !important;
        }
    </style>

    <link href="/_layouts/15/Roster.Presentation/js/select2.min.css" type="text/css" rel="stylesheet" />
    <script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js" type="text/javascript"></script>
    <script src="/_layouts/15/Roster.Presentation/js/select2.min.js" type="text/javascript"></script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <table width="100%">
        <tr>
            <td>
                <asp:DetailsView ID="holidayForm" runat="server" AllowPaging="false" AutoGenerateRows="false" GridLines="None" CellSpacing="10" OnDataBound="holidayForm_DataBound">
                    <Fields>
                        <asp:TemplateField HeaderText="Holiday Name">
                            <ItemTemplate>
                                <asp:Label ID="lblName" runat="server" Text='<%#Eval("HolidayName") %>' />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtName" runat="server" Text='<%#Eval("HolidayName") %>'></asp:TextBox>
                                <asp:RequiredFieldValidator ID ="txtNameValidator" runat="server" ErrorMessage="Please specfiy Holiday Name"
                                    ControlToValidate="txtName" ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Date">
                            <ItemTemplate>
                                <asp:Label ID="lblDate" runat="server" Text='<%# Convert.ToDateTime(Eval("HolidayDate")).ToShortDateString() %>' />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <SharePoint:DateTimeControl ID="dtHolidayDate" runat="server" SelectedDate='<%#Eval("HolidayDate") %>' DateOnly="true" />
                                <asp:RequiredFieldValidator ID ="dtHolidayDateValidator" runat="server" ErrorMessage="Please specfiy Holiday Date"
                                    ControlToValidate="dtHolidayDate$dtHolidayDateDate" ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Holiday Type">
                            <ItemTemplate>
                                <asp:Label ID="lblType" runat="server" Text='<%#Eval("HolidayType") %>' />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList runat="server" ID="ddlType" AppendDataBoundItems="true" AutoPostBack="false"></asp:DropDownList>
                                <asp:RequiredFieldValidator ID ="ddlTypeValidator" runat="server" ErrorMessage="Please specfiy Holiday Type"
                                    ControlToValidate="ddlType" ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
                                <script type="text/javascript">
                                    jQuery('select[id$="ddlType"]').select2({
                                        placeholder: "", width: '250px'
                                    });
                                    jQuery('select[id$="ddlType"]').select2("val", '<%#Eval("HolidayType") %>');
                                </script>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Where it is observed">
                            <ItemTemplate>
                                <asp:Label ID="lblObserved" runat="server" Text='<%#Eval("Observed") %>' />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:ListBox runat="server" ID="ddlObserved" SelectionMode="multiple" AppendDataBoundItems="true" AutoPostBack="false"></asp:ListBox>
                                <asp:RequiredFieldValidator ID ="ddlObservedValidator" runat="server" ErrorMessage="Please specfiy State"
                                    ControlToValidate="ddlObserved" ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
                                <script type="text/javascript">
                                    jQuery('select[id$="ddlObserved"]').select2({
                                        placeholder: "", allowClear: true, width: '250px'
                                    });
                                    jQuery('select[id$="ddlObserved"]').select2("val", '<%#Eval("Observed") %>'.split('; '));
                                </script>
                            </EditItemTemplate>
                        </asp:TemplateField>
                    </Fields>
                </asp:DetailsView>
            </td>
        </tr>
        <tr>
            <td align="right" style="padding-top: 15px;">
                <asp:Button ID="btnSave" runat="server" Text="Ok" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" />
                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="ms-ButtonHeightWidth" OnClick="btnDelete_Click" CausesValidation="False" />
                <asp:Button ID="btnCancel" runat="server" CausesValidation="False" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClick="btnCancel_Click" />
            </td>
        </tr>
    </table>

    <asp:PlaceHolder ID="ErrorHolder" runat="server"></asp:PlaceHolder>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Holiday Form
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
    <asp:Label ID="lblPageName" runat="server"></asp:Label>
</asp:Content>
