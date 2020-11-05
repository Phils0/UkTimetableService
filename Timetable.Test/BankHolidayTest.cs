using System;
using Xunit;

namespace Timetable.Test
{
    public class BankHolidayTest
    {
        public static TheoryData<DateTime, bool> EnglishData =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2021, 8, 2), false },    // Scotish Bank Holiday
                {new DateTime(2021, 8, 30), true },    // English Bank Holiday
                {new DateTime(2021, 8, 27), false }    // No Bank Holiday
            };


        [Theory]
        [MemberData(nameof(EnglishData))]
        public void IsEnglishBankHoliday(DateTime day, bool expected)
        {
            Assert.Equal(expected, BankHolidays.IsEnglishBankHoliday(day));
        }
        
        public static TheoryData<DateTime, bool> ScotishData =>
            new TheoryData<DateTime, bool>()
            {
                {new DateTime(2021, 8, 2), true },    // Scotish Bank Holiday
                {new DateTime(2021, 8, 30), false },    // English Bank Holiday
                {new DateTime(2021, 8, 27), false }    // No Bank Holiday
            };
        
        [Theory]
        [MemberData(nameof(ScotishData))]
        public void IsScotishBankHoliday(DateTime day, bool expected)
        {
            Assert.Equal(expected, BankHolidays.IsScotishBankHoliday(day));
        }
    }
}