using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleOriginTest
    {
        private static readonly Time Ten = new Time(TestTime.Ten);

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

            Assert.Equal(TestLocations.Surbiton, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static ScheduleOrigin Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.OriginLocation, Timetable.ScheduleOrigin>(
                TestSchedules.CreateOriginLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void ScheduleMapDeparture()
        {
            var output = Map();

            Assert.Equal(Ten, output.Departure);
            Assert.Equal(Ten.Add(TestTime.ThirtySeconds), output.WorkingDeparture);
        }

        [Fact]
        public void ScheduleMapPlatform()
        {
            var output = Map();

            Assert.Equal("1", output.Platform);
        }

        [Fact]
        public void ScheduleMapActivities()
        {
            var output = Map();

            Assert.Contains("TB", output.Activities);
        }
    }
}