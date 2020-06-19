using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Roster.Presentation.Helpers;

namespace Roster.Presentation.WebParts.AvailabilityButtonWebPart
{
    public partial class AvailabilityButtonWebPartUserControl : UserControl
    {
        #region Props

        public string DateFormat { get; set; }

        public string RedirectUrl { get; set; }

        public string FilterConditions { get; set; }

        public string CalendarPeriod { get; set; }

        #endregion

        public NameValueCollection FilterQuery
        {
            get
            {
                if (!string.IsNullOrEmpty(hidHashValue.Value))
                {
                    //#InplviewHashd4d48185-a7b1-4c94-bbc3-fea96acda0c7=FilterField1=FullName1-FilterValue1=Percival%2DCarman-FilterField2=Region-FilterValue2=Shelbyville
                    // remove hash key and replace '-' to '&'
                    string queryString = SPHttpUtility.UrlKeyValueDecode(hidHashValue.Value.Substring(50).Replace('-', '&'));
                    return System.Web.HttpUtility.ParseQueryString(queryString);
                }
                else
                {
                    return new NameValueCollection();
                }
            }
        }

        private string FormatedCalendarDate
        {
            get
            {
                string urlParamTempl1 = "&CalendarDate={0}";
                string urlParamTempl2 = "&CalendarDate={0}&DateFormat={1}";
                string calendarDate = this.Page.Request.QueryString["CalendarDate"];

                if (string.IsNullOrEmpty(calendarDate))
                {
                    return string.Empty; // StartDate is empty
                }
                else
                {
                    if (string.IsNullOrEmpty(this.DateFormat))
                    {
                        return string.Format(urlParamTempl1, SPHttpUtility.UrlKeyValueEncode(calendarDate)); // no Date formating
                    }
                    else
                    {
                        DateTime dt;
                        if (DateTime.TryParseExact(calendarDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            return string.Format(urlParamTempl2,
                                SPHttpUtility.UrlKeyValueEncode(dt.ToString(this.DateFormat)),
                                SPHttpUtility.UrlKeyValueEncode(this.DateFormat));
                        }
                    }
                }

                return string.Empty;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnViewAvailability.Enabled = !string.IsNullOrEmpty(this.RedirectUrl);
            hidFilterOper.Value = this.FilterConditions;
        }

        protected void btnViewAvailability_Click(object sender, EventArgs e)
        {
            SPLimitedWebPartManager webPartManager = null;

            try
            {
                SPList list = null;
                SPView view = null;

                // get WebPartManager to identify current List/View
                SPWeb web = SPContext.Current.Web;
                webPartManager = SPContext.Current.File.GetLimitedWebPartManager(PersonalizationScope.Shared);
                foreach (System.Web.UI.WebControls.WebParts.WebPart wp in webPartManager.WebParts)
                {
                    XsltListViewWebPart xlvwp = wp as XsltListViewWebPart;
                    if (xlvwp != null) {
                        list = web.Lists[xlvwp.ListId];
                        view = xlvwp.View;
                        break;
                    }
                }

                // get list data by Filters
                FieldFilterOperatorsLayer fol = new FieldFilterOperatorsLayer(this.FilterConditions);
                CamlFiltersLayer fl = new CamlFiltersLayer(list, this.FilterQuery, fol);
                string myQuery = fl.GetQueryByFilters();
                SPListItemCollection navigator = list.GetItems(new SPQuery() { Query = myQuery, ViewFields = "<FieldRef Name=\"ID\" />", RowLimit = view.RowLimit });

                // get filter IDs
                string filteredIDs = string.Empty;
                if (navigator != null && navigator.Count > 0)
                    filteredIDs = String.Join(",", navigator.Cast<SPListItem>().Select(x => x.ID.ToString()).ToArray());

                string redigUrl = string.Format("{0}/{1}?FilteredIDs={2}&OrigSchiftId={3}{4}{5}",
                    web.ServerRelativeUrl.TrimEnd('/'), this.RedirectUrl.TrimStart('/'),
                    filteredIDs, this.Page.Request.QueryString["SchiftId"],
                    string.IsNullOrEmpty(this.CalendarPeriod) ? "" : string.Format("&CalendarPeriod={0}", this.CalendarPeriod),
                    FormatedCalendarDate);
                SPUtility.Redirect(redigUrl, SPRedirectFlags.Default, this.Context);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                lblError.Visible = true;
            }
            finally
            {
                if (webPartManager != null)
                    webPartManager.Web.Dispose();
            }
        }
    }
}
