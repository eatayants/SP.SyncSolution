using Roster.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Roster.Common;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    public static class RecurrenceItemExpander
    {
        // Fields
        private const int MaxExpand = 999;

        // Methods
        internal static IEnumerable<ExpandedRosterEvent> ShiftByHolidays(IEnumerable<ExpandedRosterEvent> expandedEvents, List<DateTime> holidays)
        {
            if (holidays.IsEmpty())
            {
                return expandedEvents;
            }
            var result = new List<ExpandedRosterEvent>();
            var expandedRosterEvents = expandedEvents.ToList();
            foreach (var expandedEvent in expandedRosterEvents.OrderBy(item=>item.StartDate))
            {
                var occupiedDates = result.Select(item => item.StartDate).ToList();
                var startDate = new XDateTime(expandedEvent.StartDate, holidays);
                if (startDate.IsHoliday)
                {
                    var next = startDate.NextBusinessDay(occupiedDates);
                    expandedEvent.StartDate = new DateTime(next.Year, next.Month, next.Day, expandedEvent.StartDate.Hour, expandedEvent.StartDate.Minute, expandedEvent.StartDate.Second);
                    expandedEvent.EndDate = new DateTime(next.Year, next.Month, next.Day, expandedEvent.EndDate.Hour, expandedEvent.EndDate.Minute, expandedEvent.EndDate.Second);
                }
                else
                {
                    result.Add(expandedEvent);
                }
            }
            return result.DistinctBy(item => item.StartDate).ToList();
        }

        internal static IEnumerable<ExpandedRosterEvent> Expand(IEnumerable<RosterEvent> items, IEnumerable<int> excludeExceptionIds, DateTime rangeBegin, DateTime rangeEnd, string beginFieldName, string endFieldName, SPTimeZone timeZone)
        {
            List<ExpandedRosterEvent> list = new List<ExpandedRosterEvent>();
            Dictionary<Guid, RosterEvent> dictionary = new Dictionary<Guid, RosterEvent>();
            Dictionary<Guid, IList<RosterEvent>> dictionary2 = new Dictionary<Guid, IList<RosterEvent>>();
            foreach (RosterEvent item in items)
            {
                if (item.GetIsRecurrence() && item.GetShPEventType() == 1)
                {
                    if (!dictionary2.ContainsKey(item.Id)) {
                        dictionary2.Add(item.Id, new List<RosterEvent>());
                        dictionary.Add(item.Id, item);
                    }
                }
                else
                {
                    // non-recurrence event
                    var ere = new ExpandedRosterEvent(item, true, timeZone);
                    list.Add(ere);
                }
            }

            foreach (Guid itmId in dictionary.Keys)
            {
                IList<ExpandedRosterEvent> expanded = ExpandSeriesItem(dictionary[itmId], beginFieldName, endFieldName, rangeBegin, rangeEnd, timeZone);
                //ApplyRelatedItems(dictionary[itmId], expanded, dictionary2[itmId], excludeExceptionIds, beginFieldName, endFieldName, timeZone);
                list.AddRange(expanded);
            }
            return list;
        }

        internal static int GetEventType(RosterEvent item)
        {
            return item.GetShPEventType();
        }

        internal static Guid GetMasterId(RosterEvent item)
        {
            int eventType = GetEventType(item);
            string stringFieldValue = item.Id.ToString();
            if ((eventType == 1) && !stringFieldValue.Contains<char>('.')) {
                return item.Id;
            }

            if ((eventType != 4) && (eventType != 3)) {
                return Guid.Empty;
            }

            return Guid.Empty; //SafeFieldAccessor.GetIntegerFieldValue(item, "MasterSeriesItemID");
        }


        #region Private methods

        private static IList<ExpandedRosterEvent> ExpandSeriesItem(RosterEvent masterItem, string beginFieldName, string endFieldName, DateTime localTimeRangeBegin, DateTime localTimeRangeEnd, SPTimeZone localTZ)
        {
            DateTime time;
            DateTime time2;
            RecurrenceRule rule = new RecurrenceRule(masterItem.GetRecurrence());
            bool isAllDayEvent = masterItem.GetIsAllDayEvent();
            SPTimeZone timeZone = GetTimeZone(masterItem, localTZ);
            DateTime dateTimeFieldValue = masterItem.GetStartDate();
            DateTime rangeEnd = masterItem.GetEndDate();

            RecurrenceTimeZoneConverter converter = new RecurrenceTimeZoneConverter(timeZone, localTZ, dateTimeFieldValue, rangeEnd);
            if (isAllDayEvent) {
                time = dateTimeFieldValue;
                time2 = rangeEnd;
            } else {
                time = converter.ToOriginal(dateTimeFieldValue);
                time2 = converter.ToOriginal(rangeEnd);
            }
            TimeSpan itemLength = CalculateItemLength(time, time2);
            DateTime rangeBegin = converter.ToOriginal(localTimeRangeBegin);
            DateTime time6 = converter.ToOriginal(localTimeRangeEnd);
            if (isAllDayEvent) {
                rangeBegin = localTimeRangeBegin;
                time6 = localTimeRangeEnd;
            }
            DateTime time7 = new DateTime(dateTimeFieldValue.Ticks) + itemLength;
            if (time7.Day != dateTimeFieldValue.Day) {
                rangeBegin = rangeBegin.AddDays(-1.0);
            }
            rangeBegin = ComputeExpandBegin(rangeBegin, time, rule);
            time6 = ComputeExpandEnd(time6, time, time2, rule, timeZone);

            List<ExpandedRosterEvent> list = new List<ExpandedRosterEvent>();
            DateTime date = rangeBegin.Date;
            while (true)
            {
                DateTime itemBegin = ComputeTargetBegin(date, time, rule);
                DateTime time10 = ComputeTargetEnd(itemBegin, itemLength);
                TimeSpan span2 = (TimeSpan)(time10 - itemBegin);
                bool flag2 = span2.Ticks == 0L;
                if ((time6 < itemBegin) || ((time6 == itemBegin) && !flag2)) {
                    return list;
                }
                if (999 <= list.Count) {
                    return list;
                }
                if (((rule.Type == RecurrenceRule.RecurrenceType.Daily) && rule.IsWeekday) && ((itemBegin.DayOfWeek == DayOfWeek.Saturday) || (itemBegin.DayOfWeek == DayOfWeek.Sunday)))
                {
                    date = IncrementDate(date, rule);
                }
                else if ((rule.Type == RecurrenceRule.RecurrenceType.Weekly) && !rule.DaysOfWeek.Contains(itemBegin.DayOfWeek))
                {
                    date = IncrementDate(date, rule);
                }
                else
                {
                    ExpandedRosterEvent item = null;
                    if ((rangeBegin < time10) || ((rangeBegin == time10) && (itemLength.Ticks == 0L)))
                    {
                        item = new ExpandedRosterEvent(masterItem, false, null);
                        if (isAllDayEvent) {
                            item.StartDate = itemBegin;
                            item.EndDate = time10;
                        } else {
                            item.StartDate = converter.ToLocal(itemBegin);
                            item.EndDate = converter.ToLocal(time10);
                        }
                        //item["ID"] = str2;
                        item.InstanceID = GenerateRecurrenceItemId(masterItem.Id, item.StartDate, localTZ, isAllDayEvent);
                        if (999 > list.Count) {
                            list.Add(item);
                        }
                    }
                    date = IncrementDate(date, rule);
                }
            }
        }

        private static TimeSpan CalculateItemLength(DateTime itemBegin, DateTime itemEnd)
        {
            DateTime time = new DateTime(itemBegin.Ticks);
            DateTime time2 = time.Date.AddHours((double)itemEnd.Hour).AddMinutes((double)itemEnd.Minute);
            int num = (itemBegin.Hour * 60) + itemBegin.Minute;
            int num2 = (itemEnd.Hour * 60) + itemEnd.Minute;
            if (num > num2)
            {
                time2 = time2.AddDays(1.0);
            }
            return time2 - time;
        }

        private static DateTime ComputeExpandBegin(DateTime rangeBegin, DateTime orgTimeItemBegin, RecurrenceRule rule)
        {
            DateTime itemBegin = new DateTime(orgTimeItemBegin.Ticks);
            if (rangeBegin <= itemBegin)
            {
                return new DateTime(itemBegin.Ticks);
            }
            DateTime time2 = new DateTime();
            switch (rule.Type)
            {
                case RecurrenceRule.RecurrenceType.Daily:
                    return ComputeExpandBeginDaily(rangeBegin, itemBegin, rule);

                case RecurrenceRule.RecurrenceType.Weekly:
                    return ComputeExpandBeginWeekly(rangeBegin, itemBegin, rule);

                case RecurrenceRule.RecurrenceType.Monthly:
                case RecurrenceRule.RecurrenceType.MonthlyByDay:
                    return ComputeExtractBeginMonthly(rangeBegin, itemBegin, rule);

                case RecurrenceRule.RecurrenceType.Yearly:
                case RecurrenceRule.RecurrenceType.YearlyByDay:
                    return ComputeExtractBeginYearly(rangeBegin, itemBegin, rule);
            }
            return time2;
        }

        private static DateTime ComputeExpandBeginDaily(DateTime rangeBegin, DateTime itemBegin, RecurrenceRule rule)
        {
            int num = ComupteSpanDays(rangeBegin, itemBegin);
            if ((num % rule.Frequency) == 0)
            {
                return new DateTime(rangeBegin.Ticks);
            }
            int num2 = rule.Frequency - (num % rule.Frequency);
            return rangeBegin.AddDays((double)num2);
        }

        private static DateTime ComputeExpandBeginWeekly(DateTime rangeBegin, DateTime itemBegin, RecurrenceRule rule)
        {
            while (!rule.DaysOfWeek.Contains(itemBegin.DayOfWeek))
            {
                itemBegin = itemBegin.AddDays(1.0);
            }
            if (rangeBegin <= itemBegin)
            {
                return new DateTime(itemBegin.Ticks);
            }
            int length = Enum.GetNames(typeof(DayOfWeek)).Length;
            DateTime time = new DateTime(itemBegin.Ticks);
            while ((time < rangeBegin) || !rule.DaysOfWeek.Contains(time.DayOfWeek))
            {
                time = time.AddDays(1.0);
                if ((time.DayOfWeek == rule.FirstDayOfWeek) && (rule.Frequency > 1))
                {
                    time = time.AddDays((double)(length * (rule.Frequency - 1)));
                }
            }
            return time;
        }

        private static DateTime ComputeExpandEnd(DateTime rangeEnd, DateTime orgTimeItemBegin, DateTime orgTimeItemEnd, RecurrenceRule rule, SPTimeZone orgTZ)
        {
            DateTime time;
            DateTime time2 = new DateTime(orgTimeItemBegin.Ticks);
            DateTime time3 = new DateTime(orgTimeItemEnd.Ticks);
            if (rule.HasWindowEnd)
            {
                time3 = orgTZ.UTCToLocalTime(rule.WindowEnd).Date.AddDays(1.0);
            }
            if (time3 < rangeEnd)
            {
                time = new DateTime(time3.Ticks);
            }
            else
            {
                time = new DateTime(rangeEnd.Ticks);
            }
            int repeatInstances = rule.RepeatInstances;
            if (((repeatInstances == 0) && !rule.HasWindowEnd) || (repeatInstances > 0x3e7))
            {
                repeatInstances = 0x3e7;
            }
            if (repeatInstances > 0)
            {
                int num2 = 0;
                DateTime date = time2.Date;
                while (date < rangeEnd)
                {
                    DateTime time5 = ComputeTargetBegin(date, orgTimeItemBegin, rule);
                    if (((rule.Type == RecurrenceRule.RecurrenceType.Daily) && rule.IsWeekday) && ((time5.DayOfWeek == DayOfWeek.Saturday) || (time5.DayOfWeek == DayOfWeek.Sunday)))
                    {
                        date = IncrementDate(date, rule);
                    }
                    else if ((rule.Type == RecurrenceRule.RecurrenceType.Weekly) && !rule.DaysOfWeek.Contains(time5.DayOfWeek))
                    {
                        date = IncrementDate(date, rule);
                    }
                    else
                    {
                        if (time5 >= time2)
                        {
                            num2++;
                            if (num2 == repeatInstances)
                            {
                                return time5.AddDays(1.0).Date;
                            }
                        }
                        date = IncrementDate(date, rule);
                    }
                }
            }
            return time;
        }

        private static DateTime ComputeExtractBeginMonthly(DateTime rangeBegin, DateTime itemBegin, RecurrenceRule rule)
        {
            DateTime time;
            int num = ComputeSpanMonths(itemBegin, rangeBegin);
            if ((num % rule.Frequency) == 0)
            {
                time = new DateTime(rangeBegin.Ticks);
            }
            else
            {
                int months = rule.Frequency - (num % rule.Frequency);
                time = rangeBegin.AddMonths(months);
            }

            time = ComputeTargetDateOfMonth(time, rule).Date.AddHours((double)itemBegin.Hour).AddMinutes((double)itemBegin.Minute);
            if (time < rangeBegin)
            {
                time = ComputeTargetDateOfMonth(time.AddMonths(rule.Frequency), rule);
            }
            return time;
        }

        private static DateTime ComputeExtractBeginYearly(DateTime rangeBegin, DateTime itemBegin, RecurrenceRule rule)
        {
            DateTime time;
            int num = Math.Abs((int)(itemBegin.Year - rangeBegin.Year));
            if ((num % rule.Frequency) == 0)
            {
                time = new DateTime(rangeBegin.Ticks);
            }
            else
            {
                time = rangeBegin.AddYears(rule.Frequency - (num % rule.Frequency));
            }
            if (rule.Type == RecurrenceRule.RecurrenceType.Yearly)
            {
                time = GetValidDate(time.Year, rule.Month, rule.Day).Date.AddHours((double)itemBegin.Hour).AddMinutes((double)itemBegin.Minute);
                if (time < rangeBegin)
                {
                    time = time.AddYears(rule.Frequency);
                }
                return time;
            }
            if (rule.Type == RecurrenceRule.RecurrenceType.YearlyByDay)
            {
                time = ComputeTargetDateOfMonth(GetValidDate(time.Year, rule.Month, 1), rule).Date.AddHours((double)itemBegin.Hour).AddMinutes((double)itemBegin.Minute);
                if (time < rangeBegin)
                {
                    time = ComputeTargetDateOfMonth(GetValidDate(time.Year + 1, rule.Month, 1), rule);
                }
            }
            return time;
        }

        private static int ComputeSpanMonths(DateTime dt1, DateTime dt2)
        {
            if (dt1 > dt2)
            {
                DateTime time = dt1;
                dt1 = dt2;
                dt2 = time;
            }
            int num = 12;
            int num2 = dt2.Year - dt1.Year;
            int num3 = dt2.Month - dt1.Month;
            return (num3 + (num2 * num));
        }

        private static DateTime ComputeTargetBegin(DateTime date, DateTime orgTimeBegin, RecurrenceRule rule)
        {
            DateTime time = new DateTime();
            if ((rule.Type == RecurrenceRule.RecurrenceType.Daily) || (rule.Type == RecurrenceRule.RecurrenceType.Weekly))
            {
                return date.AddHours((double)orgTimeBegin.Hour).AddMinutes((double)orgTimeBegin.Minute);
            }
            if ((rule.Type == RecurrenceRule.RecurrenceType.Monthly) || (rule.Type == RecurrenceRule.RecurrenceType.MonthlyByDay))
            {
                return ComputeTargetDateOfMonth(date, rule).AddHours((double)orgTimeBegin.Hour).AddMinutes((double)orgTimeBegin.Minute);
            }
            if ((rule.Type != RecurrenceRule.RecurrenceType.Yearly) && (rule.Type != RecurrenceRule.RecurrenceType.YearlyByDay))
            {
                return time;
            }
            return ComputeTargetDateOfMonth(GetValidDate(date.Year, rule.Month, 1), rule).AddHours((double)orgTimeBegin.Hour).AddMinutes((double)orgTimeBegin.Minute);
        }

        private static DateTime ComputeTargetDateOfMonth(DateTime dt, RecurrenceRule rule)
        {
            DateTime firstDayOfMonth = new DateTime(dt.Year, dt.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1.0);
            if (rule.Ordinal == RecurrenceRule.OrdinalType.None)
            {
                int day = rule.Day;
                int num2 = lastDayOfMonth.Day;
                if (num2 < day)
                {
                    day = num2;
                }
                return firstDayOfMonth.AddDays((double)(day - 1));
            }
            int position = -1;
            switch (rule.Ordinal)
            {
                case RecurrenceRule.OrdinalType.First:
                    position = 0;
                    break;

                case RecurrenceRule.OrdinalType.Second:
                    position = 1;
                    break;

                case RecurrenceRule.OrdinalType.Third:
                    position = 2;
                    break;

                case RecurrenceRule.OrdinalType.Fourth:
                    position = 3;
                    break;
            }
            DateTime time3 = firstDayOfMonth;
            if (rule.IsDay)
            {
                return ComputeTargetDateOfMonthDay(firstDayOfMonth, lastDayOfMonth, position);
            }
            if (rule.IsWeekday || rule.IsWeekendDay)
            {
                return ComputeTargetDateOfMonthWeekday(firstDayOfMonth, lastDayOfMonth, position, rule);
            }
            if ((rule.DaysOfWeek != null) && (rule.DaysOfWeek.Count > 0))
            {
                time3 = ComputeTargetDateOfMonthDayOfWeek(firstDayOfMonth, position, rule);
            }
            return time3;
        }

        private static DateTime ComputeTargetDateOfMonthDay(DateTime firstDayOfMonth, DateTime lastDayOfMonth, int position)
        {
            if (position < 0)
            {
                return lastDayOfMonth;
            }
            return firstDayOfMonth.AddDays((double)position);
        }

        private static DateTime ComputeTargetDateOfMonthDayOfWeek(DateTime firstDayOfMonth, int position, RecurrenceRule rule)
        {
            DateTime time;
            int length = Enum.GetNames(typeof(DayOfWeek)).Length;
            DayOfWeek week = rule.DaysOfWeek[0];
            if (position < 0)
            {
                time = firstDayOfMonth.AddMonths(1);
                int num2 = Math.Abs((int)(time.DayOfWeek - week));
                if (time.DayOfWeek <= week)
                {
                    num2 -= length;
                }
                else
                {
                    num2 = -num2;
                }
                return time.AddDays((double)num2);
            }
            time = firstDayOfMonth;
            int num3 = 0;
            if (week < time.DayOfWeek)
            {
                num3 = ((int)(week - time.DayOfWeek)) + length;
            }
            else if (week > time.DayOfWeek)
            {
                num3 = (int)(week - time.DayOfWeek);
            }
            num3 += length * position;
            return time.AddDays((double)num3);
        }

        private static DateTime ComputeTargetDateOfMonthWeekday(DateTime firstDayOfMonth, DateTime lastDayOfMonth, int position, RecurrenceRule rule)
        {
            DateTime time;
            if (position < 0)
            {
                time = lastDayOfMonth;
                while ((!rule.IsWeekday || (time.DayOfWeek == DayOfWeek.Saturday)) || (time.DayOfWeek == DayOfWeek.Sunday))
                {
                    if (rule.IsWeekendDay && ((time.DayOfWeek == DayOfWeek.Saturday) || (time.DayOfWeek == DayOfWeek.Sunday)))
                    {
                        return time;
                    }
                    time = time.AddDays(-1.0);
                }
                return time;
            }
            time = firstDayOfMonth;
        Label_0057:
            while ((rule.IsWeekday && ((time.DayOfWeek == DayOfWeek.Saturday) || (time.DayOfWeek == DayOfWeek.Sunday))) || ((rule.IsWeekendDay && (time.DayOfWeek != DayOfWeek.Saturday)) && (time.DayOfWeek != DayOfWeek.Sunday)))
            {
                time = time.AddDays(1.0);
            }
            if (position > 0)
            {
                time = time.AddDays(1.0);
                position--;
                goto Label_0057;
            }
            return time;
        }

        private static DateTime ComputeTargetEnd(DateTime itemBegin, TimeSpan itemLength)
        {
            return itemBegin.Add(itemLength);
        }

        private static int ComupteSpanDays(DateTime dt1, DateTime dt2)
        {
            TimeSpan span = (TimeSpan)(dt1.Date - dt2.Date);
            return Math.Abs(span.Days);
        }


        private static string GenerateRecurrenceItemId(Guid masterId, DateTime time, SPTimeZone timeZone, bool isAllDay)
        {
            DateTime time2 = isAllDay ? time : timeZone.LocalTimeToUTC(time);
            return string.Format(CultureInfo.InvariantCulture, "{0}.0.{1}", new object[] { masterId, SPUtility.CreateISO8601DateTimeFromSystemDateTime(time2) });
        }

        private static SPTimeZone GetTimeZone(RosterEvent item, SPTimeZone localTZ)
        {
            return localTZ;
        }

        private static DateTime GetValidDate(int year, int month, int day)
        {
            int num = day;
            DateTime time = new DateTime(year, month, 1);
            int num2 = time.AddMonths(1).AddDays(-1.0).Day;
            if (num2 < day)
            {
                num = num2;
            }
            return new DateTime(year, month, num);
        }

        private static DateTime IncrementDate(DateTime dt, RecurrenceRule rule)
        {
            DateTime time = dt;
            if (rule.Type == RecurrenceRule.RecurrenceType.Daily)
            {
                return dt.AddDays((double)rule.Frequency);
            }
            if (rule.Type == RecurrenceRule.RecurrenceType.Weekly)
            {
                int num = Enum.GetNames(typeof(DayOfWeek)).Count<string>();
                time = dt.AddDays(1.0);
                if ((time.DayOfWeek == rule.FirstDayOfWeek) && (rule.Frequency > 1))
                {
                    time = time.AddDays((double)(num * (rule.Frequency - 1)));
                }
                return time;
            }
            if ((rule.Type == RecurrenceRule.RecurrenceType.Monthly) || (rule.Type == RecurrenceRule.RecurrenceType.MonthlyByDay))
            {
                return dt.AddMonths(rule.Frequency);
            }
            if ((rule.Type != RecurrenceRule.RecurrenceType.Yearly) && (rule.Type != RecurrenceRule.RecurrenceType.YearlyByDay))
            {
                return time;
            }
            return dt.AddYears(rule.Frequency);
        }

        #endregion
    }
}
