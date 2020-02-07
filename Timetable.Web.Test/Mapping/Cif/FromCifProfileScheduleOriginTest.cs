using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
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
        public void MapLocation()
        {
            var output = Map();

            Assert.Equal(TestLocations.Surbiton, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static ScheduleOrigin Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.OriginLocation, Timetable.ScheduleOrigin>(
                Test.Cif.TestSchedules.CreateOriginLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void MapDeparture()
        {
            var output = Map();

            Assert.Equal(Ten, output.Departure);
            Assert.Equal(Ten.Add(TestTime.ThirtySeconds), output.WorkingDeparture);
        }

        [Fact]
        public void MapPlatform()
        {
            var output = Map();

            Assert.Equal("1", output.Platform);
        }

        [Fact]
        public void MapActivities()
        {
            var output = Map();

            Assert.Contains("TB", output.Activities);
        }
        
        [Fact]
        public void SetAdvertisedStop()
        {
            var output = Map();

            Assert.Equal(PublicStop.PickUpOnly, output.AdvertisedStop);
        } 
    }
}