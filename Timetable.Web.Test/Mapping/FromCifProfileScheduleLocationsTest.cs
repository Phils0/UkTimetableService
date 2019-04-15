using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CifParser;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
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
        public void ScheduleMapLocation()
        {
            var output = FromCifProfileScheduleTest.MapSchedule();
            Assert.NotEmpty(output.Locations);
        }

        [Fact]
        public void ScheduleMapOriginLocation()
        {
            var output = FromCifProfileScheduleTest.MapSchedule();
            var location = output.Locations.First();
            Assert.IsType<ScheduleOrigin>(location);
        }
        
        [Fact]
        public void ScheduleMapTerminalLocation()
        {
            var output = FromCifProfileScheduleTest.MapSchedule();
            var location = output.Locations.Last();
            Assert.IsType<ScheduleDestination>(location);
        }
        
        [Fact]
        public void ScheduleMapPassingLocation()
        {
            var output = FromCifProfileScheduleTest.MapSchedule();
            Assert.NotEmpty(output.Locations.OfType<SchedulePass>());
        }
        
        [Fact]
        public void ScheduleMapStoppingLocation()
        {
            var output = FromCifProfileScheduleTest.MapSchedule();
            Assert.NotEmpty(output.Locations.OfType<ScheduleStop>());
        }
    }
}