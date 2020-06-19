using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using SqlSync.SP.Extensions;
using System.Collections.Specialized;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using System.Reflection;

namespace SqlSync.SP.Helpers
{
    public class Utils
    {
        public static void GoBackOnSuccess(System.Web.UI.Page page, System.Web.HttpContext context)
        {
            string fullOrRelativeUrl = page.Request.QueryString["Source"];
            if (!string.IsNullOrEmpty(fullOrRelativeUrl)) {
                fullOrRelativeUrl = SPHttpUtility.UrlPathDecode(fullOrRelativeUrl, true);
            }

            string redirectUrl = string.IsNullOrEmpty(fullOrRelativeUrl) ?
                SPContext.Current.Web.ServerRelativeUrl : fullOrRelativeUrl;

            bool isPopUp = (page.Request.QueryString["IsDlg"] != null);
            if (!isPopUp)
            {
                SPUtility.Redirect(redirectUrl, SPRedirectFlags.UseSource, context);
                return;
            }
            else
            {
                context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
                context.Response.Flush();
                context.Response.End();
            }
        }

        public static Tuple<DateTime, DateTime> GetCalendarViewPeriod(DateOptions dateOptions, string calendarScope)
        {
            Tuple<DateTime, DateTime> period;
            SimpleDate selDate = dateOptions.SelectedDate;

            if (calendarScope == "day")
            {
                DateTime dt = new DateTime(selDate.Year, selDate.Month, selDate.Day);
                period = new Tuple<DateTime, DateTime>(dt, dt.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999));
            }
            else
            {
                DateTime firstDayOfCalendarView = (calendarScope == "week") ? new DateTime(selDate.Year, selDate.Month, selDate.Day) : new DateTime(selDate.Year, selDate.Month, 1);
                DateTime lastDayOfCalendarView = (calendarScope == "week") ? new DateTime(selDate.Year, selDate.Month, selDate.Day) : new DateTime(selDate.Year, selDate.Month, DateTime.DaysInMonth(selDate.Year, selDate.Month));

                // get FIRST date of Calendar view
                while ((int)firstDayOfCalendarView.DayOfWeek != dateOptions.FirstDayOfWeek)
                    firstDayOfCalendarView = firstDayOfCalendarView.AddDays(-1);

                // get LAST date of Calendar view
                do {
                    lastDayOfCalendarView = lastDayOfCalendarView.AddDays(1);
                } while ((int)lastDayOfCalendarView.DayOfWeek != dateOptions.FirstDayOfWeek);

                period = new Tuple<DateTime, DateTime>(firstDayOfCalendarView, lastDayOfCalendarView.AddMilliseconds(-1));
            }

            return period;
        }

        public static DayOfWeek GetDayOfWeekByDayShortName(string dayShortName)
        {
            DayOfWeek dayOfWeek = DayOfWeek.Sunday; // default value. Will be 100% overwriten

            switch (dayShortName)
            {
                case "su":
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
                case "mo":
                    dayOfWeek = DayOfWeek.Monday;
                    break;
                case "tu":
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;
                case "we":
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;
                case "th":
                    dayOfWeek = DayOfWeek.Thursday;
                    break;
                case "fr":
                    dayOfWeek = DayOfWeek.Friday;
                    break;
                case "sa":
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
                default:
                    break;
            }

            return dayOfWeek;
        }

        public static string GetValueAfterPrefix(string prefixAndValue, string prefix)
        {
            if (!HasPrefix(prefixAndValue, prefix))
            {
                return null;
            }
            if (prefix.Length == prefixAndValue.Length)
            {
                return string.Empty;
            }
            return prefixAndValue.Substring(prefix.Length);
        }
        public static bool HasPrefix(string prefixAndValue, string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(prefixAndValue)) {
                return false;
            }
            if (prefix.Length > prefixAndValue.Length) {
                return false;
            }
            return (prefixAndValue.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static string GetDateAsSqlString(DateTime dt)
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, "#{0}#", dt);
        }
        public static object[] GetFilterValuesFromQueryString(NameValueCollection queryString, string filterField)
        {
            if (queryString != null && queryString.HasKeys())
            {
                var filterFieldKey = queryString.AllKeys.Where(k => k != null && k.StartsWith("FilterField") && queryString.GetValues(k)[0] == filterField).FirstOrDefault();

                if (string.IsNullOrEmpty(filterFieldKey))
                    return null;

                int num = int.Parse(filterFieldKey.Substring(filterFieldKey.StartsWith("FilterFields") ? 12 : 11), CultureInfo.InvariantCulture);
                var filterValueKey = queryString.AllKeys.Where(k => (k == ("FilterValue" + num)) || (k == ("FilterValues" + num))).FirstOrDefault();
                string filterValue = queryString.GetValues(filterValueKey)[0];

                if (!string.IsNullOrEmpty(filterValue)) {
                    return filterValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return null;
        }
    }
}
