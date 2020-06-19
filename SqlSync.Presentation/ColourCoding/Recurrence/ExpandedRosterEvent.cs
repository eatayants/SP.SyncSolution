using Microsoft.SharePoint;
using Roster.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Extensions;
using System.Dynamic;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    public sealed class ExpandedRosterEvent : RosterEvent
    {
        // Fields
        private RosterEvent m_item;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ExpandoObject RosterEventProperties
        {
            get
            {
                return m_item != null ? m_item.RosterEventProperties : null;
            }
        }
        public RosterEvent OriginalItem
        {
            get { return m_item; }
            set { m_item = value; }
        }

        public string InstanceID { get; set; }

        public ExpandedRosterEvent()
        {
        }

        public ExpandedRosterEvent(RosterEvent item, bool withInit, SPTimeZone timeZone)
        {
            this.m_item = item;
            this.EventTypeId = this.m_item.EventTypeId;

            if (withInit)
                this.InitNonRecurrenceExpandedRoster(timeZone);
        }

        public void InitNonRecurrenceExpandedRoster(SPTimeZone timeZone)
        {
            bool isAllDayEvent = this.m_item.GetIsAllDayEvent();
            DateTime sDate = this.m_item.GetStartDate();
            DateTime eDate = this.m_item.GetEndDate();

            RecurrenceTimeZoneConverter converter = new RecurrenceTimeZoneConverter(timeZone, timeZone, sDate, eDate);
            this.StartDate = isAllDayEvent ? sDate : converter.ToLocal(sDate);
            this.EndDate = isAllDayEvent ? eDate : converter.ToLocal(eDate);
            this.InstanceID = this.m_item.Id.ToString();
        }
    }
}
