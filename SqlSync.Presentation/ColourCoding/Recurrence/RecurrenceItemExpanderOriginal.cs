using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    public static class RecurrenceItemExpanderOriginal
    {
        // Fields
        private const int MaxExpand = 999;

        // Methods
        private static void ApplyRelatedItems(SPItem masterItem, IList<SPItem> expanded, IEnumerable<SPItem> related, IEnumerable<int> excludeExceptionIds, string beginFieldName, string endFieldName, SPTimeZone localTimeZone)
        {
            foreach (SPItem item in related)
            {
                int integerFieldValue = SafeFieldAccessor.GetIntegerFieldValue(masterItem, "ID");
                DateTime dateTimeFieldValue = SafeFieldAccessor.GetDateTimeFieldValue(item, "RecurrenceID");
                string id = GenerateRecurrenceItemId(integerFieldValue, dateTimeFieldValue, localTimeZone, false);
                SPItem item2 = FindItemById(expanded, id);
                if (item2 != null)
                {
                    expanded.Remove(item2);
                }
                int eventType = GetEventType(item);
                if ((eventType == 4) && ((excludeExceptionIds == null) || !excludeExceptionIds.Contains<int>(item.ID)))
                {
                    DateTime start = SafeFieldAccessor.GetDateTimeFieldValue(item, beginFieldName);
                    DateTime end = SafeFieldAccessor.GetDateTimeFieldValue(item, endFieldName);
                    item2 = FindItemByBeginEnd(expanded, beginFieldName, endFieldName, start, end);
                    if (item2 != null)
                    {
                        if (GetEventType(item2) == 4)
                        {
                            continue;
                        }
                        expanded.Remove(item2);
                    }
                    SPItem item3 = new ExpandedCalendarItem(item);
                    string str2 = GenerateExceptionItemId(item.ID, integerFieldValue, eventType);
                    item3["ID"] = str2;
                    expanded.Add(item3);
                }
            }
        }

        private static TimeSpan CalculateItemLength(DateTime itemBegin, DateTime itemEnd)
        {
            DateTime time = new DateTime(itemBegin.Ticks);
            DateTime time2 = time.Date.AddHours((double)itemEnd.Hour).AddMinutes((double)itemEnd.Minute);
            int num = (itemBegin.Hour * 60) + itemBegin.Minute;
            int num2 = (itemEnd.Hour * 60) + itemEnd.Minute;
            if (num > num2) {
                time2 = time2.AddDays(1.0);
            }
            return (TimeSpan)(time2 - time);
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
            if ((num % rule.Frequency) == 0) {
                return new DateTime(rangeBegin.Ticks);
            }
            int num2 = rule.Frequency - (num % rule.Frequency);
            return rangeBegin.AddDays((double)num2);
        }

        private static DateTime ComputeExpandBeginWeekly(DateTime rangeBegin, DateTime itemBegin, RecurrenceRule rule)
        {
            while (!rule.DaysOfWeek.Contains(itemBegin.DayOfWeek)) {
                itemBegin = itemBegin.AddDays(1.0);
            }
            if (rangeBegin <= itemBegin) {
                return new DateTime(itemBegin.Ticks);
            }
            int length = Enum.GetNames(typeof(DayOfWeek)).Length;
            DateTime time = new DateTime(itemBegin.Ticks);
            while ((time < rangeBegin) || !rule.DaysOfWeek.Contains(time.DayOfWeek))
            {
                time = time.AddDays(1.0);
                if ((time.DayOfWeek == rule.FirstDayOfWeek) && (rule.Frequency > 1)) {
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
            if (rule.HasWindowEnd) {
                time3 = orgTZ.UTCToLocalTime(rule.WindowEnd).Date.AddDays(1.0);
            }
            if (time3 < rangeEnd) {
                time = new DateTime(time3.Ticks);
            }
            else {
                time = new DateTime(rangeEnd.Ticks);
            }
            int repeatInstances = rule.RepeatInstances;
            if (((repeatInstances == 0) && !rule.HasWindowEnd) || (repeatInstances > 0x3e7)) {
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
                            if (num2 == repeatInstances) {
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
            if ((num % rule.Frequency) == 0) {
                time = new DateTime(rangeBegin.Ticks);
            } else {
                int months = rule.Frequency - (num % rule.Frequency);
                time = rangeBegin.AddMonths(months);
            }

            time = ComputeTargetDateOfMonth(time, rule).Date.AddHours((double)itemBegin.Hour).AddMinutes((double)itemBegin.Minute);
            if (time < rangeBegin) {
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
            if (dt1 > dt2) {
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
            if ((rule.Type == RecurrenceRule.RecurrenceType.Daily) || (rule.Type == RecurrenceRule.RecurrenceType.Weekly)) {
                return date.AddHours((double)orgTimeBegin.Hour).AddMinutes((double)orgTimeBegin.Minute);
            }
            if ((rule.Type == RecurrenceRule.RecurrenceType.Monthly) || (rule.Type == RecurrenceRule.RecurrenceType.MonthlyByDay)) {
                return ComputeTargetDateOfMonth(date, rule).AddHours((double)orgTimeBegin.Hour).AddMinutes((double)orgTimeBegin.Minute);
            }
            if ((rule.Type != RecurrenceRule.RecurrenceType.Yearly) && (rule.Type != RecurrenceRule.RecurrenceType.YearlyByDay)) {
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
                if (num2 < day) {
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
            if (position < 0) {
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
                if (time.DayOfWeek <= week) {
                    num2 -= length;
                } else {
                    num2 = -num2;
                }
                return time.AddDays((double)num2);
            }
            time = firstDayOfMonth;
            int num3 = 0;
            if (week < time.DayOfWeek) {
                num3 = ((int)(week - time.DayOfWeek)) + length;
            } else if (week > time.DayOfWeek) {
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

        internal static IEnumerable<SPItem> Expand(IEnumerable<SPItem> items, IEnumerable<int> excludeExceptionIds, DateTime rangeBegin, DateTime rangeEnd, string beginFieldName, string endFieldName, SPTimeZone timeZone)
        {
            List<SPItem> list = new List<SPItem>();
            Dictionary<int, SPItem> dictionary = new Dictionary<int, SPItem>();
            Dictionary<int, IList<SPItem>> dictionary2 = new Dictionary<int, IList<SPItem>>();
            foreach (SPItem item in items)
            {
                if (SafeFieldAccessor.GetBoolFieldValue(item, "fRecurrence") && (GetEventType(item) == 1))
                {
                    int integerFieldValue = SafeFieldAccessor.GetIntegerFieldValue(item, "ID");
                    if (!dictionary2.ContainsKey(integerFieldValue))
                    {
                        dictionary2.Add(integerFieldValue, new List<SPItem>());
                        dictionary.Add(integerFieldValue, item);
                    }
                }
            }
            foreach (SPItem item2 in items)
            {
                int masterId = GetMasterId(item2);
                if (masterId != item2.ID)
                {
                    if (!dictionary.ContainsKey(masterId))
                    {
                        switch (GetEventType(item2))
                        {
                            case 3:
                                {
                                    continue;
                                }
                            case 4:
                                {
                                    SPItem item3 = item2;
                                    SPListItem item4 = item2 as SPListItem;
                                    if (item4 != null)
                                    {
                                        item3 = new ExpandedCalendarItem(item4);
                                        item3["ID"] = item4.RecurrenceID;
                                    }
                                    list.Add(item3);
                                    continue;
                                }
                        }
                        list.Add(item2);
                    }
                    else
                    {
                        dictionary2[masterId].Add(item2);
                    }
                }
            }
            foreach (int num3 in dictionary.Keys)
            {
                IList<SPItem> expanded = ExpandSeriesItem(dictionary[num3], beginFieldName, endFieldName, rangeBegin, rangeEnd, timeZone);
                ApplyRelatedItems(dictionary[num3], expanded, dictionary2[num3], excludeExceptionIds, beginFieldName, endFieldName, timeZone);
                list.AddRange(expanded);
            }
            return list;
        }

        private static IList<SPItem> ExpandSeriesItem(SPItem masterItem, string beginFieldName, string endFieldName, DateTime localTimeRangeBegin, DateTime localTimeRangeEnd, SPTimeZone localTZ)
        {
            DateTime time;
            DateTime time2;
            RecurrenceRule rule = new RecurrenceRule(SafeFieldAccessor.GetStringFieldValue(masterItem, "RecurrenceData"));
            bool boolFieldValue = SafeFieldAccessor.GetBoolFieldValue(masterItem, "fAllDayEvent");
            SPTimeZone timeZone = GetTimeZone(masterItem, localTZ);
            DateTime dateTimeFieldValue = SafeFieldAccessor.GetDateTimeFieldValue(masterItem, beginFieldName);
            DateTime rangeEnd = SafeFieldAccessor.GetDateTimeFieldValue(masterItem, endFieldName);
            RecurrenceTimeZoneConverter converter = new RecurrenceTimeZoneConverter(timeZone, localTZ, dateTimeFieldValue, rangeEnd);
            if (boolFieldValue)
            {
                time = dateTimeFieldValue;
                time2 = rangeEnd;
            }
            else
            {
                time = converter.ToOriginal(dateTimeFieldValue);
                time2 = converter.ToOriginal(rangeEnd);
            }
            TimeSpan itemLength = CalculateItemLength(time, time2);
            DateTime rangeBegin = converter.ToOriginal(localTimeRangeBegin);
            DateTime time6 = converter.ToOriginal(localTimeRangeEnd);
            if (boolFieldValue)
            {
                rangeBegin = localTimeRangeBegin;
                time6 = localTimeRangeEnd;
            }
            DateTime time7 = new DateTime(dateTimeFieldValue.Ticks) + itemLength;
            if (time7.Day != dateTimeFieldValue.Day)
            {
                rangeBegin = rangeBegin.AddDays(-1.0);
            }
            rangeBegin = ComputeExpandBegin(rangeBegin, time, rule);
            time6 = ComputeExpandEnd(time6, time, time2, rule, timeZone);
            List<SPItem> list = new List<SPItem>();
            DateTime date = rangeBegin.Date;
            while (true)
            {
                DateTime itemBegin = ComputeTargetBegin(date, time, rule);
                DateTime time10 = ComputeTargetEnd(itemBegin, itemLength);
                TimeSpan span2 = (TimeSpan)(time10 - itemBegin);
                bool flag2 = span2.Ticks == 0L;
                if ((time6 < itemBegin) || ((time6 == itemBegin) && !flag2))
                {
                    return list;
                }
                if (0x3e7 <= list.Count)
                {
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
                    ExpandedCalendarItem item = null;
                    if ((rangeBegin < time10) || ((rangeBegin == time10) && (itemLength.Ticks == 0L)))
                    {
                        item = new ExpandedCalendarItem(masterItem);
                        if (boolFieldValue)
                        {
                            item[beginFieldName] = itemBegin;
                            item[endFieldName] = time10;
                        }
                        else
                        {
                            item[beginFieldName] = converter.ToLocal(itemBegin);
                            item[endFieldName] = converter.ToLocal(time10);
                        }
                        string str2 = GenerateRecurrenceItemId(SafeFieldAccessor.GetIntegerFieldValue(masterItem, "ID"), (DateTime)item[beginFieldName], localTZ, boolFieldValue);
                        item["ID"] = str2;
                        if (0x3e7 > list.Count)
                        {
                            list.Add(item);
                        }
                    }
                    date = IncrementDate(date, rule);
                }
            }
        }

        private static SPItem FindItemByBeginEnd(IList<SPItem> items, string beginFieldName, string endFieldName, DateTime start, DateTime end)
        {
            foreach (SPItem item in items)
            {
                DateTime time = (DateTime)item[beginFieldName];
                DateTime time2 = (DateTime)item[endFieldName];
                if ((time == start) && (time2 == end)) {
                    return item;
                }
            }
            return null;
        }

        private static SPItem FindItemById(IList<SPItem> items, string id)
        {
            foreach (SPItem item2 in items)
            {
                if (Convert.ToString(item2["ID"], CultureInfo.InvariantCulture) == id) {
                    return item2;
                }
            }
            return null;
        }

        private static string GenerateExceptionItemId(int itemId, int masterId, int type)
        {
            string str = "{0}.1.{1}";
            string str2 = "{0}.2.{1}";
            string format = str;
            if (type == 3) {
                format = str2;
            }
            return string.Format(CultureInfo.InvariantCulture, format, new object[] { itemId, masterId });
        }

        private static string GenerateRecurrenceItemId(int masterId, DateTime time, SPTimeZone timeZone, bool isAllDay)
        {
            DateTime time2 = isAllDay ? time : timeZone.LocalTimeToUTC(time);
            return string.Format(CultureInfo.InvariantCulture, "{0}.0.{1}", new object[] { masterId, SPUtility.CreateISO8601DateTimeFromSystemDateTime(time2) });
        }

        internal static int GetEventType(SPItem item)
        {
            return SafeFieldAccessor.GetIntegerFieldValue(item, "EventType");
        }

        internal static int GetMasterId(SPItem item)
        {
            int num = 0;
            int eventType = GetEventType(item);
            string stringFieldValue = SafeFieldAccessor.GetStringFieldValue(item, "ID");
            if ((eventType == 1) && !stringFieldValue.Contains<char>('.')) {
                return SafeFieldAccessor.GetIntegerFieldValue(item, "ID");
            }

            if ((eventType != 4) && (eventType != 3)) {
                return num;
            }

            return SafeFieldAccessor.GetIntegerFieldValue(item, "MasterSeriesItemID");
        }

        private static SPTimeZone GetTimeZone(SPItem item, SPTimeZone localTZ)
        {
            return localTZ;
        }

        private static DateTime GetValidDate(int year, int month, int day)
        {
            int num = day;
            DateTime time = new DateTime(year, month, 1);
            int num2 = time.AddMonths(1).AddDays(-1.0).Day;
            if (num2 < day) {
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
    }


}
