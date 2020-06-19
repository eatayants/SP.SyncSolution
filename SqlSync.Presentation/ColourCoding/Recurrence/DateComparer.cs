using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roster.Presentation.ColourCoding.Recurrence
{
    class DateComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return x.Date == y.Date;
        }

        public int GetHashCode(DateTime obj)
        {
            return obj.Date.GetHashCode();
        }
    }

    public class XDateTime
    {
        #region Private Class Member Variable

        public enum XDateTimeType { Calendar = 0, Business = 1 };
        private DateTime _date;
        private XDateTimeType _type;
        private List<DateTime> _holidays;

        #endregion

        #region Constructor

        public XDateTime(List<DateTime> holidays)
        {
            Init(DateTime.Now, XDateTimeType.Calendar, holidays);
        }

        public XDateTime(DateTime dateTime, List<DateTime> holidays)
        {
            Init(dateTime, XDateTimeType.Calendar, holidays);
        }

        public XDateTime(DateTime dateTime, XDateTimeType dateType, List<DateTime> holidays)
        {
            Init(dateTime, dateType, holidays);
            Check();
        }

        #endregion

        #region Public methods

        public XDateTimeType DateType
        {
            get { return _type; }
            set
            {
                _type = value;
                Check();
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                Check();
            }
        }

        public bool IsHoliday
        {
            get
            {
                return _holidays.Contains(_date, new DateComparer());
            }
        }

        public bool IsWeekEnd
        {
            get
            {
                return (_date.DayOfWeek == DayOfWeek.Saturday || _date.DayOfWeek == DayOfWeek.Sunday);
            }
        }

        public bool IsWorkDay
        {
            get
            {
                return !(IsWeekEnd || IsHoliday);
            }
        }

        public void AddBusinessDays(short days)
        {
            var sign = Convert.ToDouble(Math.Sign(days));
            var unsignedDays = Math.Sign(days) * days;
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    _date = _date.AddDays(sign);
                }
                while (!IsWorkDay);
            }
        }

        public DateTime NextBusinessDay()
        {
            return NextBusinessDay(null);
        }

        public DateTime NextBusinessDay(List<DateTime> occupied)
        {
            var occupiedDates = occupied ?? new List<DateTime>();
            var date = _date;
            do
            {
                date = date.AddDays(1.0);
            }
            while (IsHoliday || occupiedDates.Contains(date, new DateComparer()));
            return date;
        }

        public DateTime PreviousBusinessDay()
        {
            var date = _date;
            do
            {
                date = date.AddDays(-1.0);
            }
            while (IsWeekEnd || IsHoliday);
            return date;
        }

        #endregion

        #region Private methods

        private void Init(DateTime dateTime, XDateTimeType dateType, List<DateTime> holidays)
        {
            _date = dateTime;
            _type = dateType;
            InitHolidays(holidays);
        }
        private void InitHolidays(List<DateTime> holidays)
        {
            _holidays = holidays??new List<DateTime>();
        }

        private void Check()
        {
            if (_type == XDateTimeType.Business && !IsWorkDay)
            {
                _date = NextBusinessDay();
            }
        }

        #endregion
    }
}
