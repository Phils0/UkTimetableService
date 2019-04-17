using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleTest
    {
        private static readonly DateTime TestDate = TestTime.August1;
        
        private static readonly MapperConfiguration ToViewProfileConfiguration = 
            ToViewModelProfileLocationTest.ToViewProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }
        
        [Fact]
        public void MapTimetableId()
        {
            var output = MapSchedule();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static Model.Service MapSchedule(Timetable.Schedule schedule = null)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            schedule = schedule ?? TestData.CreateSchedule();

            return mapper.Map<Timetable.Schedule, Model.Service>(
                schedule, 
                o => { o.Items["On"] = TestDate; });
        }

        [Fact]
        public void MapDate()
        {
            var output = MapSchedule();            
            Assert.Equal(TestDate, output.Date);
        }
        
        [Fact]
        public void MapRetailServiceId()
        {
            var output = MapSchedule();
            Assert.Equal("VT123400", output.RetailServiceId);         
        }
        
        [Fact]
        public void MapToc()
        {
            var output = MapSchedule();
            Assert.Equal("VT", output.Toc);         
        }
        
        [Fact]
        public void MapSeatClass()
        {
            var output = MapSchedule();
            Assert.Equal("Both", output.SeatClass);         
        }
        
        [Fact]
        public void MapSleeperClass()
        {
            var output = MapSchedule();
            Assert.Equal("None", output.SleeperClass);         
        }
        
        [Fact]
        public void MapReservationIndicator()
        {
            var output = MapSchedule();
            Assert.Equal("Supported", output.ReservationIndicator);         
        }
        
        [Fact]
        public void MapStatus()
        {
            var output = MapSchedule();
            Assert.Equal("P", output.Status);         
        }
        
        [Fact]
        public void MapCategory()
        {
            var output = MapSchedule();
            Assert.Equal("XX", output.Category);         
        }
        
        [Fact]
        public void MapLocations()
        {
            var output = MapSchedule();
            Assert.NotEmpty(output.Stops);         
        }
    }
}