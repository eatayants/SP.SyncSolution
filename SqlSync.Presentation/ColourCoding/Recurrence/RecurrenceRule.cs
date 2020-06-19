using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    internal class RecurrenceRule
    {
        #region Fields

        private int? m_day = null;
        private IList<DayOfWeek> m_daysOfWeek;
        private DayOfWeek m_defaultFirstDayOfWeek;
        private int m_defaultRepeatInstances;
        private OrdinalType m_defaultWeekOrdinal;
        private XmlDocument m_doc = new XmlDocument();
        private DayOfWeek? m_firstDayOfWeek = null;
        private XmlNode m_firstDayOfWeekNode;
        private string m_firstDayOfWeekNodeXPath = "/recurrence/rule/firstDayOfWeek";
        private int? m_frequency = null;
        private bool? m_isDay = null;
        private bool? m_isWeekday = null;
        private bool? m_isWeekendDay = null;
        private int? m_month = null;
        private OrdinalType? m_ordinal = null;
        private int? m_repeatInstances = null;
        private XmlNode m_repeatInstancesNode;
        private string m_repeatInstancesNodeXPath = "/recurrence/rule/repeatInstances";
        private XmlNode m_repeatNode;
        private string m_repeatNodeXPath = "/recurrence/rule/repeat";
        private RecurrenceType m_type;
        private XmlNode m_windowEndNode;
        private string m_windowEndNodeXPath = "/recurrence/rule/windowEnd";

        #endregion

        // Methods
        public RecurrenceRule(string rule)
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(rule)))
            {
                reader.DtdProcessing = DtdProcessing.Prohibit;
                this.m_doc.Load(reader);
            }
        }

        // Properties
        public int Day
        {
            get
            {
                if ((!this.m_day.HasValue && (this.Type == RecurrenceType.Monthly)) || (this.Type == RecurrenceType.Yearly))
                {
                    string str = this.RepeatNode.FirstChild.Attributes.GetNamedItem("day").Value;
                    this.m_day = new int?(Convert.ToInt32(str, CultureInfo.InvariantCulture));
                }
                return this.m_day.Value;
            }
        }

        public IList<DayOfWeek> DaysOfWeek
        {
            get
            {
                if (this.m_daysOfWeek == null)
                {
                    List<DayOfWeek> list = new List<DayOfWeek>();
                    XmlAttributeCollection attributes = this.RepeatNode.FirstChild.Attributes;
                    if (attributes.GetNamedItem("su") != null)
                    {
                        list.Add(DayOfWeek.Sunday);
                    }
                    if (attributes.GetNamedItem("mo") != null)
                    {
                        list.Add(DayOfWeek.Monday);
                    }
                    if (attributes.GetNamedItem("tu") != null)
                    {
                        list.Add(DayOfWeek.Tuesday);
                    }
                    if (attributes.GetNamedItem("we") != null)
                    {
                        list.Add(DayOfWeek.Wednesday);
                    }
                    if (attributes.GetNamedItem("th") != null)
                    {
                        list.Add(DayOfWeek.Thursday);
                    }
                    if (attributes.GetNamedItem("fr") != null)
                    {
                        list.Add(DayOfWeek.Friday);
                    }
                    if (attributes.GetNamedItem("sa") != null)
                    {
                        list.Add(DayOfWeek.Saturday);
                    }
                    this.m_daysOfWeek = list;
                }
                return this.m_daysOfWeek;
            }
        }

        public DayOfWeek FirstDayOfWeek
        {
            get
            {
                if (!this.m_firstDayOfWeek.HasValue)
                {
                    this.m_firstDayOfWeek = new DayOfWeek?(this.m_defaultFirstDayOfWeek);
                    if (this.FirstDayOfWeekNode != null)
                    {
                        switch (this.FirstDayOfWeekNode.InnerText)
                        {
                            case "su":
                                this.m_firstDayOfWeek = DayOfWeek.Sunday;
                                break;

                            case "mo":
                                this.m_firstDayOfWeek = DayOfWeek.Monday;
                                break;

                            case "tu":
                                this.m_firstDayOfWeek = DayOfWeek.Tuesday;
                                break;

                            case "we":
                                this.m_firstDayOfWeek = DayOfWeek.Wednesday;
                                break;

                            case "th":
                                this.m_firstDayOfWeek = DayOfWeek.Thursday;
                                break;

                            case "fr":
                                this.m_firstDayOfWeek = DayOfWeek.Friday;
                                break;

                            case "sa":
                                this.m_firstDayOfWeek = DayOfWeek.Saturday;
                                break;
                            default:
                                break;
                        }
                    }
                }

                return this.m_firstDayOfWeek.Value;
            }
        }

        private XmlNode FirstDayOfWeekNode
        {
            get
            {
                if (this.m_firstDayOfWeekNode == null)
                {
                    this.m_firstDayOfWeekNode = this.m_doc.DocumentElement.SelectSingleNode(this.m_firstDayOfWeekNodeXPath);
                }
                return this.m_firstDayOfWeekNode;
            }
        }

        public int Frequency
        {
            get
            {
                if (!this.m_frequency.HasValue)
                {
                    if (this.IsWeekday && (this.Type == RecurrenceType.Daily))
                    {
                        this.m_frequency = 1;
                    }
                    else
                    {
                        string name = string.Empty;
                        switch (this.Type)
                        {
                            case RecurrenceType.Daily:
                                name = "dayFrequency";
                                break;

                            case RecurrenceType.Weekly:
                                name = "weekFrequency";
                                break;

                            case RecurrenceType.Monthly:
                            case RecurrenceType.MonthlyByDay:
                                name = "monthFrequency";
                                break;

                            case RecurrenceType.Yearly:
                            case RecurrenceType.YearlyByDay:
                                name = "yearFrequency";
                                break;

                            default:
                                name = string.Empty;
                                break;
                        }
                        string str2 = this.RepeatNode.FirstChild.Attributes.GetNamedItem(name).Value;
                        this.m_frequency = new int?(Convert.ToInt32(str2, CultureInfo.InvariantCulture));
                    }
                }
                return this.m_frequency.Value;
            }
        }

        public bool HasWindowEnd
        {
            get
            {
                return (this.WindowEndNode != null);
            }
        }

        public bool IsDay
        {
            get
            {
                if (!this.m_isDay.HasValue) {
                    XmlNode namedItem = this.RepeatNode.FirstChild.Attributes.GetNamedItem("day");
                    this.m_isDay = ((namedItem != null) && (namedItem.Value.ToUpperInvariant() == "TRUE"));
                }
                return this.m_isDay.Value;
            }
        }

        public bool IsWeekday
        {
            get
            {
                if (!this.m_isWeekday.HasValue) {
                    XmlNode namedItem = this.RepeatNode.FirstChild.Attributes.GetNamedItem("weekday");
                    this.m_isWeekday = ((namedItem != null) && (namedItem.Value.ToUpperInvariant() == "TRUE"));
                }
                return this.m_isWeekday.Value;
            }
        }

        public bool IsWeekendDay
        {
            get
            {
                if (!this.m_isWeekendDay.HasValue) {
                    XmlNode namedItem = this.RepeatNode.FirstChild.Attributes.GetNamedItem("weekend_day");
                    this.m_isWeekendDay = ((namedItem != null) && (namedItem.Value.ToUpperInvariant() == "TRUE"));
                }
                return this.m_isWeekendDay.Value;
            }
        }

        public int Month
        {
            get
            {
                if ((!this.m_month.HasValue && (this.Type == RecurrenceType.Yearly)) || (this.Type == RecurrenceType.YearlyByDay))
                {
                    string str = this.RepeatNode.FirstChild.Attributes.GetNamedItem("month").Value;
                    this.m_month = new int?(Convert.ToInt32(str, CultureInfo.InvariantCulture));
                }
                return this.m_month.Value;
            }
        }

        public OrdinalType Ordinal
        {
            get
            {
                if (!this.m_ordinal.HasValue)
                {
                    this.m_ordinal = new OrdinalType?(this.m_defaultWeekOrdinal);
                    XmlNode namedItem = this.RepeatNode.FirstChild.Attributes.GetNamedItem("weekdayOfMonth");
                    if (((namedItem != null) && (this.Type == RecurrenceType.MonthlyByDay)) || (this.Type == RecurrenceType.YearlyByDay))
                    {
                        string str = namedItem.Value;
                        if (str == null) {
                            this.m_ordinal = OrdinalType.None;
                        }
                        else
                        {
                            switch (str)
                            {
                                case "first":
                                    this.m_ordinal = OrdinalType.First;
                                    break;
                                case "second":
                                    this.m_ordinal = OrdinalType.Second;
                                    break;
                                case "third":
                                    this.m_ordinal = OrdinalType.Third;
                                    break;
                                case "fourth":
                                    this.m_ordinal = OrdinalType.Fourth;
                                    break;
                                case "last":
                                    this.m_ordinal = OrdinalType.Last;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                return this.m_ordinal.Value;
            }
        }

        public int RepeatInstances
        {
            get
            {
                if (!this.m_repeatInstances.HasValue)
                {
                    this.m_repeatInstances = new int?(this.m_defaultRepeatInstances);
                    if (this.RepeatInstancesNode != null) {
                        this.m_repeatInstances = new int?(Convert.ToInt32(this.RepeatInstancesNode.InnerText, CultureInfo.InvariantCulture));
                    }
                }
                return this.m_repeatInstances.Value;
            }
        }

        private XmlNode RepeatInstancesNode
        {
            get
            {
                if (this.m_repeatInstancesNode == null) {
                    this.m_repeatInstancesNode = this.m_doc.DocumentElement.SelectSingleNode(this.m_repeatInstancesNodeXPath);
                }
                return this.m_repeatInstancesNode;
            }
        }

        private XmlNode RepeatNode
        {
            get
            {
                if (this.m_repeatNode == null) {
                    this.m_repeatNode = this.m_doc.DocumentElement.SelectSingleNode(this.m_repeatNodeXPath);
                }
                return this.m_repeatNode;
            }
        }

        public RecurrenceType Type
        {
            get
            {
                if (this.m_type == RecurrenceType.None)
                {
                    this.m_type = RecurrenceType.None;
                    switch (this.RepeatNode.FirstChild.Name)
                    {
                        case "daily":
                            this.m_type = RecurrenceType.Daily;
                            break;

                        case "weekly":
                            this.m_type = RecurrenceType.Weekly;
                            break;

                        case "monthly":
                            this.m_type = RecurrenceType.Monthly;
                            break;

                        case "monthlyByDay":
                            this.m_type = RecurrenceType.MonthlyByDay;
                            break;

                        case "yearly":
                            this.m_type = RecurrenceType.Yearly;
                            break;

                        case "yearlyByDay":
                            this.m_type = RecurrenceType.YearlyByDay;
                            break;
                        default:
                            break;
                    }
                }

                return this.m_type;
            }
        }

        public DateTime WindowEnd
        {
            get
            {
                XmlNode windowEndNode = this.WindowEndNode;
                if (windowEndNode == null) {
                    throw new ArgumentException();
                }
                if (string.IsNullOrEmpty(windowEndNode.FirstChild.Value))
                {
                    throw new ArgumentNullException();
                }
                var value = windowEndNode.FirstChild.Value;
                return new DateTime(Convert.ToInt32(value.Substring(0, 4)), Convert.ToInt32(value.Substring(5, 2)), Convert.ToInt32(value.Substring(8, 2)),
                    Convert.ToInt32(value.Substring(11, 2)), Convert.ToInt32(value.Substring(14, 2)), 
                    Convert.ToInt32(value.Substring(17, 2)), (Calendar)new GregorianCalendar());
            }
        }

        private XmlNode WindowEndNode
        {
            get
            {
                if (this.m_windowEndNode == null) {
                    this.m_windowEndNode = this.m_doc.DocumentElement.SelectSingleNode(this.m_windowEndNodeXPath);
                }
                return this.m_windowEndNode;
            }
        }

        // Nested Types
        internal enum OrdinalType
        {
            None,
            First,
            Second,
            Third,
            Fourth,
            Last
        }

        internal enum RecurrenceType
        {
            None,
            Daily,
            Weekly,
            Monthly,
            MonthlyByDay,
            Yearly,
            YearlyByDay
        }
    }
}
