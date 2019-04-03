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
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
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
        DoesNotRunOnEnglishBankHolidays = 1,   // X 
        DoesNotRunOnScotishBankHolidays = 2    // G
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
    public class Calendar : ICalendar, IEquatable<Calendar>
    {
        public DateTime RunsFrom { get; set; }
        
        public DateTime RunsTo { get; set; }
        
        public DaysFlag DayMask { get; set; }

        public BankHolidayRunning BankHolidays { get; set; } = BankHolidayRunning.RunsOnBankHoliday;

        private BitArray _calendarMask;
        
        public void Generate()
        {
            var days = (int) ((RunsTo.Date - RunsFrom.Date).TotalDays) + 1;
            
            if(days <= 0)
                throw new ArgumentOutOfRangeException($"Negative calendar range {nameof(RunsFrom)} > {nameof(RunsTo)}");
            
            _calendarMask = new BitArray(days);
            for (int i = 0; i < days; i++)
            {
                _calendarMask[i] = IsActiveOnDay(RunsFrom.AddDays(i));
            }            
        }

        private bool IsActiveOnDay(DateTime day)
        {
            if(!DayMask.IsActiveOnDay(day))
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
            if(!IsWithinCalendar(date))
                return false;

            var dayIdx = (int) (date - RunsFrom).TotalDays;
            return _calendarMask[dayIdx];
        }

        private bool IsWithinCalendar(DateTime date)
        {
            return RunsFrom <= date && date <= RunsTo;
        }

        public bool Equals(Calendar other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RunsFrom.Equals(other.RunsFrom) && RunsTo.Equals(other.RunsTo) && DayMask == other.DayMask && BankHolidays == other.BankHolidays;
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
            return BankHolidays == BankHolidayRunning.RunsOnBankHoliday ?
                $"{RunsFrom:d}-{RunsTo:d} {DayMask.ToStringEx()}" :
                $"{RunsFrom:d}-{RunsTo:d} {DayMask.ToStringEx()} ({BankHolidays.ToStringEx()})";
        }
    }
}