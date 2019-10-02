using System;
using System.Collections.Generic;
using Xunit;

namespace Timetable.Test
{
    public class DateTimeFormattingTest
    {
        [Theory]
        [MemberData(nameof(FormatDateTestData))]
        public void FormatDateAsYearMonthDay(DateTime date, string expected)
        {
            Assert.Equal(expected, date.ToYMD());
        }

        public static IEnumerable<object[]> FormatDateTestData
        {
            get
            {
                yield return new object[] { new DateTime(2018, 1, 1), "2018-01-01" };
                yield return new object[] { new DateTime(2018, 2, 1), "2018-02-01" };
                yield return new object[] { new DateTime(2018, 10, 31), "2018-10-31" };
            }
        }

        [Theory]
        [MemberData(nameof(FormatDateTimeTestData))]
        public void FormatDateTimeAsYearMonthDayHourMin(DateTime date, string expected)
        {
            Assert.Equal(expected, date.ToYMDHms());
        }

        public static IEnumerable<object[]> FormatDateTimeTestData
        {
            get
            {
                yield return new object[] { new DateTime(2018, 2, 1), "2018-02-01 00:00:00" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 0), "2018-10-31 10:45:00" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 30), "2018-10-31 10:45:30" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 59), "2018-10-31 10:45:59" };
            }
        }

        [Theory]
        [MemberData(nameof(FormatTimeTestData))]
        public void FormatTimeAsYearMonthDayHourMin(DateTime date, string expected)
        {
            Assert.Equal(expected, date.ToHm());
        }

        public static IEnumerable<object[]> FormatTimeTestData
        {
            get
            {
                yield return new object[] { new DateTime(2018, 2, 1), "00:00" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 0), "10:45" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 30), "10:45" };
                yield return new object[] { new DateTime(2018, 10, 31, 10, 45, 59), "10:45" };
            }
        }
    }
}
