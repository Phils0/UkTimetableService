using System;
using Xunit;

namespace Timetable.Test
{
    public class CalendarExtensionsTest
    {
        [Theory]
        [InlineData(DaysFlag.Monday, "Mo")]
        [InlineData(DaysFlag.Tuesday, "Tu")]
        [InlineData(DaysFlag.Wednesday, "We")]
        [InlineData(DaysFlag.Thursday, "Th")]
        [InlineData(DaysFlag.Friday, "Fr")]
        [InlineData(DaysFlag.Saturday, "Sa")]
        [InlineData(DaysFlag.Sunday, "Su")]
        [InlineData(DaysFlag.None, "None")]
        [InlineData(DaysFlag.Weekdays, "MoTuWeThFr")]
        [InlineData(DaysFlag.Monday | DaysFlag.Wednesday, "MoWe")]
        public void DaysFlagToStingEx(DaysFlag days, string expected)
        {
            Assert.Equal(expected, days.ToStringEx());
        }
        
        [Theory]
        [InlineData(BankHolidayRunning.RunsOnBankHoliday, "")]
        [InlineData(BankHolidayRunning.DoesNotRunOnEnglishBankHolidays, "E")]
        [InlineData(BankHolidayRunning.DoesNotRunOnScotishBankHolidays, "S")]
        public void BankHolidayRunningToStingEx(BankHolidayRunning bankHoliday, string expected)
        {
            Assert.Equal(expected, bankHoliday.ToStringEx());
        }

        private static DateTime Friday = new DateTime(2019, 8, 9);
        
        [Theory]
        [InlineData(DaysFlag.Monday, 3)]
        [InlineData(DaysFlag.Tuesday, 4)]
        [InlineData(DaysFlag.Wednesday, 5)]
        [InlineData(DaysFlag.Thursday, 6)]
        [InlineData(DaysFlag.Friday, 0)]
        [InlineData(DaysFlag.Saturday, 1)]
        [InlineData(DaysFlag.Sunday, 2)]
        public void IsActiveOnSpecificDay(DaysFlag flag, int increment)
        {
            Assert.True(flag.IsActiveOnDay(Friday.AddDays(increment)));   
        }
        
        public static TheoryData<DateTime, bool> CalendarDays =>
            new TheoryData<DateTime, bool>()
            {
                {Friday, false },
                {Friday.AddDays(1), true },
                {Friday.AddDays(2), true },
                {Friday.AddDays(3), false }
            };
        
        [Theory]
        [MemberData(nameof(CalendarDays))]
        public void IsActiveWhenOnDay(DateTime day, bool expected)
        { 
            Assert.Equal(expected,  DaysFlag.Weekend.IsActiveOnDay(day));
        }

        [Theory]
        [InlineData(DaysFlag.Monday, "1")]
        [InlineData(DaysFlag.Tuesday, "2")]
        [InlineData(DaysFlag.Wednesday, "3")]
        [InlineData(DaysFlag.Thursday, "4")]
        [InlineData(DaysFlag.Friday, "5")]
        [InlineData(DaysFlag.Saturday, "6")]
        [InlineData(DaysFlag.Sunday, "7")]
        [InlineData(DaysFlag.None, "")]
        [InlineData(DaysFlag.Weekdays, "1,2,3,4,5")]
        [InlineData(DaysFlag.Monday | DaysFlag.Wednesday, "1,3")]
        public void DaysFlagToIsoDays(DaysFlag days, string expected)
        {
            Assert.Equal(expected, days.ToIsoDays());
        }
    }
}