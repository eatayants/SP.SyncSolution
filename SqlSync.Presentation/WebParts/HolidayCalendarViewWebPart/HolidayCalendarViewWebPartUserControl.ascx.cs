using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Presentation.Helpers;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Common;

namespace Roster.Presentation.WebParts.HolidayCalendarViewWebPart
{
    public partial class HolidayCalendarViewWebPartUserControl : UserControl
    {
        private DateOptions _currentDateOptions;
        protected SPCalendarView spCalendarHolidayView;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            try
            {
                SPWeb web = SPContext.Current.Web;

                spCalendarHolidayView = new SPCalendarView();
                spCalendarHolidayView.EnableViewState = true;
                spCalendarHolidayView.EnableV4Rendering = web.UIVersion >= 4;
                spCalendarHolidayView.NewItemFormUrl = web.ServerRelativeUrl.TrimEnd('/')     + Constants.Pages.HOLIDAY_FORM_PAGE_URL + "?Mode=3";
                spCalendarHolidayView.EditItemFormUrl = web.ServerRelativeUrl.TrimEnd('/')    + Constants.Pages.HOLIDAY_FORM_PAGE_URL + "?Mode=2";
                spCalendarHolidayView.DisplayItemFormUrl = web.ServerRelativeUrl.TrimEnd('/') + Constants.Pages.HOLIDAY_FORM_PAGE_URL + "?Mode=1";

                string viewType = this.CalendarPeriod;
                string selectedDate = SPUtility.GetSelectedDate(Request, web);

                Tuple<DateTime, DateTime> calendarPeriod = Utils.GetCalendarViewPeriod(SPContext.Current.Web.GetDateOptions(Request), viewType);
                spCalendarHolidayView.ViewType = viewType;
                spCalendarHolidayView.SelectedDate = selectedDate;

                // Bind the datasource to the SPCalendarView
                var ds = new SPCalendarItemCollection();
                short calendarType = web.RegionalSettings.CalendarType;
                var holidays = new RosterDataService().ListHolidayEvents().OrderBy(item => item.HolidayDate).ToList();
                ds.AddRange(holidays.Select(item => new SPCalendarItem()
                {
                    DisplayFormUrl = web.ServerRelativeUrl.TrimEnd('/') + Constants.Pages.HOLIDAY_FORM_PAGE_URL + "?Mode=1",
                    CalendarType = calendarType,
                    ItemID = item.Id.ToString(),
                    StartDate = item.HolidayDate.Date,
                    EndDate = item.HolidayDate.Date.AddSeconds(86400),
                    hasEndDate = true,
                    Title = item.Holiday.Name,
                    Location = item.Holiday.HolidayType.Name,
                    IsAllDayEvent = true,
                    IsRecurrence = false
                }).ToList());

                // data bind
                spCalendarHolidayView.DataSource = ds;
                spCalendarHolidayView.DataBind();

                // add Calendar to page
                this.Controls.Add(spCalendarHolidayView);
            }
            catch (Exception ex)
            {
                Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                Controls.Add(new Label() { Text = " StackTrace: " + ex.StackTrace, ForeColor = System.Drawing.Color.Red });
            }
        }

        #region Private methods

        private string CalendarPeriod
        {
            get
            {
                string viewType = SPUtility.GetViewType(Request);
                if (string.IsNullOrEmpty(viewType)) {
                    viewType = "month";
                }
                return viewType;
            }
        }

        #endregion
    }
}
