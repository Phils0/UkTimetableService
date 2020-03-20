using System.Linq;
using AutoMapper;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class FromCifProfileScheduleLocationsTest
    {
        private static readonly MapperConfiguration FromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            FromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapLocation()
        {
            var output = MapSchedule();
            Assert.NotEmpty(output.Locations);
        }

        private static Schedule MapSchedule()
        {
            return FromCifProfileScheduleTest.MapSchedule();
        }
        
        [Fact]
        public void MapOriginLocation()
        {
            var output = MapSchedule();
            var location = output.Locations.First();
            Assert.IsType<ScheduleStop>(location);
        }
        
        [Fact]
        public void MapTerminalLocation()
        {
            var output = MapSchedule();
            var location = output.Locations.Last();
            Assert.IsType<ScheduleStop>(location);
        }
        
        [Fact]
        public void MapPassingLocation()
        {
            var output = MapSchedule();
            Assert.NotEmpty(output.Locations.OfType<SchedulePass>());
        }
        
        [Fact]
        public void MapStoppingLocation()
        {
            var output = MapSchedule();
            Assert.NotEmpty(output.Locations.OfType<ScheduleStop>());
        }
        
        [Fact]
        public void UniqueIds()
        {
            var output = MapSchedule();

            var uniqueIds = output.Locations
                .Select(l => l.Id)
                .Distinct()
                .Count();
            
            Assert.Equal(output.Locations.Count(), uniqueIds);
        }
        
        [Fact]
        public void ParentInScheduleLocationSetToSchedule()
        {
            var output = MapSchedule();

            var first = output.Locations.First();            
            Assert.Same(output, first.Schedule);
            var last = output.Locations.First();            
            Assert.Same(output, last.Schedule);
        }
    }
}