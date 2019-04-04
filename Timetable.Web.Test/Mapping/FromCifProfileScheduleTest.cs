using System;
using AutoMapper;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleTest
    {
        private static readonly MapperConfiguration FromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            FromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void ScheduleMapTimetableUid()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal("X12345", output.TimetableUid);
        }
        
        [Fact]
        public void ScheduleMapStpIndicator()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(Timetable.StpIndicator.Permanent, output.StpIndicator);
        }
        
        [Fact]
        public void ScheduleMapStatus()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(ServiceStatus.PermanentPassenger, output.Status);
        }
        
        [Fact]
        public void ScheduleMapCategory()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(ServiceStatus.PermanentPassenger, output.Status);
        }
        
        [Fact]
        public void ScheduleMapSeatClass()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(AccomodationClass.Both, output.SeatClass);
        }
        
        [Fact]
        public void ScheduleMapSleeperClass()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(AccomodationClass.None, output.SleeperClass);
        }
        
        [Fact]
        public void ScheduleMapReservationIndicator()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal(ReservationIndicator.Recommended, output.ReservationIndicator);
        }
        
        [Fact]
        public void ScheduleMapCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            var calendar = output.On;
            Assert.Equal(new DateTime(2019, 8, 1), calendar.RunsFrom);
            Assert.Equal(new DateTime(2019, 8, 31), calendar.RunsTo);
            Assert.Equal(DaysFlag.Weekdays, calendar.DayMask);
            Assert.Equal(BankHolidayRunning.RunsOnBankHoliday, calendar.BankHolidays);
        }
        
        [Fact]
        public void ScheduleMapReusesExistingCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output1 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);
            var output2 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.NotSame(output1, output2);
            Assert.Same(output1.On, output2.On);
        }
        
        [Fact]
        public void ScheduleMapRetailServiceId()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.Equal("SW123400", output.RetailServiceId);
        }
        
        [Fact]
        public void ScheduleMapToc()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            var toc = output.Toc;
            Assert.Equal("SW", toc.Code);
            Assert.Equal("", toc.Name);
        }
        
        [Fact]
        public void ScheduleMapReusesExistingToc()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
           
            var output1 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);
            var output2 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test);

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Toc, output2.Toc);
        }
    }
}