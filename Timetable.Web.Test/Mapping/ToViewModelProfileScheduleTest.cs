using System;
using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleTest
    {
        private static readonly DateTime August1 = new DateTime(2019, 8, 1);
        
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
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("X12345", output.TimetableUid);
        }
        
        [Fact]
        public void MapDate()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("X12345", output.TimetableUid);
            Assert.Equal(August1, output.Date);
            Assert.Equal("VT123400", output.RetailServiceId);         
        }
        
        [Fact]
        public void MapRetailServiceId()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("VT123400", output.RetailServiceId);         
        }
        
        [Fact]
        public void MapToc()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("VT", output.Toc);         
        }
        
        [Fact]
        public void MapSeatClass()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("Both", output.SeatClass);         
        }
        
        [Fact]
        public void MapSleeperClass()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("None", output.SleeperClass);         
        }
        
        [Fact]
        public void MapReservationIndicator()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("Supported", output.ReservationIndicator);         
        }
        
        [Fact]
        public void MapStatus()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("P", output.Status);         
        }
        
        [Fact]
        public void MapCategory()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.Equal("XX", output.Category);         
        }
        
        [Fact]
        public void MapLocations()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Schedule, Model.Service>(
                TestData.CreateSchedule(),
                o => { o.Items["On"] = August1; });
            
            Assert.NotEmpty(output.Stops);         
        }
    }
}