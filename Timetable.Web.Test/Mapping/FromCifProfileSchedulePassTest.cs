using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileSchedulePassTest
    {
        private static readonly Time TenFifteen = new Time(TestTime.TenFifteen);

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

            Assert.Equal(TestLocations.CLPHMJN, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static SchedulePass Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.IntermediateLocation, Timetable.SchedulePass>(
                TestSchedules.CreatePassLocation(),
                o => o.Items.Add("Locations", TestData.Instance));
        }

        [Fact]
        public void ScheduleMapPass()
        {
            var output = Map();

            Assert.Equal(TenFifteen, output.PassesAt);
        }

        [Fact]
        public void ScheduleMapPlatform()
        {
            var output = Map();

            Assert.Equal("2", output.Platform);
        }

        [Fact]
        public void ScheduleMapActivities()
        {
            var output = Map();

            Assert.Empty(output.Activities);
        }    }
}