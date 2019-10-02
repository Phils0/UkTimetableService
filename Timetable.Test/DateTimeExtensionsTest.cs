using System;
using System.Collections.Generic;
using Xunit;

namespace Timetable.Test
{
    public class DateTimeExtensionsTest
    {
        private static readonly DateTime Day = new DateTime(2018, 10, 31);

        public static IEnumerable<object[]> MinuteTestData
        {
            get
            {
                yield return new object[] { new TimeSpan(8, 0, 0), new DateTime(2018, 10, 31, 8, 0, 0) };
                yield return new object[] { new TimeSpan(8, 0, 29), new DateTime(2018, 10, 31, 8, 0, 0) };
                yield return new object[] { new TimeSpan(8, 0, 30), new DateTime(2018, 10, 31, 8, 1, 0) };
                yield return new object[] { new TimeSpan(8, 0, 31), new DateTime(2018, 10, 31, 8, 1, 0) };
                yield return new object[] { new TimeSpan(8, 0, 59), new DateTime(2018, 10, 31, 8, 1, 0) };
                yield return new object[] { new TimeSpan(8, 1, 30), new DateTime(2018, 10, 31, 8, 2, 0) };
                yield return new object[] { new TimeSpan(23, 59, 29), new DateTime(2018, 10, 31, 23, 59, 0) };
                yield return new object[] { new TimeSpan(23, 59, 30), new DateTime(2018, 11, 1, 0, 0, 0) };
            }
        }

        [Theory]
        [MemberData(nameof(MinuteTestData))]
        public void RoundToNearestMinute(TimeSpan time, DateTime expected)
        {
            var rounded = Day.Add(time).RoundToMinute();

            Assert.Equal(expected, rounded);
        }


    }
}
