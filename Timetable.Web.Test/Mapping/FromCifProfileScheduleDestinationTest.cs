using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleDestinationTest
    {
        private static readonly Time TenThirty = new Time(TestTime.TenThirty);

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
            var output = Map();

            Assert.Equal(TestLocations.WaterlooMain, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static ScheduleDestination Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.TerminalLocation, Timetable.ScheduleDestination>(
                TestSchedules.CreateTerminalLocation(),
                o => o.Items.Add("Locations", TestData.Instance));
        }

        [Fact]
        public void ScheduleMapArrival()
        {
            var output = Map();

            Assert.Equal(TenThirty, output.Arrival);
            Assert.Equal(TenThirty.Subtract(TestTime.ThirtySeconds), output.WorkingArrival);
        }
        
        [Fact]
        public void ScheduleMapPlatform()
        {
            var output = Map();

            Assert.Equal("5", output.Platform);
        }

        [Fact]
        public void ScheduleMapActivities()
        {
            var output = Map();

            Assert.Contains("TF", output.Activities);
        }    }
}