using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedServiceToServiceTest
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

        private static Model.Service MapResolvedService(Timetable.Schedule schedule = null, bool isCancelled = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            schedule = schedule ?? TestSchedules.CreateScheduleWithService();
            var resolved = new ResolvedService(schedule, TestDate, isCancelled);
            
            var service = mapper.Map<Timetable.ResolvedService, Model.Service>(resolved, opts => opts.Items["On"] = resolved.On);
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
        public void MapLocations()
        {
            var output = MapResolvedService();
            Assert.NotEmpty(output.Stops);         
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