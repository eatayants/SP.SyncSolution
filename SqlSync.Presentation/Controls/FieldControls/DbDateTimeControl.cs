using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;
using Microsoft.SharePoint;
using System.IO;
using System.Reflection;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbDateTimeControl : DbBaseFieldControl
    {
        private DateTimeControl ctrl_DateCtrl;

        private Label ctrl_Label;

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            if (Field == null) return;
            if (Field.ReadOnly || ControlMode == SPControlMode.Display)
            {
                ctrl_Label = new Label {ID = Field.InternalName};
                this.Controls.Add(ctrl_Label);
            }
            else
            {
                ctrl_DateCtrl = new DateTimeControl {
                    ID = Field.InternalName,
                    DateOnly = (((DbFieldDateTime) Field).Format == "DateOnly")
                };

                var regSettings = SPContext.Current.Web.RegionalSettings;
                if (regSettings != null) {
                    ctrl_DateCtrl.Calendar = ((SPCalendarType)regSettings.CalendarType);
                    ctrl_DateCtrl.LocaleId = ((int)regSettings.LocaleId);
                    ctrl_DateCtrl.UseTimeZoneAdjustment = true;
                    ctrl_DateCtrl.HijriAdjustment = regSettings.AdjustHijriDays;
                    ctrl_DateCtrl.HoursMode24 = regSettings.Time24;
                    ctrl_DateCtrl.FirstDayOfWeek = ((int)regSettings.FirstDayOfWeek);
                    ctrl_DateCtrl.FirstWeekOfYear = regSettings.FirstWeekOfYear;
                    ctrl_DateCtrl.ShowWeekNumber = regSettings.ShowWeeks;
                    ctrl_DateCtrl.TimeZoneID = regSettings.TimeZone.ID;
                    ctrl_DateCtrl.DatePickerFrameUrl = SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/') + "/" + ctrl_DateCtrl.DatePickerFrameUrl.TrimStart('/');
                }
                if (Field.Required) 
                {
                    ctrl_DateCtrl.IsRequiredField = true;
                    ctrl_DateCtrl.ErrorMessage = "You can't leave this blank";
                }
                Controls.Add(ctrl_DateCtrl);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
            if (Field.ReadOnly || ControlMode == SPControlMode.Display)
            {
                ctrl_Label.RenderControl(writer); // <label/>
            }
            else
            {
                // HACK
                // manually render control
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    using (var htw = new HtmlTextWriter(sw))
                    {
                        ctrl_DateCtrl.RenderControl(htw);
                    }
                }
                writer.Write(sb.ToString());
                // using this give incorrect input ID ((
                // ctrl_DateCtrl.RenderControl(writer); // <dateTimeControl/>
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return (Field.ReadOnly || ControlMode == SPControlMode.Display) ? 
                    SPUtility.CreateDateTimeFromISO8601DateTimeString(ctrl_Label.Text):ctrl_DateCtrl.SelectedDate.ToLocalTime();
            }
            set
            {
                EnsureChildControls();
	            if (value == null) return;
	            try
	            {
                    bool isDateOnly = ((Field as DbFieldDateTime).Format == "DateOnly");
		            var dt = DateTime.MinValue;
		            if (value.ToString() == "[today]") {
                        dt = isDateOnly ? DateTime.Today : DateTime.Now;
		            } else {
			            dt = value as DateTime? ?? SPUtility.CreateDateTimeFromISO8601DateTimeString(value.ToString());
		            }
		            if (dt == DateTime.MinValue) return;

                    if (Field.ReadOnly || ControlMode == SPControlMode.Display) {
                        ctrl_Label.Text = dt.ToString(isDateOnly ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm");//dt.ToString(CultureInfo.InvariantCulture);
		            } else {
			            ctrl_DateCtrl.SelectedDate = dt.ToUniversalTime();
		            }
	            }
	            catch
	            {
		            // ignored
	            }
            }
        }
    }
}
