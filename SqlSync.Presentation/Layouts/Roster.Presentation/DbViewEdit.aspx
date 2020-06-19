<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DbViewEdit.aspx.cs" Inherits="Roster.Presentation.Layouts.DbViewEdit" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/knockout-3.1.0.js"></script>
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/linq.min.js"></script>
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/jscolor.js"></script>

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
        function Reorder(eSelect, iCurrentField, numSelects) {
            var iNewOrder = eSelect.selectedIndex + 1;
            var iPrevOrder;
            var positions = new Array(numSelects);
            var ix;
            for (ix = 0; ix < numSelects; ix++) {
                positions[ix] = 0;
            }

            var colsBlock = document.getElementById('view_settings_tbl');
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

    <table width="100%" border="0" cellpadding="0" cellspacing="0" id="view_settings_tbl">
        <tbody>
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2"><h3 class="ms-standardheader">Name</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewName">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Type a name for this view of the list. Make the name descriptive, so that site visitors will know what to expect.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellspacing="1" cellpadding="1">
			            <tr><td class="ms-authoringcontrols" nowrap="nowrap">View Name:</td></tr>
                        <tr><td>
                            <asp:TextBox runat="server" ID="txtViewName" Width="234px"></asp:TextBox><br />
                            <asp:RequiredFieldValidator ID="validatorViewName" runat="server" ControlToValidate="txtViewName" ErrorMessage="You must specify a value for this required field."
                                Display="Dynamic" CssClass="ms-error" EnableClientScript="true"></asp:RequiredFieldValidator>
                            <br />
                            <asp:CheckBox ID="chIsDefault" runat="server" Visible="false" Text=" Make this the default view " />
                            <asp:Label ID="lblIsDefaultView" runat="server" Text="Default view" Visible="false"></asp:Label>
                        </td></tr>
			        </table>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewColumnsHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewColumns.ClientID%>"),document.getElementById("imgViewColumns"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewColumns" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Columns</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewColumns" runat="server">
		    <tr>
				<td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Select or clear the check box next to each column you want to show or hide in this view of this page.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <asp:GridView ID="ViewColumnsGrid" runat="server" AutoGenerateColumns="false" OnRowDataBound="ViewColumnsGrid_RowDataBound"
                        DataKeyNames="Id" Width="100%" CellPadding="2" BorderWidth="0" GridLines="None">
                        <Columns>
                            <asp:TemplateField HeaderText="Display" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" id="chIsSelected" AutoPostBack="false"></asp:CheckBox> 
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DisplayName" HeaderText="Column Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:TemplateField HeaderText="Position from Left" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:DropDownList runat="server" id="ddlColumnPosition" AutoPostBack="false"></asp:DropDownList> 
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewSortHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewSort.ClientID %>"),document.getElementById("imgViewSort"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewSort" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Sort</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewSort" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Select up to two columns to determine the order in which the items in the view are displayed</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
			        <table border="0" cellspacing="1" cellpadding="4">
				        <tr>
				            <td>First sort by the column:</td>
				        </tr>
                        <tr><td>
                            <asp:DropDownList ID="ddlSortColumn" runat="server" DataTextField="FieldName" DataValueField="Id" AppendDataBoundItems="true">
                                <asp:ListItem Text="None" Value="00000000-0000-0000-0000-000000000000"></asp:ListItem>
                            </asp:DropDownList>
                        </td></tr>
				        <tr>
                            <td>
                                <asp:RadioButtonList ID="radioSortOrder" runat="server">
                                    <asp:ListItem Text="Show items in ASCending order (A, B, C, or 1, 2, 3)" Value="ASC" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Show items in DESCending order (C, B, A, or 3, 2, 1)" Value="DESC"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
				        </tr>
			        </table>
			    </td>
		    </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewFilterHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewFilter.ClientID %>"),document.getElementById("imgViewFilter"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewFilter" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Filter</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewFilter" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Show all of the items in this view, or display a subset of the items by using filters</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
			        <table border="0" cellspacing="1" cellpadding="4">
				        <tr>
                            <td>
                                <asp:RadioButtonList ID="radioIsWithFilters" runat="server">
                                    <asp:ListItem Text="Show all items in this view" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Show items only when the following is true:" Value="1"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
				        </tr>
                        <tr><td style="padding-left: 23px;">
                            <table>
                                <tr><td>Show the items when column</td></tr>
                                <tr><td>
                                    <asp:DropDownList ID="ddlFilterColumn" runat="server" DataTextField="FieldName" DataValueField="Id" AppendDataBoundItems="true" onchange="UpdateFiltering(this);">
                                        <asp:ListItem Text="None" Value="00000000-0000-0000-0000-000000000000"></asp:ListItem>
                                    </asp:DropDownList>

                                    <script type="text/javascript">
                                        function UpdateFiltering(filterFieldCtrl) {
                                            var fldId = filterFieldCtrl.value;
                                            if (fldId !== "00000000-0000-0000-0000-000000000000") {
                                                var filtersOnElemId = "<%= radioIsWithFilters.ClientID %>" + "_1";
                                                var filtersOffElemId = "<%= radioIsWithFilters.ClientID %>" + "_0";
                                                document.getElementById(filtersOnElemId).checked = true;
                                                document.getElementById(filtersOffElemId).checked = false;
                                            }
                                        }
                                    </script>
                                </td></tr>
                                <tr><td>
                                    <asp:DropDownList ID="ddlFilterCompareTypes" runat="server" DataTextField="FieldName" DataValueField="Id">
                                        <asp:ListItem Text="is equal to" Value="0" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="is less than" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="is greater than" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="is less than or equal to" Value="3"></asp:ListItem>
                                        <asp:ListItem Text="is greater than or equal to" Value="4"></asp:ListItem>
                                    </asp:DropDownList>
                                </td></tr>
                                <tr><td>
                                    <asp:TextBox ID="txtFilterValue" runat="server"></asp:TextBox>
                                </td></tr>
                            </table>
                        </td></tr>
			        </table>
			    </td>
		    </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewLimitHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewLimit.ClientID%>"),document.getElementById("imgViewLimit"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewLimit" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Item Limit</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewLimit" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Use an item limit to limit the amount of data that is returned to users of this view.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellspacing="1" cellpadding="1">
			            <tr><td class="ms-authoringcontrols" nowrap="nowrap">Number of items to display:</td></tr>
                        <tr><td style="padding-left: 20px;"><asp:TextBox runat="server" ID="txtViewLimit" Text="30" Width="60px"></asp:TextBox></td></tr>
			        </table>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewCalendarColsHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewCalendarCols.ClientID%>"),document.getElementById("imgViewCalendarCols"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewCalendarCols" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Calendar Columns</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewCalendarCols" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Specify columns to be represented in the Calendar Views. The Title fields are required fields. The Sub Heading fields are optional fields.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellspacing="1" cellpadding="1">
			            <tr><td class="ms-authoringcontrols" nowrap="nowrap">Month View Title:</td></tr>
                        <tr><td style="padding-left: 15px"><asp:DropDownList ID="ddlMonthViewTitle" runat="server" DataTextField="FieldName" DataValueField="InternalName"></asp:DropDownList></td></tr>
                        <tr><td class="ms-authoringcontrols" nowrap="nowrap">Week View Title:</td></tr>
                        <tr><td style="padding-left: 15px"><asp:DropDownList ID="ddlWeekViewTitle" runat="server" DataTextField="FieldName" DataValueField="InternalName"></asp:DropDownList></td></tr>
                        <tr><td class="ms-authoringcontrols" nowrap="nowrap">Week View Sub Heading:</td></tr>
                        <tr><td style="padding-left: 15px"><asp:DropDownList ID="ddlWeekViewSubHeading" runat="server" DataTextField="FieldName" DataValueField="InternalName"></asp:DropDownList></td></tr>
                        <tr><td class="ms-authoringcontrols" nowrap="nowrap">Day View Title:</td></tr>
                        <tr><td style="padding-left: 15px"><asp:DropDownList ID="ddlDayViewTitle" runat="server" DataTextField="FieldName" DataValueField="InternalName"></asp:DropDownList></td></tr>
                        <tr><td class="ms-authoringcontrols" nowrap="nowrap">Day View Sub Heading:</td></tr>
                        <tr><td style="padding-left: 15px"><asp:DropDownList ID="ddlDayViewSubHeading" runat="server" DataTextField="FieldName" DataValueField="InternalName"></asp:DropDownList></td></tr>
			        </table>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewScopeHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewScope.ClientID%>"),document.getElementById("imgViewScope"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewScope" alt="" src="/_layouts/15/images/minus.gif?rev=23" border="0"/>&nbsp; Default Scope</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewScope" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Choose the default scope for the view.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellspacing="1" cellpadding="1">
			            <tr><td class="ms-authoringcontrols" nowrap="nowrap">Default scope:</td></tr>
                        <tr><td>
                            <asp:RadioButtonList ID="radioDefaultScope" runat="server">
                                <asp:ListItem Value="Day" Text="Day"></asp:ListItem>
                                <asp:ListItem Value="Week" Text="Week" Selected="True"></asp:ListItem>
                                <asp:ListItem Value="Month" Text="Month"></asp:ListItem>
                            </asp:RadioButtonList>
                        </td></tr>
                        <tr><td class="ms-authoringcontrols">You can change this at any time while using the calendar.</td></tr>
			        </table>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewDynamicColorHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewDynamicColor.ClientID%>"),document.getElementById("imgViewDynamicColor"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewDynamicColor" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0"/>&nbsp; Dynamic colour-coding</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewDynamicColor" style="display: none" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Add Dynamic colour-coding settings to this view.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellpadding="0" cellspacing="0" data-bind='visible: (DynamicConditions().length > 0)'>
                        <tr><td style="font-weight: bold">Matching column</td></tr>
                        <tr>
                            <td class="ms-authoringcontrols" style="padding-left: 5px;">
                                <select data-bind='options: availableFields_dynCol, value: dynMatchingField, optionsText: "fieldTitle", optionsValue: "fieldName"' title="Matching column"></select>
                            </td>
                        </tr>
                        <tr><td style="font-weight: bold">Filter fields</td></tr>
                        <tr>
                            <td class="ms-authoringcontrols" style="padding-left: 5px; padding-bottom: 5px">
                                <input type="text" data-bind='value: dynFilterFieldsColl' class="ms-input" style="width: 365px;" title="Filter fields" />
                            </td>
                        </tr>
                    </table>
                    <table border="0" cellpadding="0" cellspacing="0">
                        <tbody data-bind='foreach: DynamicConditions'>
                            <tr><td>
                                <table width="100%" data-bind='style: { backgroundColor: color }' cellpadding="3" cellspacing="1" border="0">
                                    <tr>
                                        <td style="font-weight: bold" data-bind='text: ("Filter " + ($index() + 1))'></td>
                                        <td rowspan="4" valign="middle" align="center">
                                            <a title="Remove" href="#" data-bind='click: $parent.removeDynamicCondition, visible: ($parent.DynamicConditions().length > 1)'>
                                                <img class="ms-advsrch-img" src="/_layouts/images/IMNDND.PNG" alt="" />
                                            </a>
                                        </td>
                                    </tr>
                                    <tr><td>
                                        <textarea data-bind='value: camlQuery' class="ms-input" title="Color" rows="3" cols="50"></textarea>
                                    </td></tr>
                                    <tr>
                                        <td style="font-weight: bold" data-bind='text: ("Color " + ($index() + 1))'></td>
                                    </tr>
                                    <tr><td><input type="text" data-bind='value: color, jscolor: {}' class="ms-input" maxlength="255" title="Color" /></td></tr>
                                    <tr>
                                        <td style="font-weight: bold" data-bind='text: ("Font color " + ($index() + 1))'></td>
                                    </tr>
                                    <tr><td><input type="text" data-bind='value: fontColor, jscolor: {}' class="ms-input" maxlength="255" title="Font color" /></td></tr>
                                </table>
                            </td></tr>
                        </tbody>
                        <tfoot>
                            <tr><td class="ms-addnew">
                                <span style="POSITION: relative; WIDTH: 10px; DISPLAY: inline-block; HEIGHT: 10px; OVERFLOW: hidden" class="s4-clust">
                                    <img style="POSITION: absolute; TOP: -128px !important; LEFT: 0px !important" alt="" src="/_layouts/images/fgimg.png" />
                                </span>&nbsp;<a title="Add condition" href="#" data-bind='click: addDynamicCondition' class="ms-addnew">Add Dynamic condition</a>
                            </td></tr>
                        </tfoot>
                    </table>
                    <input type="hidden" id="hidMatchingField" data-bind="value: dynMatchingField" runat="server" />
                    <input type="hidden" id="hidFilterFields" data-bind="value: dynFilterFieldsColl" runat="server" />
                    <input type="hidden" id="hidDynamicConditions" data-bind="value: ko.toJSON(DynamicConditions)" runat="server" />
                    <script type="text/javascript">
                        var hidDynamicMatchingFld_Value = document.getElementById('<%= hidMatchingField.ClientID %>').value;
                        var hidDynamicFilterFields_Value = document.getElementById('<%= hidFilterFields.ClientID %>').value;
                        var hidDynamicConditions_Value = JSON.parse(document.getElementById('<%= hidDynamicConditions.ClientID %>').value);
                    </script>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewStaticColorHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewStaticColor.ClientID%>"),document.getElementById("imgViewStaticColor"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewStaticColor" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0"/>&nbsp; Static colour-coding</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewStaticColor" style="display: none" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Add Static colour-coding settings to this view.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <table border="0" cellpadding="0" cellspacing="0">
                        <tbody data-bind='foreach: StaticConditions'>
                            <tr><td>
                                <table width="100%" data-bind='style: { backgroundColor: color }' cellpadding="4" cellspacing="1" border="0">
                                    <tr>
                                        <td><select data-bind='options: $parent.availableFields, value: Id, optionsText: "fieldTitle", optionsValue: "fieldName"' title="Column to Filter"></select></td>
                                        <td rowspan="4" valign="middle">
                                            <a title="Remove" href="#" data-bind='click: $parent.removeStaticCondition, visible: ($parent.StaticConditions().length > 1)'>
                                                <img class="ms-advsrch-img" src="/_layouts/images/IMNDND.PNG" alt="" />
                                            </a>
                                        </td>
                                    </tr>
                                    <tr><td><select data-bind='options: $parent.availableOperators, value: camlOperator, optionsText: "displayName", optionsValue: "camlOper"' title="Operator"></select></td></tr>
                                    <tr data-bind='visible: (camlOperator() != "IsNull" && camlOperator() != "IsNotNull")'>
                                        <td><input type="text" data-bind='value: filterValue' class="ms-input" maxlength="255" title="Value" /></td>
                                    </tr>
                                    <tr><td>
                                        <span style="display: block; color: Gray; font-size: 10px;">Background color:</span>
                                        <input type="text" data-bind='value: color, jscolor: {}' class="ms-input" maxlength="255" title="Color" />
                                    </td></tr>
                                    <tr><td>
                                        <span style="display: block; color: Gray; font-size: 10px;">Font color:</span>
                                        <input type="text" data-bind='value: fontColor, jscolor: {}' class="ms-input" maxlength="255" title="Font color" />
                                    </td></tr>
                                </table>
                            </td></tr>
                        </tbody>
                        <tfoot>
                            <tr><td class="ms-addnew">
                                <span style="POSITION: relative; WIDTH: 10px; DISPLAY: inline-block; HEIGHT: 10px; OVERFLOW: hidden" class="s4-clust">
                                    <img style="POSITION: absolute; TOP: -128px !important; LEFT: 0px !important" alt="" src="/_layouts/images/fgimg.png" />
                                </span>&nbsp;<a title="Add condition" href="#" data-bind='click: addStaticCondition' class="ms-addnew">Add Static condition</a>
                            </td></tr>
                        </tfoot>
                    </table>
                    <input type="hidden" id="hidStaticConditions" data-bind="value: ko.toJSON(StaticConditions)" runat="server" />
                    <script type="text/javascript">
                        var hidStaticConditions_Value = JSON.parse(document.getElementById('<%= hidStaticConditions.ClientID %>').value);
                    </script>
                    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/colour_coding.model.js"></script>
	            </td>
	        </tr>
	        <tr><td colspan="2">&nbsp;</td><td height="21" class="ms-authoringcontrols" colspan="2">&nbsp;</td></tr>
	    </tbody>

        <tbody id="tbodyViewTooltipHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="2">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("<%= tbodyViewTooltip.ClientID%>"),document.getElementById("imgViewTooltip"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgViewTooltip" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0"/>&nbsp; Tooltips</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
        </tbody>
        <tbody id="tbodyViewTooltip" style="display: none" runat="server">
		    <tr>
                <td nowrap="nowrap"></td>
				<td class="ms-descriptiontext" valign="top">
					<table border="0" cellspacing="0" cellpadding="1">
						<tr><td class="ms-descriptiontext">Specify tooltip fields and  their position for this view.</td><td>&nbsp;</td></tr>
					</table>
				</td>
				<td width="10" class="ms-authoringcontrols">&nbsp;</td>
				<td class="ms-authoringControls" valign="top">
                    <asp:GridView ID="TooltipColumnsGrid" runat="server" AutoGenerateColumns="false" OnRowDataBound="ViewColumnsGrid_RowDataBound"
                        DataKeyNames="Id" Width="100%" CellPadding="2" BorderWidth="0" GridLines="None">
                        <Columns>
                            <asp:TemplateField HeaderText="Display" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" id="chIsSelected" AutoPostBack="false"></asp:CheckBox> 
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DisplayName" HeaderText="Column Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <asp:TemplateField HeaderText="Position" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:DropDownList runat="server" id="ddlColumnPosition" AutoPostBack="false"></asp:DropDownList> 
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
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
                            <td><asp:Button ID="btnRemove" runat="server" Text="Remove" CssClass="ms-ButtonHeightWidth" OnClick="btnRemove_Click" /></td>
                        </tr>
                    </table>
			    </td>
            </tr>
        </tfoot>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Edit View
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Edit View
</asp:Content>
