using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Roster.Common;
using Roster.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Roster.Presentation.Extensions;
using System.Collections.Specialized;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using System.Reflection;

namespace Roster.Presentation.Helpers
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

        public static List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>> GetWhereCriteriaFromFilterExpression(ViewMetadata view, string filterExpression)
        {
            var whereCriterias = new List<Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>>();

            if (!string.IsNullOrEmpty(filterExpression))
            {
                string[] _conditions = filterExpression.Split(new string[] { " AND " }, StringSplitOptions.None);
                foreach (string cond in _conditions)
                {
                    string[] fValues = cond.Split(new string[] { " = " }, StringSplitOptions.None);
                    if (fValues.Length > 1)
                    {
                        var filterField = view.ListMetadata.ListMetadataFields.FirstOrDefault(item => item.InternalName == fValues[0]);
                        object fieldValue = fValues[1].Trim(new char[] { '\'' });
                        var _dbFieldType = filterField.GetDbField().Type;
                        if (_dbFieldType == SPFieldType.DateTime)
                            fieldValue = DateTime.Parse(fieldValue.ToString());
                        if (_dbFieldType == SPFieldType.Boolean)
                            fieldValue = (fieldValue.ToString() == "Yes");
                        whereCriterias.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(filterField, CompareType.Equal, ConcateOperator.And, fieldValue, null));
                    }
                }
            }

            return whereCriterias;
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

        public static void AddRibbonButtonsToPage(List<ListMetadataAction> ribbonBtns, Page page)
        {
            // Gets the current instance of the ribbon on the page.
            SPRibbon ribbon = SPRibbon.GetCurrent(page);
            XmlDocument ribbonExtensions = new XmlDocument();

            //Load the contextual tab XML and register the ribbon.
            ribbonExtensions.LoadXml(string.Format(Constants.Ribbon.VIEW_TAB_TEMPLATE,
                string.Join("", ribbonBtns.Select(b => b.GetButtonAsXml()))));
            ribbon.RegisterDataExtension(ribbonExtensions.FirstChild, "Ribbon.Tabs._children");
            //Load the custom templates and register the ribbon.
            ribbonExtensions.LoadXml(Constants.Ribbon.CONTEXTUAL_TAB_TEMPLATE);
            ribbon.RegisterDataExtension(ribbonExtensions.FirstChild, "Ribbon.Templates._children");

            ribbon.Minimized = false;
            ribbon.CommandUIVisible = true;
            const string initialTabId = "Ribbon.RosterTab";
            if (!ribbon.IsTabAvailable(initialTabId)) { ribbon.MakeTabAvailable(initialTabId); }
            ribbon.InitialTabId = initialTabId;

            // REGISTER COMMANDS
            var commands = new List<IRibbonCommand>();

            // register the command at the ribbon. Include the callback to the server to generate the xml
            ribbonBtns.ToList().ForEach(b => {
                commands.Add(new SPRibbonCommand(b.GetCommandName(), b.Command, "true"));
            });

            //Register initialize function
            var manager = new SPRibbonScriptManager();
            var methodInfo = typeof(SPRibbonScriptManager).GetMethod("RegisterInitializeFunction", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(manager, new object[] { 
                    page, "InitPageComponent", "/_layouts/15/Roster.Presentation/js/PageComponent.js", false, "Roster.PageComponent.initialize()" });

            // Register ribbon scripts
            manager.RegisterGetCommandsFunction(page, "getGlobalCommands", commands);
            manager.RegisterCommandEnabledFunction(page, "commandEnabled", commands);
            manager.RegisterHandleCommandFunction(page, "handleCommand", commands);
        }
    }
}
