<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HolidayCalendarViewWebPartUserControl.ascx.cs" Inherits="Roster.Presentation.WebParts.HolidayCalendarViewWebPart.HolidayCalendarViewWebPartUserControl" %>

<script src="/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js"></script>
<script type="text/javascript">

    SP.SOD.executeOrDelayUntilEventNotified(RegisterOnloadEvents, "sp.bodyloaded");

    function RegisterOnloadEvents() {

        ExecuteOrDelayUntilScriptLoaded(function () {
            RegisterEventOnDayDoubleClick();

            AddPosibilityToOpenEventsInDialog();
        }, "sp.ui.applicationpages.calendar.js");
        
    }

    function RegisterEventOnDayDoubleClick() {

        jQuery('tr.ms-acal-hour30 > td, tr.ms-acal-hour00 > td').dblclick(function (e) {
            var selectedCell = jQuery(this);
            if (selectedCell.tagName !== 'TD')
                selectedCell = selectedCell.closest('td');
            var closestRow = selectedCell.closest('tr');

            // get calendar view header row
            var date;
            var headerRow = selectedCell.closest('table').find('tr.ms-acal-week-top').first();
            if (headerRow.length) {
                // WEEK view mode
                // get cell index in parent row
                var columnIndex = 0;
                var siblinsTDs = closestRow.find('td');
                for (var i = 0; i < siblinsTDs.length; i++) {
                    if (siblinsTDs[i] == selectedCell[0]) {
                        columnIndex = i;
                        break;
                    }
                }

                // get cell by index from header row and extract date info
                var rowDate = jQuery(headerRow.find('td')[columnIndex]).attr('date');
                date = Date._parseExact(rowDate, Sys.CultureInfo.InvariantCulture.dateTimeFormat.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
                if (date == null) {
                    date = Date._parseExact(rowDate, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
                }
            } else {
                // DAY view mode
                var dayAsStr = GetInnerText(jQuery('div.ms-acal-header').find('span.ms-acal-display')[0]).trim();
                date = Date._parseExact(dayAsStr, 'dddd, MMMM d, yyyy', Sys.CultureInfo.InvariantCulture);
            }

            SP.UI.ModalDialog.ShowPopupDialog(String.format('{0}/_layouts/15/Roster.Presentation/HolidayFormPage.aspx?Mode=3&HolidayDate={1}',
                _spPageContextInfo.webServerRelativeUrl,
                date.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z'));
        });

        jQuery('tr.ms-acal-summary-itemrow div').dblclick(function (e) {
            // MONTH view mode

            // try find clicked Date on previous row
            var selectedCell = jQuery(this);
            if (selectedCell.tagName !== 'TD')
                selectedCell = selectedCell.closest('td');
            var closestRow = selectedCell.closest('tr');

            // get cell index in parent row
            var columnIndex = 0;
            var siblinsTDs = closestRow.find('td');
            for (var i = 0; i < siblinsTDs.length; i++) {
                if (siblinsTDs[i] == selectedCell[0]) {
                    columnIndex = i;
                    break;
                }
            }

            var date = '';
            try {
                // get date from previous row
                date = jQuery(jQuery(closestRow[0].previousSibling).find('td')[columnIndex]).attr('date');
            } catch (e) { }

            var dt = Date._parseExact(date, Sys.CultureInfo.InvariantCulture.dateTimeFormat.ShortDatePattern, Sys.CultureInfo.InvariantCulture);
            if (dt == null) {
                dt = Date._parseExact(date, 'dd/MM/yyyy', Sys.CultureInfo.InvariantCulture);
            }

            SP.UI.ModalDialog.ShowPopupDialog(String.format('{0}/_layouts/15/Roster.Presentation/HolidayFormPage.aspx?Mode=3&HolidayDate={1}',
                _spPageContextInfo.webServerRelativeUrl,
                dt.format(Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern) + 'Z'));
        });
    }

    function AddPosibilityToOpenEventsInDialog() {
        // month Calendar view :: override 'renderGrids' function
        var proxiedRenderGridsSummary = SP.UI.ApplicationPages.SummaryCalendarView.prototype.renderGrids;
        SP.UI.ApplicationPages.SummaryCalendarView.prototype.renderGrids = function () {
            proxiedRenderGridsSummary.apply(this, arguments);

            AddDialogModeToEvents();
        }

        // day or week Calendar view :: override 'renderGrids' function
        var proxiedRenderGridsDetail = SP.UI.ApplicationPages.DetailCalendarView.prototype.renderGrids;
        SP.UI.ApplicationPages.DetailCalendarView.prototype.renderGrids = function () {
            proxiedRenderGridsDetail.apply(this, arguments);

            AddDialogModeToEvents();
        }
    }

    function AddDialogModeToEvents() {
        try {
            jQuery('div.ms-acal-item').each(function () {
                var aElem = jQuery(this).find('a');
                aElem.prop('onclick', null)
                     .click(function () {
                         SP.UI.ModalDialog.OpenPopUpPage(this.href, function () { SP.UI.ModalDialog.RefreshPage(true); });
                         return false;
                     });
            });
        } catch (e) { }
    }

</script>

<asp:Panel ID="MainPanel" runat="server"></asp:Panel>