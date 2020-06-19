using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;
using System.Xml.Linq;
using System.Globalization;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldRecurrence : DbField
    {
        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Recurrence;
            }
        }

        public override string SqlType
        {
            get
            {
                return "xml";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Recurrence";
            }
        }

        public DbFieldRecurrence(string fieldName) : base(fieldName)
        {}
        public DbFieldRecurrence(Guid fieldId) : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbRecurrenceControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldRecurrence fldSource = sourceField as DbFieldRecurrence;

            if (fldSource != null)
            {
                //this.DefaultValue = fldSource.DefaultValue;
            }
        }

        public override object GetFieldValue(object dityValue)
        {
            if (dityValue == null || string.IsNullOrEmpty(dityValue.ToString()))
                return null;

            string dtext = "Every ";
            try
            {
                XDocument xDoc = XDocument.Parse(dityValue.ToString());
                XElement repeatEl = xDoc.Descendants("repeat").FirstOrDefault();

                XElement repeatInnerEl = null;
                if ((repeatInnerEl = repeatEl.Element("daily")) != null)
                {
                    XAttribute attr = repeatInnerEl.Attribute("dayFrequency");
                    dtext += (attr != null) ? attr.Value + " day(s)" : "weekday";
                }
                else if ((repeatInnerEl = repeatEl.Element("weekly")) != null)
                {
                    XAttribute attrFrequency = repeatInnerEl.Attribute("weekFrequency");
                    var attrDays = repeatInnerEl.Attributes().Where(x => x.Name.ToString().Length == 2);
                    dtext += string.Format("{0} week(s) on: {1}", attrFrequency.Value, string.Join(", ", attrDays.Select(x => DayOfWeekByShortName(x.Name.ToString())).ToArray()));
                }
                else if ((repeatInnerEl = repeatEl.Element("monthly")) != null)
                {
                    dtext += string.Format("{0} month(s) on day {1}", repeatInnerEl.Attribute("monthFrequency").Value, repeatInnerEl.Attribute("day").Value);
                }
                else if ((repeatInnerEl = repeatEl.Element("monthlyByDay")) != null)
                {
                    var attrDay = repeatInnerEl.Attributes().Where(x => x.Name.ToString().Length == 2).FirstOrDefault();
                    dtext += string.Format("{0} month(s) on the {1} {2}",
                        repeatInnerEl.Attribute("monthFrequency").Value, repeatInnerEl.Attribute("weekdayOfMonth").Value, DayOfWeekByShortName(attrDay.Name.ToString()));
                }
                else if ((repeatInnerEl = repeatEl.Element("yearly")) != null)
                {
                    dtext += string.Format("year on {0} {1}",
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(repeatInnerEl.Attribute("month").Value)),
                        repeatInnerEl.Attribute("day").Value);
                }
                else if ((repeatInnerEl = repeatEl.Element("yearlyByDay")) != null)
                {
                    var attrDay = repeatInnerEl.Attributes().Where(x => x.Name.ToString().Length == 2).FirstOrDefault();
                    dtext += string.Format("{0} on the {1} {2}",
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(repeatInnerEl.Attribute("month").Value)),
                        repeatInnerEl.Attribute("weekdayOfMonth").Value,
                        DayOfWeekByShortName(attrDay.Name.ToString()));
                }
            }
            catch
            {
                dtext = "Incorrect!";
            }

            return dtext;
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            return (string)this.GetFieldValue(dirtyValue) ?? string.Empty;
        }

        private static string DayOfWeekByShortName(string dayOfWeekShortName)
        {
            string dayOfWeek = string.Empty;
            switch (dayOfWeekShortName)
            {
                case "su":
                    dayOfWeek = "Sunday";
                    break;
                case "mo":
                    dayOfWeek = "Monday";
                    break;
                case "tu":
                    dayOfWeek = "Tuesday";
                    break;
                case "we":
                    dayOfWeek = "Wednesday";
                    break;
                case "th":
                    dayOfWeek = "Thursday";
                    break;
                case "fr":
                    dayOfWeek = "Friday";
                    break;
                case "sa":
                    dayOfWeek = "Saturday";
                    break;
                default:
                    break;
            }

            return dayOfWeek;
        }
    }
}
