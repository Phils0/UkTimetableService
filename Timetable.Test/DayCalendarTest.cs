using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class DayCalendarTest
    {
        public static TheoryData<DateTime,bool> Dates =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2019, 7, 31), false},
                {new DateTime(2019, 8, 1), true},
                {new DateTime(2019, 8, 1, 0, 0, 1), true},
                {new DateTime(2019, 8, 1, 23, 59, 59), true},
                {new DateTime(2019, 8, 2), false},
            };

        
        public DayCalendar TestDay => new DayCalendar(new DateTime(2019, 8, 1));
        
        [Theory]
        [MemberData(nameof(Dates))]
        public void IsActiveOn(DateTime date, bool expected)
        {
            Assert.Equal(expected, TestDay.IsActiveOn(date));
        }
        
        public static TheoryData<DayCalendar,int> ComparedDates =>
            new TheoryData<DayCalendar, int>()
            {
                {new DayCalendar(new DateTime(2019, 7, 31)),1},
                {new DayCalendar(new DateTime(2019, 8, 1)), 0},
                {new DayCalendar(new DateTime(2019, 8, 2)), -1},
                {null, -1}
            };
        
        [Theory]
        [MemberData(nameof(ComparedDates))]
        public void CompareToDayCalendar(DayCalendar calendar, int expected)
        {
            Assert.Equal(expected, TestDay.CompareTo(calendar));
        }
        
        public static TheoryData<CifCalendar,int> OrderingTests =>
            new TheoryData<CifCalendar, int>()
            {
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 7, 31), new DateTime(2019, 8, 31)), 1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 1)), 0},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 2), new DateTime(2019, 8, 2)), -1},
                {(CifCalendar) TestSchedules.CreateEverydayCalendar(new DateTime(2019, 8, 1), new DateTime(2019, 8, 2)), -1},
                {null, -1}
            };

        [Theory]
        [MemberData(nameof(OrderingTests))]
        public void CompareToCalendar(CifCalendar calendar, int expected)
        {
            Assert.Equal(expected, TestDay.CompareTo(calendar));
        }
    }
}