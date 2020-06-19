<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExternalListViewWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.ExternalListViewWebPart.ExternalListViewWebPartUserControl" %>
<%@ Register TagPrefix="total" Namespace="Roster.Presentation.Controls" Assembly="$SharePoint.Project.AssemblyFullName$" %>

<style type="text/css">

    .handsontable th { background-color: white !important; }
    .ht_clone_top { display: none; }
    .ms-unselectedtitle, .ms-selectedtitle, .ms-unselectedtitle td, .ms-selectedtitle td { border: 0px !important; }
    .ms-unselectedtitle td, .ms-selectedtitle td { white-space: nowrap !important; }
    .wtHolder { height: auto !important; }
    .ht_master, .handsontable th { overflow: visible !important; } /*.ht_master .wtHolder, */
    .checkboxlist-row { display:block;}
    .checklistHolder { border: 1px solid #CCC; padding: 5px; background-color: #fff;}
    .checklistBtn { min-width: 20px !important; width: 25px !important; height: 20px !important;padding:1px !important;margin-left: 3px !important; }
    .tbl-original-hidden table.ms-listviewtable { display: none; }
    .timesheet-holder #dayTabLinkId, .timesheet-holder #weekTabLinkId, .timesheet-holder #monthTabLinkId  { display: none; }
    .always-hidden { display: none; }

</style>

<link href="/_layouts/15/Roster.Presentation/js/select2.min.css" type="text/css" rel="stylesheet" />
<link href="/_layouts/15/Roster.Presentation/js/jquery-ui.datepicker.min.css" type="text/css" rel="stylesheet" />
<link href="/_layouts/15/Roster.Presentation/js/handsontable.full.min.css" rel="stylesheet"/>

<script src="/_layouts/15/mQuery.js" type="text/javascript"></script>
<script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/select2.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/jquery-ui.datepicker.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/moment.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/pikaday.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/handsontable.full.min.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/linq.min.js" type="text/javascript"></script>
<script src="/_layouts/15/Roster.Presentation/js/tooltips.manager.js?rev=20161017" type="text/javascript"></script>
<script src="/_layouts/15/Roster.Presentation/js/overwrite.standard.functions.js?rev=20160723" type="text/javascript"></script>
<script src="/_layouts/15/Roster.Presentation/js/handsontable.editors.custom.js"></script>
<script src="/_layouts/15/Roster.Presentation/js/quickEdit.js?rev=20150922"></script>

<script type="text/javascript">
    var $j = jQuery.noConflict();

    $j(document).ready(function () {
        $j('.ms-vb > a.ms-core-menu-root').each(function () {
            var thisEntry = $j(this);
            var href = thisEntry.attr('href');
            thisEntry.attr('href', 'javascript:;');
            thisEntry.on('click', function () { SP.UI.ModalDialog.ShowPopupDialog(href); });
        });
    });
</script>

<asp:HiddenField ID="hidFiltersHistory" ClientIDMode="Static" runat="server" />