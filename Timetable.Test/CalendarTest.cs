using System;
using Xunit;

namespace Timetable.Test
{
    public class CalendarTest
    {
        [Fact]
        public void ToStringRunsOnBankHolidays()
        {
            var calendar = CreateCalendar();
            
            Assert.Equal("01/08/2019-31/08/2019 MoTuWeThFrSaSu", calendar.ToString());
        }

        private ICalendar CreateCalendar(DaysFlag dayMask = DaysFlag.Everyday, BankHolidayRunning bankHolidays = BankHolidayRunning.RunsOnBankHoliday)
        {
            var calendar = new Calendar()
            {
                RunsFrom = new DateTime(2019, 8, 1),
                RunsTo = new DateTime(2019, 8,31),
                DayMask = dayMask,
                BankHolidays = bankHolidays
            };
            calendar.Generate();
            return calendar;
        }

        [Fact]
        public void ToStringDoesNotRunOnBankHolidays()
        {
            var calendar = CreateCalendar(bankHolidays: BankHolidayRunning.DoesNotRunOnEnglishBankHolidays);
            
            Assert.Equal("01/08/2019-31/08/2019 MoTuWeThFrSaSu (E)", calendar.ToString());
        }
        
        public static TheoryData<DateTime, bool> CalendarBoundaries =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2019, 7, 31), false },
                {new DateTime(2019, 8, 1), true },
                {new DateTime(2019, 8, 31), true },
                {new DateTime(2019, 9, 1), false }
            };
        
        [Theory]
        [MemberData(nameof(CalendarBoundaries))]
        public void IsActiveWhenWithinCalendarRange(DateTime day, bool expected)
        {
            var calendar = CreateCalendar();
            
            Assert.Equal(expected, calendar.IsActiveOn(day));
        }


        private static DateTime Friday = new DateTime(2019, 8, 9);
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
            var calendar = CreateCalendar(dayMask: DaysFlag.Weekend);
            
            Assert.Equal(expected, calendar.IsActiveOn(day));
        }
        
        public static TheoryData<BankHolidayRunning,DateTime, bool> BankHolidays =>
            new TheoryData<BankHolidayRunning, DateTime, bool>()
            {
                {BankHolidayRunning.RunsOnBankHoliday, new DateTime(2019, 8, 5), true },    // Scotish Bank Holiday
                {BankHolidayRunning.RunsOnBankHoliday,new DateTime(2019, 8, 26), true },    // English Bank Holiday
                {BankHolidayRunning.RunsOnBankHoliday,new DateTime(2019, 8, 27), true },    // No Bank Holiday
                {BankHolidayRunning.DoesNotRunOnEnglishBankHolidays, new DateTime(2019, 8, 5), true },    // Scotish Bank Holiday
                {BankHolidayRunning.DoesNotRunOnEnglishBankHolidays,new DateTime(2019, 8, 26), false },    // English Bank Holiday
                {BankHolidayRunning.DoesNotRunOnEnglishBankHolidays,new DateTime(2019, 8, 27), true },    // No Bank Holiday
                {BankHolidayRunning.DoesNotRunOnScotishBankHolidays, new DateTime(2019, 8, 5), false },    // Scotish Bank Holiday
                {BankHolidayRunning.DoesNotRunOnScotishBankHolidays,new DateTime(2019, 8, 26), true },    // English Bank Holiday
                {BankHolidayRunning.DoesNotRunOnScotishBankHolidays,new DateTime(2019, 8, 27), true },    // No Bank Holiday
            };
        
        [Theory]
        [MemberData(nameof(BankHolidays))]
        public void IsActiveWhenRunsOnBankHolidays(BankHolidayRunning bankHolidays, DateTime day, bool expected)
        {
            var calendar = CreateCalendar(bankHolidays: bankHolidays);
            
            Assert.Equal(expected, calendar.IsActiveOn(day));
        }

        public static TheoryData<DateTime, bool> SingleDayCalendarBoundaries =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2019, 7, 31), false },
                {new DateTime(2019, 8, 1), true },
                {new DateTime(2019, 8, 2), false },
            };
        
        [Theory]
        [MemberData(nameof(SingleDayCalendarBoundaries))]
        public void HandlesSingleDayCalendars(DateTime day, bool expected)
        {
            var calendar = new Calendar()
            {
                RunsFrom = new DateTime(2019, 8, 1),
                RunsTo = new DateTime(2019, 8,1),
                DayMask = DaysFlag.Everyday,
                BankHolidays = BankHolidayRunning.RunsOnBankHoliday
            };
            calendar.Generate();
            
            Assert.Equal(expected, calendar.IsActiveOn(day));
        }

        [Fact]
        public void ThrowsExceptionIfCalendarCannotExist()
        {
            var calendar = new Calendar()
            {
                RunsFrom = new DateTime(2019, 8, 2),
                RunsTo = new DateTime(2019, 8,1),
                DayMask = DaysFlag.Everyday,
                BankHolidays = BankHolidayRunning.RunsOnBankHoliday
            };
            
            Assert.Throws<ArgumentOutOfRangeException>(() => calendar.Generate());           
        }
    }
}