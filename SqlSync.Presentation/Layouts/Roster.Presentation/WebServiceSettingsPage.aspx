<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebServiceSettingsPage.aspx.cs" Inherits="Roster.Presentation.Layouts.WebServiceSettingsPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

    <script type="text/javascript">
        function ShowHideGroup(group, img) {
            if ((group == null) || browseris.mac)
                return;
            if (group.style.display != "none") {
                group.style.display = "none";
                img.src = "/_layouts/15/images/plus.gif?rev=23";
            }
            else {
                group.style.display = "";
                img.src = "/_layouts/15/images/minus.gif?rev=23";
            }
        }
    </script>

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

    <asp:PlaceHolder ID="pannelError" runat="server"></asp:PlaceHolder>
    <table width="100%" border="0" cellpadding="3" cellspacing="0" class="ms-v4propertysheetspacing">
        <tbody>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top">
                    <table border="0" cellspacing="0" cellpadding="1">
                        <tr>
                            <td height="28" class="ms-sectionheader" valign="top">
                                <h3 class="ms-standardheader">NAV web service connection</h3>
                            </td>
                        </tr>
                        <tr>
                            <td class="ms-descriptiontext" id="onetidNewColumnDescription">
                                Type URLs and user credentials information you want to store.
                            </td>
                        </tr>
                        <tr>
                            <td><img width="100" height="1" alt="" src="/_layouts/15/images/blank.gif?rev=23" /></td>
                        </tr>
                    </table>
                </td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtCreateTimesheetUrl">Create timesheet URL:</label><br />
                    <asp:TextBox ID="txtCreateTimesheetUrl" runat="server" MaxLength="255" class="ms-input" ToolTip="Create timesheet URL" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorCreateTS" runat="server" ControlToValidate="txtCreateTimesheetUrl" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtProcessTimesheetsUrl">Process timesheets URL:</label><br />
                    <asp:TextBox ID="txtProcessTimesheetsUrl" runat="server" MaxLength="255" class="ms-input" ToolTip="Process timesheets URL" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorDatabase" runat="server" ControlToValidate="txtProcessTimesheetsUrl" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtUser">User:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtUser" runat="server" MaxLength="255" class="ms-input" ToolTip="User" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorUser" runat="server" ControlToValidate="txtUser" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <label for="txtUser">Password:</label><font size="3">&nbsp;</font><br />
                    <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" class="ms-input" ToolTip="Password" Text="" /><br />
                    <asp:RequiredFieldValidator ID="validatorPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="You must specify a value for this required field."
                        Display="Static" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                </td>
            </tr>
        </tbody>
        <tbody id="tbodyFieldMapperHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyFieldMapper.ClientID %>"),document.getElementById("imgFieldMapper"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgFieldMapper" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0"/>&nbsp; Field Mapping</a>
					</h3>
                </td>
            </tr>
        </tbody>
        <tbody id="tbodyFieldMapper" runat="server" style="display: none">
		    <tr>
                <td class="ms-descriptiontext ms-formdescriptioncolumn-wide" valign="top"></td>
                <td class="ms-authoringcontrols" valign="top">
                    <table width="600px" border="0" cellpadding="2">
                        <thead>
                            <tr>
                                <th style="width: 100px; text-align: left">Web method param</th>
                                <th style="width: 15px">&nbsp;</th>
                                <th align="left">Timesheet field</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td align="left">
                                    <span>[Resource_No]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlResourceNo" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Type]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlType" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Job_No]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlJobNo" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Job_Task_No]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlJobTaskNo" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Cause_of_Absence_Code]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlCauseOfAbsence" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Description]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlDescription" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Work_Type_Code]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlWorkType" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Chargeable]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlChargeable" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Non_Roster_Day]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlNonRosterDay" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[Start_Time]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlStartTime" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr><tr>
                                <td align="left">
                                    <span>[End_Time]</span>
                                </td>
                                <td align="center" style="padding: 1px 20px"><=></td>
                                <td align="left">
                                    <asp:DropDownList ID="ddlEndTime" runat="server" AutoPostBack="false" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
		    </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>
        <tfoot>
            <tr>
                <td>&nbsp;</td>
                <td align="right">
                    <table border="0">
                        <tr>
                            <td>
                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="ms-ButtonHeightWidth" OnClick="btnSave_Click" />
                            </td>
                            <td>
                                <asp:Button ID="Button1" runat="server" CausesValidation="False" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClick="btnCancel_Click" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </tfoot>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
NAV web service settings
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
NAV web service settings
</asp:Content>
