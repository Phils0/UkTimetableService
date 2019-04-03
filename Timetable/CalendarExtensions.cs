using System;
using System.Text;

namespace Timetable
{
    public static class CalendarExtensions
    {
        public static string ToStringEx(this DaysFlag flag)
        {
            var s = new StringBuilder();

            if (flag.HasFlag(DaysFlag.Monday))
                s.Append("Mo");
            if (flag.HasFlag(DaysFlag.Tuesday))
                s.Append("Tu");
            if (flag.HasFlag(DaysFlag.Wednesday))
                s.Append("We");
            if (flag.HasFlag(DaysFlag.Thursday))
                s.Append("Th");
            if (flag.HasFlag(DaysFlag.Friday))
                s.Append("Fr");
            if (flag.HasFlag(DaysFlag.Saturday))
                s.Append("Sa");
            if (flag.HasFlag(DaysFlag.Sunday))
                s.Append("Su");

            var o = s.ToString();
            return o == "" ? "None" : o;
        }
        
        public static string ToStringEx(this BankHolidayRunning b)
        {
            switch (b)
            {
                case BankHolidayRunning.RunsOnBankHoliday:
                    return "";
                case BankHolidayRunning.DoesNotRunOnEnglishBankHolidays:
                    return "E";
                case BankHolidayRunning.DoesNotRunOnScotishBankHolidays:
                    return "S";
                default:
                    return b.ToString();
            }
        }

        public static bool IsActiveOnDay(this DaysFlag mask, DateTime day)
        {
            switch (day.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return mask.HasFlag(DaysFlag.Monday);
                case DayOfWeek.Tuesday:
                    return mask.HasFlag(DaysFlag.Tuesday);
                case DayOfWeek.Wednesday:
                    return mask.HasFlag(DaysFlag.Wednesday);
                case DayOfWeek.Thursday:
                    return mask.HasFlag(DaysFlag.Thursday);
                case DayOfWeek.Friday:
                    return mask.HasFlag(DaysFlag.Friday);
                case DayOfWeek.Saturday:
                    return mask.HasFlag(DaysFlag.Saturday);
                case DayOfWeek.Sunday:
                    return mask.HasFlag(DaysFlag.Sunday);
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled day {day.DayOfWeek} - {day}");
            }
        }
    }
}