<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PublishingJobSetting.aspx.cs" Inherits="Roster.Presentation.Layouts.PublishingJobSetting" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:PlaceHolder ID="pannelError" runat="server"></asp:PlaceHolder>
    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
        <tbody>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
                    <table border="0" cellspacing="0" cellpadding="1">
                        <tr>
                            <td height="28" class="ms-sectionheader" valign="top">
                                <h3 class="ms-standardheader">Days Ahead:</h3>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-descriptiontext" id="onetidNewColumnDescription">
                                Type the "Days Ahead" value.
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td>
                        </tr>
                    </table>
                </td>
                <td class="ms-authoringcontrols" valign="top">
                    <asp:TextBox ID="txtDaysAhead" runat="server" MaxLength="255" class="ms-input" ToolTip="Days Ahead" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorDaysAhead" runat="server" ControlToValidate="txtDaysAhead" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                    <asp:CompareValidator ID="typeDaysAhead" runat="server" ControlToValidate="txtDaysAhead" Type="Integer" ErrorMessage="Value must be an digit!" 
                         Display="Static" CssClass="ms-error"  Operator="DataTypeCheck" EnableClientScript="true"></asp:CompareValidator>
                </td>
            </tr>
             <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
                    <table border="0" cellspacing="0" cellpadding="1">
                        <tr>
                            <td height="28" class="ms-sectionheader" valign="top">
                                <h3 class="ms-standardheader">Status:</h3>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-descriptiontext">
                                Current SQLServer Agent Status
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td>
                        </tr>
                    </table>
                </td>                 
                <td class="ms-authoringcontrols" valign="top">
                    <asp:PlaceHolder ID="pnAgentStatus" runat="server"></asp:PlaceHolder>
                </td>
            </tr>
            <tr>
                <td class="ms-authoringcontrols" valign="top" colspan="2">  
                    <label>SQL Server Agent Jobs</label>
                    <br />               
                    <SharePoint:SPGridView ID="JobSettingGrid" runat="server" AutoGenerateColumns="false" OnRowDataBound="JobSettingGrid_RowDataBound"
                        DataKeyNames="Id" CellPadding="2" BorderWidth="0" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="JobName" HeaderText="Job Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="JobDescription" HeaderText="Description" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="IsScheduled" HeaderText="Is Scheduled" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="IsEnabled" HeaderText="Is Enabled" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="LastRunDateTime" HeaderText="Last Run" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="LastRunDuration" HeaderText="Run Duration" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="LastRunStatus" HeaderText="Run Status" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="LastRunStatusMessage" HeaderText="Run Status Message" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:BoundField DataField="NextRunDateTime" HeaderText="Next Run" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                        </Columns>
                    </SharePoint:SPGridView>
                </td>
            </tr>
            <tr>
                <td class="ms-authoringcontrols" valign="top">&nbsp;</td>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
                    <table border="0">
                        <tr>
                            <td>
                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" />
                            </td>
                            <td>
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClick="btnCancel_Click"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Roster Solution Jobs
</asp:Content>
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
Roster Solution Jobs
</asp:Content>
