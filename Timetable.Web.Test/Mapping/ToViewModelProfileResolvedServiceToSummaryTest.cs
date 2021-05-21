using System;
using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedServiceToSummaryTest
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
            var output = MapResolvedService();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static Model.ServiceSummary MapResolvedService(Timetable.CifSchedule schedule = null, bool isCancelled = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            schedule = schedule ?? TestSchedules.CreateScheduleWithService();
            var resolved = new ResolvedService(schedule, TestDate, isCancelled);
            
            var service = mapper.Map<Timetable.ResolvedService, Model.ServiceSummary>(resolved, opts => opts.Items["On"] = resolved.On);
            return service;
        }

        [Fact]
        public void MapDate()
        {
            var output = MapResolvedService();            
            Assert.Equal(TestDate, output.Date);
        }
        
        [Fact]
        public void MapRetailServiceId()
        {
            var output = MapResolvedService();
            Assert.Equal("VT123400", output.RetailServiceId);         
        }
        
        [Fact]
        public void MapNrsRetailServiceId()
        {
            var output = MapResolvedService();
            Assert.Equal("VT1234", output.NrsRetailServiceId);         
        }
                
        [Fact]
        public void MapTrainIdentity()
        {
            var output = MapResolvedService();
            Assert.Equal("9Z12", output.TrainIdentity);         
        }
        
        [Fact]
        public void MapToc()
        {
            var output = MapResolvedService();
            Assert.Equal("VT", output.Operator);         
        }
        
        [Fact]
        public void MapSeatClass()
        {
            var output = MapResolvedService();
            Assert.Equal("Both", output.SeatClass);         
        }
        
        [Fact]
        public void MapSleeperClass()
        {
            var output = MapResolvedService();
            Assert.Equal("None", output.SleeperClass);         
        }
        
        [Fact]
        public void MapCatering()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            schedule.Properties.Catering = Catering.Buffet | Catering.Trolley;
            var output = MapResolvedService(schedule);
            Assert.Equal("Buffet, Trolley", output.Catering);         
        }
        
        [Fact]
        public void MapNoCatering()
        {
            var output = MapResolvedService();
            Assert.Equal("None", output.Catering);         
        }
        
        [Fact]
        public void MapReservationIndicator()
        {
            var output = MapResolvedService();
            Assert.Equal("Supported", output.ReservationIndicator);         
        }
        
        [Fact]
        public void MapStatus()
        {
            var output = MapResolvedService();
            Assert.Equal("P", output.Status);         
        }
        
        [Fact]
        public void MapCategory()
        {
            var output = MapResolvedService();
            Assert.Equal("XX", output.Category);         
        }
        
        [Fact]
        public void MapOrigin()
        {
            var output = MapResolvedService();
            Assert.Equal("SUR",output.Origin.Location.ThreeLetterCode);         
        }
        
        [Fact]
        public void MapDestination()
        {
            var output = MapResolvedService();
            Assert.Equal("WAT", output.Destination.Location.ThreeLetterCode);         
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MapIsCancelled(bool isCancelled)
        {
            var output = MapResolvedService(isCancelled: isCancelled);
            Assert.Equal(isCancelled, output.IsCancelled);         
        }
        
        [Fact]
        public void AsosciationsIsEmptyIfNone()
        {
            var output = MapResolvedService();
            Assert.Empty(output.Associations);         
        }
    }
}