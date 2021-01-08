using System;
using System.Collections;

namespace Timetable
{
    /// <summary>
    /// Days a service runs on
    /// </summary>
    [Flags]
    public enum DaysFlag
    {
        None = 0,
        Monday = 1 << 0,
        Tuesday = 1 << 1,
        Wednesday = 1 << 2,
        Thursday = 1 << 3,
        Friday = 1 << 4,
        Saturday = 1 << 5,
        Sunday = 1 << 6,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,
        Everyday = Weekdays | Weekend
    }

    /// <summary>
    /// Service runs on bank holidays?
    /// </summary>
    public enum BankHolidayRunning
    {
        RunsOnBankHoliday = 0,
        DoesNotRunOnEnglishBankHolidays = 1, // X 
        DoesNotRunOnScotishBankHolidays = 2 // G
    }

    public interface ICalendar
    {
        /// <summary>
        /// The calendar container is active on the day
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns></returns>
        bool IsActiveOn(DateTime date);
    }

    /// <summary>
    /// Schedule Calendar
    /// </summary>
    public class Calendar : ICalendar, IEquatable<Calendar>, IComparable<Calendar>
    {
        public DateTime RunsFrom { get; }

        public DateTime RunsTo { get; }

        public DaysFlag DayMask { get; }

        public BankHolidayRunning BankHolidays { get; } = BankHolidayRunning.RunsOnBankHoliday;

        private BitArray _calendarMask;

        public Calendar(DateTime runsFrom, DateTime runsTo, DaysFlag days, BankHolidayRunning bankHolidays)
        {
            RunsFrom = runsFrom;
            RunsTo = runsTo;
            DayMask = days;
            BankHolidays = bankHolidays;
        }

        public void Generate()
        {
            // Make idempotent, don't regenerate mask
            if (_calendarMask != null)
                return;

            var days = (int) ((RunsTo.Date - RunsFrom.Date).TotalDays) + 1;

            if (days <= 0)
                throw new ArgumentOutOfRangeException($"Negative calendar range {nameof(RunsFrom)} > {nameof(RunsTo)}");

            _calendarMask = new BitArray(days);
            for (int i = 0; i < days; i++)
            {
                _calendarMask[i] = IsActiveOnDay(RunsFrom.AddDays(i));
            }
        }

        private bool IsActiveOnDay(DateTime day)
        {
            if (!DayMask.IsActiveOnDay(day))
                return false;

            switch (BankHolidays)
            {
                case BankHolidayRunning.RunsOnBankHoliday:
                    return true;
                case BankHolidayRunning.DoesNotRunOnEnglishBankHolidays:
                    return !Timetable.BankHolidays.IsEnglishBankHoliday(day);
                case BankHolidayRunning.DoesNotRunOnScotishBankHolidays:
                    return !Timetable.BankHolidays.IsScotishBankHoliday(day);
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled BankHolidayRunning enum value: {BankHolidays}");
            }
        }

        public bool IsActiveOn(DateTime date)
        {
            date = date.Date;
            if (!IsWithinCalendar(date))
                return false;

            var dayIdx = (int) (date - RunsFrom).TotalDays;
            return _calendarMask[dayIdx];
        }

        public bool IsWithinCalendar(DateTime date)
        {
            return RunsFrom <= date && date <= RunsTo;
        }

        public bool Equals(Calendar other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RunsFrom.Equals(other.RunsFrom) && RunsTo.Equals(other.RunsTo) && DayMask == other.DayMask &&
                   BankHolidays == other.BankHolidays;
        }
        
        public int CompareTo(Calendar other)
        {
            if (ReferenceEquals(null, other)) return -1;

            var compare = RunsFrom.CompareTo(other.RunsFrom);
            if (compare != 0)
                return compare;

            compare = RunsTo.CompareTo(other.RunsTo);
            if (compare != 0)
                return compare;

            compare = DayMask.CompareTo(other.DayMask);
            if (compare != 0)
                return compare;               
                
            return BankHolidays.CompareTo(other.BankHolidays);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Calendar) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RunsFrom.GetHashCode();
                hashCode = (hashCode * 397) ^ RunsTo.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) DayMask;
                hashCode = (hashCode * 397) ^ (int) BankHolidays;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return BankHolidays == BankHolidayRunning.RunsOnBankHoliday
                ? $"{RunsFrom.ToYMD()}=>{RunsTo.ToYMD()} {DayMask.ToStringEx()}"
                : $"{RunsFrom.ToYMD()}=>{RunsTo.ToYMD()} {DayMask.ToStringEx()} ({BankHolidays.ToStringEx()})";
        }
    }
}