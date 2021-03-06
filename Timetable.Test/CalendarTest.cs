using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class CalendarTest
    {
        [Fact]
        public void ToStringRunsOnBankHolidays()
        {
            var calendar = TestSchedules.EverydayAugust2019;

            Assert.Equal("2019-08-01=>2019-08-31 MoTuWeThFrSaSu", calendar.ToString());
        }

        [Fact]
        public void ToStringDoesNotRunOnBankHolidays()
        {
            var calendar = TestSchedules.CreateAugust2019Calendar(bankHolidays: BankHolidayRunning.DoesNotRunOnEnglishBankHolidays);

            Assert.Equal("2019-08-01=>2019-08-31 MoTuWeThFrSaSu (E)", calendar.ToString());
        }

        public static TheoryData<DateTime, bool> CalendarBoundaries =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2019, 7, 31), false},
                {new DateTime(2019, 8, 1), true},
                {new DateTime(2019, 8, 31), true},
                {new DateTime(2019, 9, 1), false}
            };

        [Theory]
        [MemberData(nameof(CalendarBoundaries))]
        public void IsWithinCalendar(DateTime day, bool expected)
        {
            var calendar = (CifCalendar) TestSchedules.EverydayAugust2019;

            Assert.Equal(expected, calendar.IsWithinCalendar(day));
        }


        [Theory]
        [MemberData(nameof(CalendarBoundaries))]
        public void IsActiveWhenWithinCalendarRange(DateTime day, bool expected)
        {
            var calendar = TestSchedules.EverydayAugust2019;

            Assert.Equal(expected, calendar.IsActiveOn(day));
        }
       
        private static DateTime Friday = new DateTime(2019, 8, 9);

        public static TheoryData<DateTime, bool> CalendarDays =>
            new TheoryData<DateTime, bool>()
            {
                {Friday, false},
                {Friday.AddDays(1), true},
                {Friday.AddDays(2), true},
                {Friday.AddDays(3), false}
            };

        [Theory]
        [MemberData(nameof(CalendarDays))]
        public void IsActiveWhenOnDay(DateTime day, bool expected)
        {
            var calendar = TestSchedules.CreateAugust2019Calendar(dayMask: DaysFlag.Weekend);

            Assert.Equal(expected, calendar.IsActiveOn(day));
        }

        public static TheoryData<BankHolidayRunning, DateTime, bool> BankHolidays =>
            new TheoryData<BankHolidayRunning, DateTime, bool>()
            {
                {BankHolidayRunning.RunsOnBankHoliday, new DateTime(2021, 8, 2), true}, // Scotish Bank Holiday
                {BankHolidayRunning.RunsOnBankHoliday, new DateTime(2021, 8, 30), true}, // English Bank Holiday
                {BankHolidayRunning.RunsOnBankHoliday, new DateTime(2021, 8, 27), true}, // No Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnEnglishBankHolidays, new DateTime(2021, 8, 2), true
                }, // Scotish Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnEnglishBankHolidays, new DateTime(2021, 8, 30), false
                }, // English Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnEnglishBankHolidays, new DateTime(2021, 8, 27), true
                }, // No Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnScotishBankHolidays, new DateTime(2021, 8, 2), false
                }, // Scotish Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnScotishBankHolidays, new DateTime(2021, 8, 30), true
                }, // English Bank Holiday
                {
                    BankHolidayRunning.DoesNotRunOnScotishBankHolidays, new DateTime(2021, 8, 27), true
                }, // No Bank Holiday
            };

        public static ICalendar CreateAugust2021Calendar(DaysFlag dayMask = DaysFlag.Everyday,
            BankHolidayRunning bankHolidays = BankHolidayRunning.RunsOnBankHoliday)
        {
            var calendar = new CifCalendar(
                new DateTime(2021, 8, 1),
                new DateTime(2021, 8, 31),
                dayMask,
                bankHolidays);
            calendar.Generate();
            return calendar;
        }
        
        [Theory]
        [MemberData(nameof(BankHolidays))]
        public void IsActiveWhenRunsOnBankHolidays(BankHolidayRunning bankHolidays, DateTime day, bool expected)
        {
            var calendar = CreateAugust2021Calendar(bankHolidays: bankHolidays);

            Assert.Equal(expected, calendar.IsActiveOn(day));
        }

        public static TheoryData<DateTime, bool> SingleDayCalendarBoundaries =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2019, 7, 31), false},
                {new DateTime(2019, 8, 1), true},
                {new DateTime(2019, 8, 2), false},
            };

        [Theory]
        [MemberData(nameof(SingleDayCalendarBoundaries))]
        public void HandlesSingleDayCalendars(DateTime day, bool expected)
        {
            var calendar = new CifCalendar(
                new DateTime(2019, 8, 1),
                new DateTime(2019, 8, 1),
                DaysFlag.Everyday,
                BankHolidayRunning.RunsOnBankHoliday);
            calendar.Generate();

            Assert.Equal(expected, calendar.IsActiveOn(day));
        }

        [Fact]
        public void ThrowsExceptionIfCalendarCannotExist()
        {
            var calendar = new CifCalendar(
                new DateTime(2019, 8, 2),
                new DateTime(2019, 8, 1),
                DaysFlag.Everyday,
                BankHolidayRunning.RunsOnBankHoliday);

            Assert.Throws<ArgumentOutOfRangeException>(() => calendar.Generate());
        }
        
        public static TheoryData<CifCalendar,int> ComparisonCalendars =>
            new TheoryData<CifCalendar, int>()
            {
                {(CifCalendar) TestSchedules.EverydayAugust2019, 0},
                {(CifCalendar) TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), 1},
                {(CifCalendar) TestSchedules.CreateAugust2019Calendar(bankHolidays: BankHolidayRunning.DoesNotRunOnEnglishBankHolidays), -1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 2), new DateTime(2019, 8, 31)), -1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 7, 31), new DateTime(2019, 8, 31)), 1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 30)), 1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 9, 1)), -1},
                {null, -1}
            };

        [Theory]
        [MemberData(nameof(ComparisonCalendars))]
        public void CompareTo(CifCalendar calendar, int expected)
        {
            var calendar1 = (CifCalendar) TestSchedules.EverydayAugust2019;
            Assert.Equal(expected, calendar1.CompareTo(calendar));
        }
        
    }
}