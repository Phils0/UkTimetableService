using System;
using AutoMapper;
using Microsoft.Extensions.Options;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;
using TestStations = Timetable.Test.Data.TestStations;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleToFoundItemTest
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
        public void MapService()
        {
            var output = MapResolvedStop();
            var service = output.Service;
            Assert.Equal("X12345", service.TimetableUid);
            Assert.Equal(TestDate, service.Date);
        }

        private static Model.FoundItem MapResolvedStop(Timetable.Schedule schedule = null, Timetable.Station to = null)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            schedule = schedule ?? TestSchedules.CreateScheduleWithService();
            
            var resolved = new ResolvedService(schedule, TestDate, false);
            resolved.TryFindStop(TestStations.Surbiton, TestSchedules.Ten, out var stop);
            if(to != null)
                stop.GoesTo(to);
            
            return mapper.Map<Timetable.ResolvedServiceStop, Model.FoundItem>(stop);
        }

        [Fact]
        public void MapStop()
        {
            var output = MapResolvedStop();            
            var stop = output.At;
            Assert.Equal("SUR", stop.Location.ThreeLetterCode);
            Assert.Equal(TestDate, stop.Departure.Value.Date);
        }
        
        [Fact]
        public void MapToStop()
        {
            var output = MapResolvedStop(to: TestStations.Waterloo);            
            var stop = output.To;
            Assert.Equal("WAT", stop.Location.ThreeLetterCode);
            Assert.Equal(TestDate, stop.Arrival.Value.Date);
        }
    }
}