using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class CalendarComparerTest
    {
        public static DayCalendar August1 => new DayCalendar(new DateTime(2019, 8, 1));
        
        public static TheoryData<ICalendar, ICalendar,int> OrderingTests =>
            new TheoryData<ICalendar, ICalendar, int>()
            {
                {TestSchedules.EverydayAugust2019, TestSchedules.EverydayAugust2019, 0},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), 1},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateAugust2019Calendar(bankHolidays: BankHolidayRunning.DoesNotRunOnEnglishBankHolidays), -1},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 2), new DateTime(2019, 8, 31)), -1},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 7, 31), new DateTime(2019, 8, 31)), 1},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 30)), 1},
                {TestSchedules.EverydayAugust2019, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 9, 1)), -1},
                {TestSchedules.EverydayAugust2019, new DayCalendar(new DateTime(2019, 7, 31)), 1},
                {TestSchedules.EverydayAugust2019, new DayCalendar(new DateTime(2019, 8, 1)), 1},
                {TestSchedules.EverydayAugust2019, new DayCalendar(new DateTime(2019, 8, 2)), -1},
                {TestSchedules.EverydayAugust2019, null, -1},
                {August1, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 7, 31), new DateTime(2019, 8, 31)), 1},
                {August1, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 1)), 0},
                {August1, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 2), new DateTime(2019, 8, 2)), -1},
                {August1, TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 2)), -1},
                {August1, new DayCalendar(new DateTime(2019, 7, 31)), 1},
                {August1, August1, 0},
                {August1, new DayCalendar(new DateTime(2019, 8, 2)), -1},
                {August1, null, -1},
                {null, null, 0}
            };

        [Theory]
        [MemberData(nameof(OrderingTests))]
        public void CalendarsAreOrdered(ICalendar calendar1, ICalendar calendar2, int expected)
        {
            var comparer = new CalendarComparer();
            Assert.Equal(expected, comparer.Compare(calendar1, calendar2));
        }
        
        [Theory]
        [MemberData(nameof(OrderingTests))]
        public void CalendarsAreReversed(ICalendar calendar1, ICalendar calendar2, int expected)
        {
            var comparer = new CalendarComparer();
            Assert.Equal(-expected, comparer.Compare(calendar2, calendar1));
        }
    }
}