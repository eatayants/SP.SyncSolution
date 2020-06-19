using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    internal class RecurrenceTimeZoneConverter
    {
        // Fields
        private Dictionary<string, DateTime> m_cacheDaylight = new Dictionary<string, DateTime>();
        private SPTimeZone m_localTZ;
        private SPTimeZone m_orgTZ;
        private bool m_problematic;
        private TimeZoneInfo.AdjustmentRule m_ruleForStart;
        private TimeZoneInfo.AdjustmentRule[] m_rules;

        // Methods
        internal RecurrenceTimeZoneConverter(SPTimeZone originalTimeZone, SPTimeZone localTimeZone, DateTime rangeBegin, DateTime rangeEnd)
        {
            this.m_localTZ = localTimeZone;
            this.m_orgTZ = originalTimeZone;
            this.CheckIfProblematicTimeZone(rangeBegin, rangeEnd);
        }

        private TimeSpan CalculateGap(DateTime date)
        {
            if (this.m_ruleForStart.DateEnd < date.Date)
            {
                int index = this.FindRuleIndex(this.m_rules, date);
                if (index == -1)
                {
                    return TimeSpan.Zero;
                }
                DateTime time = this.TransitionTimeToDate(date.Year, this.m_ruleForStart.DaylightTransitionStart, true, 1);
                DateTime time2 = this.TransitionTimeToDate(date.Year, this.m_ruleForStart.DaylightTransitionEnd, false, 1);
                DateTime time3 = this.TransitionTimeToDate(date.Year, this.m_rules[index].DaylightTransitionStart, true, 2);
                DateTime time4 = this.TransitionTimeToDate(date.Year, this.m_rules[index].DaylightTransitionEnd, false, 2);
                bool flag = (time <= date) && (date <= time2);
                bool flag2 = (time3 <= date) && (date <= time4);
                if (!flag && flag2)
                {
                    return this.m_rules[index].DaylightDelta;
                }
                if (flag && !flag2)
                {
                    return -this.m_ruleForStart.DaylightDelta;
                }
            }
            return TimeSpan.Zero;
        }

        private void CheckIfProblematicTimeZone(DateTime start, DateTime end)
        {
            try
            {
                TimeZoneInfo timeZoneInfoById = SPRegionalSettings.GlobalTimeZones.GetZoneInfoById(this.m_orgTZ.ID);
                if (timeZoneInfoById.SupportsDaylightSavingTime)
                {
                    TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZoneInfoById.GetAdjustmentRules();
                    if (adjustmentRules.Length != 1)
                    {
                        int index = this.FindRuleIndex(adjustmentRules, start);
                        int num2 = this.FindRuleIndex(adjustmentRules, end);
                        if (index != num2)
                        {
                            this.m_problematic = true;
                            this.m_ruleForStart = adjustmentRules[index];
                            List<TimeZoneInfo.AdjustmentRule> list = new List<TimeZoneInfo.AdjustmentRule>();
                            for (int i = 0; i < adjustmentRules.Length; i++)
                            {
                                if (i != index)
                                {
                                    list.Add(adjustmentRules[i]);
                                }
                            }
                            this.m_rules = list.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private int FindRuleIndex(TimeZoneInfo.AdjustmentRule[] rules, DateTime date)
        {
            for (int i = 0; i < rules.Length; i++)
            {
                DateTime time = date.Date;
                if ((rules[i].DateStart <= time) && (rules[i].DateEnd >= time))
                {
                    return i;
                }
            }
            return -1;
        }

        internal DateTime ToLocal(DateTime date)
        {
            if (this.m_problematic)
            {
                date += this.CalculateGap(date);
            }
            if (this.m_localTZ.ID != this.m_orgTZ.ID)
            {
                date = this.m_orgTZ.LocalTimeToUTC(date);
                date = this.m_localTZ.UTCToLocalTime(date);
            }
            return date;
        }

        internal DateTime ToOriginal(DateTime date)
        {
            if (this.m_localTZ.ID != this.m_orgTZ.ID)
            {
                date = this.m_localTZ.LocalTimeToUTC(date);
                date = this.m_orgTZ.UTCToLocalTime(date);
            }
            if (this.m_problematic)
            {
                date -= this.CalculateGap(date);
            }
            return date;
        }

        private DateTime TransitionTimeToDate(int year, TimeZoneInfo.TransitionTime transition, bool fStartDate, int index)
        {
            DateTime time;
            string key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", new object[] { fStartDate ? "S" : "E", index, year });
            if (this.m_cacheDaylight.ContainsKey(key))
            {
                return this.m_cacheDaylight[key];
            }
            if (transition.IsFixedDateRule)
            {
                time = new DateTime(year, transition.Month, transition.Day);
            }
            else
            {
                time = new DateTime(year, transition.Month, 1);
                while (time.DayOfWeek != transition.DayOfWeek)
                {
                    time = time.AddDays(1.0);
                }
                for (int i = 1; i != transition.Week; i++)
                {
                    time = time.AddDays(7.0);
                }
                while (time.Month != transition.Month)
                {
                    time = time.AddDays(-7.0);
                }
                time = time.AddHours((double)transition.TimeOfDay.Hour).AddMinutes((double)transition.TimeOfDay.Minute);
            }
            this.m_cacheDaylight[key] = time;
            return time;
        }
    }
}
