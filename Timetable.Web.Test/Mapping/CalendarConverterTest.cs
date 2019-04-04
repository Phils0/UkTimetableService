using System;
using AutoMapper;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class CalendarConverterTest
    {
        private static readonly MapperConfiguration FromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;
     
        [Fact]
        public void ScheduleMapReusesExistingCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            var context = new ResolutionContext(null, (IRuntimeMapper) mapper);
           
            var converter = new CalendarConverter();

            var output1 = converter.Convert(TestSchedules.CreateScheduleDetails(), context);
            var output2 = converter.Convert(TestSchedules.CreateScheduleDetails(), context);

            Assert.Same(output1, output2);
        }

        [Theory]
        [InlineData("NNNNNNN", DaysFlag.None)]
        [InlineData("YNNNNNN", DaysFlag.Monday)]
        [InlineData("NYNNNNN", DaysFlag.Tuesday)]
        [InlineData("NNYNNNN", DaysFlag.Wednesday)]
        [InlineData("NNNYNNN", DaysFlag.Thursday)]
        [InlineData("NNNNYNN", DaysFlag.Friday)]
        [InlineData("NNNNNYN", DaysFlag.Saturday)]
        [InlineData("NNNNNNY", DaysFlag.Sunday)]
        [InlineData("YYYYYYY", DaysFlag.Everyday)]
        public void MapDaysMask(string source, DaysFlag expected)
        {
            Assert.Equal(expected, CalendarConverter.MapMask(source));
        }
        
        [Theory]
        [InlineData("", BankHolidayRunning.RunsOnBankHoliday)]
        [InlineData("X", BankHolidayRunning.DoesNotRunOnEnglishBankHolidays)]
        [InlineData("G", BankHolidayRunning.DoesNotRunOnScotishBankHolidays)]
        public void MapBankHolidays(string source, BankHolidayRunning expected)
        {
            Assert.Equal(expected, CalendarConverter.MapBankHoliday(source));
        }

        [Fact]
        public void FailsToMapEdinburghBankHolidays()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CalendarConverter.MapBankHoliday("E"));
        }
    }
}