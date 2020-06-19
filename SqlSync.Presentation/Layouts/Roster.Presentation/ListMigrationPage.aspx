<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListMigrationPage.aspx.cs" Inherits="Roster.Presentation.Layouts.ListMigrationPage" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <link href="/_layouts/15/Roster.Presentation/js/jquery-ui.progressbar.min.css" type="text/css" rel="stylesheet" />
    <style type="text/css">
      .ui-progressbar {
        position: relative;
      }
      .progress-label {
        position: absolute;
        left: 50%;
        top: 4px;
        font-weight: bold;
        text-shadow: 1px 1px 0 #fff;
      }
  </style>
    <script type="text/javascript" src="/_layouts/15/ScriptResx.ashx?name=sp.res&culture=en-us"></script>
    <script type="text/javascript" src="/_layouts/15/sp.init.js"></script>
    <script type="text/javascript" src="/_layouts/15/SP.UI.Dialog.js"></script>
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
    <script type="text/javascript" src="/_layouts/15/Roster.Presentation/js/jquery-ui.progressbar.min.js"></script>

    <script type="text/javascript">
        SP.SOD.executeOrDelayUntilEventNotified(BodyLoaded, "sp.bodyloaded");

        function OpenDbFieldNewPage(ctrl) {
            var ctrlRosterTbls = document.getElementById('<%=ddlRosterTables.ClientID %>');
            if (ctrl.value == '_new_field_') {
                var options = SP.UI.$create_DialogOptions();
                options.url = _spPageContextInfo.webServerRelativeUrl + '/_layouts/15/Roster.Presentation/DbFldNew.aspx?List=' + ctrlRosterTbls.value;
                options.width = 600;
                options.height = 800;
                options.dialogReturnValueCallback = function (result, target) {
                    ctrl.value = '';
                    if (result == SP.UI.DialogResult.OK) {
                        SP.UI.ModalDialog.RefreshPage(SP.UI.DialogResult.OK);
                    }
                };
                SP.UI.ModalDialog.showModalDialog(options);
            }
        }

        function BodyLoaded() {
            var progressbar = jQuery("#progressbar"),
                progressLabel = jQuery(".progress-label");

            progressbar.progressbar({
                value: false,
                change: function () {
                    progressLabel.text(progressbar.progressbar("value") + "%");
                },
                complete: function () {
                    progressLabel.text("Complete!");
                }
            });

            var ctrlMigrateBtn = document.getElementById('<%=btnMigrate.ClientID %>');
            jQuery(ctrlMigrateBtn).click(function () {
                jQuery('#progressbar-wrapper').show();
                var migBtn = document.getElementById('<%=btnMigrate.ClientID %>');
                jQuery(migBtn).prop('disabled', true);

                UpdateProgressBar('', BuildMappingObject());
                return false;
            });
        }

        function BuildMappingObject() {
            var mapping = {};
            mapping.ListId = document.getElementById('<%=ddlSharePointLists.ClientID %>').value;
            mapping.TableId = document.getElementById('<%=ddlRosterTables.ClientID %>').value;
            mapping.FieldMapping = [];
            mapping.ContentTypeMapping = [];
            // fields
            jQuery('#fieldsRepeaterBody > tr').each(function () {
                var ddlTableField = jQuery(this).find('select:first');
                if (ddlTableField.length && ddlTableField.val() != '' && ddlTableField.val() != '_new_field_') {
                    var hidListField = jQuery(this).find('input[type="hidden"]:first');
                    mapping.FieldMapping.push({ ListFieldName: hidListField.val(), TableColumnName: ddlTableField.val() });
                }
            });
            // content types
            jQuery('#contentTypesRepeaterBody > tr').each(function () {
                var ddlTableCt = jQuery(this).find('select:first');
                if (ddlTableCt.length && ddlTableCt.val() != '') {
                    var hidListCt = jQuery(this).find('input[type="hidden"]:first');
                    mapping.ContentTypeMapping.push({ ListCtId: hidListCt.val(), TableCtId: ddlTableCt.val() });
                }
            });

            return mapping;
        }

        function UpdateProgressBar(val, mapping) {
            console.log('UpdateProgressBar...');
            var curIndex = jQuery("#progressbar").progressbar("value") || 0;

            jQuery.ajax({
                type: "POST", processData: false, async: true,
                url: "ListMigrationPage.aspx/MigrateItems",
                contentType: "application/json; charset=utf-8", dataType: "json",
                data: JSON.stringify({
                    query: {
                        PaginInfo: val,
                        CurrentIndex: curIndex,
                        Mapping: mapping
                    }
                }),
                success: function (Result) {
                    if (Result.d.Success) {
                        jQuery("#progressbar").progressbar("value", Result.d.CurrentIndex); //parseFloat(Result.d.CurrentIndex).toFixed(2)
                        if (Result.d.PagingInfo) {
                            setTimeout(UpdateProgressBar.bind(null, Result.d.PagingInfo, mapping), 100);
                        } else {
                            var migBtn = document.getElementById('<%=btnMigrate.ClientID %>');
                            jQuery(migBtn).prop('disabled',false);
                        }
                    } else {
                        jQuery('#progressbar-wrapper').hide();
                        jQuery("#progressbar").progressbar("value", 0);
                        var migBtn = document.getElementById('<%=btnMigrate.ClientID %>');
                        jQuery(migBtn).prop('disabled', false);
                        alert(Result.d.ErrorMessage);
                    }
                },
                error: function (xhr, status, error) {
                    jQuery('#progressbar-wrapper').hide();
                    jQuery("#progressbar").progressbar("value", 0);
                    var migBtn = document.getElementById('<%=btnMigrate.ClientID %>');
                    jQuery(migBtn).prop('disabled', false);
                    alert(error);
                }
            });
        }

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

    <table width="600px" border="0" cellpadding="2">
      <thead>
        <tr>
            <th align="left">
                <asp:DropDownList ID="ddlSharePointLists" runat="server" AutoPostBack="true" DataTextField="Title" DataValueField="Id" OnSelectedIndexChanged="ddlShPLists_SelectedIndexChanged" />
            </th>
            <th style="width: 15px">&nbsp;</th>
            <th align="left">
                <asp:DropDownList ID="ddlRosterTables" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRosterTables_SelectedIndexChanged">
                    <asp:ListItem Text="Planned Rosters" Value="5B7156BB-84A5-4F8C-AE2B-BCDE4ED9C486" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Working Rosters" Value="23E6F5B9-1843-4F9B-AAAB-20F45EBB946B"></asp:ListItem>
                </asp:DropDownList>
            </th>
        </tr>
      </thead>
      <tbody id="fieldsRepeaterBody">
          <asp:Repeater ID="ColumnsMapperRep" runat="server" OnItemDataBound="ColumnsMapperRep_ItemDataBound">
            <ItemTemplate>
                <tr>
                    <td align="left">
                        [<asp:HyperLink runat="server" NavigateUrl='<%# Eval("ListColumnEditPageUrl") %>' Text='<%# Eval("ListColumnTitle") %>' ToolTip='<%# Eval("ListColumnToolTip") %>' />]
                        <asp:HiddenField ID="hidListColumnId" runat="server" Value='<%# Eval("ListColumnInternalName") %>' />
                    </td>
                    <td align="center" style="padding: 1px 20px"><=></td>
                    <td align="left">
                        <asp:DropDownList ID="ddlTableFields" runat="server" AutoPostBack="false" AppendDataBoundItems="true" onchange="OpenDbFieldNewPage(this)"
                            Visible='<%# Eval("ListColumnIsSupported").ToString()=="True" %>'>
                            <asp:ListItem Text="" Value=""></asp:ListItem>
                            <asp:ListItem Text="Add new field..." Value="_new_field_"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:Label ID="lblNotSupported" runat="server" Text="-not supported-" Visible='<%# Eval("ListColumnIsSupported").ToString()!="True" %>' ForeColor="Red" />
                    </td>
                </tr>
            </ItemTemplate>
          </asp:Repeater>

      </tbody>
      <tbody id="tbodyViewColumnsHeader" runat="server">
            <tr>
                <td height="28" class="ms-sectionheader" colspan="3">
					<h3 class="ms-standardheader">
                        <a onclick='javascript:ShowHideGroup(document.getElementById("contentTypesRepeaterBody"),document.getElementById("imgContentTypes"));return false;' href="javascript:ShowHideGroup()">
					    <img id="imgContentTypes" alt="" src="/_layouts/15/images/plus.gif?rev=23" border="0"/>&nbsp; ContentType mapping</a>
					</h3></td>
			    <td height="28" class="ms-authoringcontrols" colspan="2">&nbsp;</td>
            </tr>
      </tbody>
      <tbody id="contentTypesRepeaterBody" style="display: none">

          <asp:Repeater ID="ContentTypesMapperRepeater" runat="server" OnItemDataBound="ContentTypesMapperRepeater_ItemDataBound">
            <ItemTemplate>
                <tr>
                    <td align="left">
                        [<asp:HyperLink runat="server" NavigateUrl='<%# Eval("ListCtEditPageUrl") %>' Text='<%# Eval("ListCtTitle") %>' />]
                        <asp:HiddenField ID="hidListCtId" runat="server" Value='<%# Eval("ListCtId") %>' />
                    </td>
                    <td align="center" style="padding: 1px 20px"><=></td>
                    <td align="left">
                        <asp:DropDownList ID="ddlTableContentTypes" runat="server" AutoPostBack="false" />
                    </td>
                </tr>
            </ItemTemplate>
          </asp:Repeater>

      </tbody>
      <tfoot>
          <tr><td></td><td></td><td></td></tr>
          <tr><td colspan="3">
              <asp:Panel ID="pnlError" runat="server"></asp:Panel>
              <div id="progressbar-wrapper" style="padding: 5px; display: none;">
                  <div id="progressbar"><div class="progress-label">Migrating...</div></div>
              </div>
          </td></tr>
          <tr><td colspan="3" align="right">
              <table border="0">
                <tr>
                    <td><asp:Button ID="btnMigrate" runat="server" Text="Migrate List items" CssClass="ms-ButtonHeightWidth" /></td>
                    <td><SharePoint:GoBackButton runat="server" ControlMode="New" ID="btnGoBack"></SharePoint:GoBackButton></td>
                </tr>
              </table>
          </td></tr>
      </tfoot>
    </table>

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Migration Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Migrate SharePoint List items to Roster DB
</asp:Content>
