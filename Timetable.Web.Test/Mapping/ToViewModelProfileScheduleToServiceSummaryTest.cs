using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleToServiceSummaryTest
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

        private static Model.ServiceSummary MapSchedule(Timetable.Schedule schedule = null, bool isCancelled = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            schedule = schedule ?? TestSchedules.CreateScheduleWithService();

            var resolved = new ResolvedService(schedule, TestDate, isCancelled, TestSchedules.NoAssociations);
            
            return mapper.Map<Timetable.ResolvedService, Model.ServiceSummary>(resolved, opts => opts.Items["On"] = resolved.On);
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
        public void MapNrsRetailServiceId()
        {
            var output = MapSchedule();
            Assert.Equal("VT1234", output.NrsRetailServiceId);         
        }
                
        [Fact]
        public void MapTrainIdentity()
        {
            var output = MapSchedule();
            Assert.Equal("9Z12", output.TrainIdentity);         
        }
        
        [Fact]
        public void MapToc()
        {
            var output = MapSchedule();
            Assert.Equal("VT", output.Operator);         
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
        public void MapOrigin()
        {
            var output = MapSchedule();
            Assert.Equal("SUR",output.Origin.Location.ThreeLetterCode);         
        }
        
        [Fact]
        public void MapDestination()
        {
            var output = MapSchedule();
            Assert.Equal("WAT", output.Destination.Location.ThreeLetterCode);         
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MapIsCancelled(bool isCancelled)
        {
            var output = MapSchedule(isCancelled: isCancelled);
            Assert.Equal(isCancelled, output.IsCancelled);         
        }
    }
}