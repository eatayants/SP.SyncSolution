using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Xml.Serialization;

namespace Roster.Presentation.WebParts.AvailabilityButtonWebPart
{
    [ToolboxItemAttribute(false)]
    public class AvailabilityButtonWebPart : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/AvailabilityButtonWebPart/AvailabilityButtonWebPartUserControl.ascx";

        #region Private properties

        private string _filterConditions = string.Empty;
        private string _redirectUrl = "/"; // default value
        private string _listName = "Scheduled Shifts"; // default value
        private string _viewName = "";
        private string _calendarPeriod = "month"; // default value
        private string _dateFormat = "d/M/yyyy"; // default value

        #endregion   

        #region Custom WebPart properties

        [Category("Custom Properties"),
         Personalizable(PersonalizationScope.Shared),
         WebDisplayName("Filter conditions"),
         Description("eg.: "),
         WebBrowsable(true),
         XmlElement(ElementName = "FilterConditions")]
        public string FilterConditions
        {
            get
            {
                return _filterConditions;
            }
            set
            {
                _filterConditions = value;

                // set value to Cache
                string _key = "FilterConditionsKey";
                object cacheObj = System.Web.HttpContext.Current.Cache[_key];
                if (cacheObj == null || (cacheObj != null && cacheObj.ToString() != _filterConditions)) {
                    System.Web.HttpContext.Current.Cache[_key] = _filterConditions;
                }
            }
        }

        [Category("Custom Properties"),
         Personalizable(PersonalizationScope.Shared),
         WebDisplayName("Redirect url"),
         WebBrowsable(true),
         XmlElement(ElementName = "RedirectUrl")]
        public string RedirectUrl
        {
            get
            {
                return _redirectUrl;
            }
            set
            {
                _redirectUrl = value;
            }
        }

        [Category("Custom Properties"),
         Personalizable(PersonalizationScope.Shared),
         WebDisplayName("CalendarPeriod"),
         WebBrowsable(true),
         XmlElement(ElementName = "CalendarPeriod")]
        public string CalendarPeriod
        {
            get
            {
                return _calendarPeriod;
            }
            set
            {
                _calendarPeriod = value;
            }
        }

        [Category("Custom Properties"),
         Personalizable(PersonalizationScope.Shared),
         WebDisplayName("DateFormat"),
         WebBrowsable(true),
         XmlElement(ElementName = "DateFormat")]
        public string DateFormat
        {
            get
            {
                return _dateFormat;
            }
            set
            {
                _dateFormat = value;
            }
        }

        #endregion

        protected override void CreateChildControls()
        {
            AvailabilityButtonWebPartUserControl control = Page.LoadControl(_ascxPath) as AvailabilityButtonWebPartUserControl;
            control.FilterConditions = this.FilterConditions;
            control.RedirectUrl = this.RedirectUrl;
            control.CalendarPeriod = this.CalendarPeriod;
            control.DateFormat = this.DateFormat;

            Controls.Add(control);
        }
    }
}
