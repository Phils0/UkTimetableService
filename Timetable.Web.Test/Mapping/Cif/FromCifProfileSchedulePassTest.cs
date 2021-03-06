using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
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
        public void MapLocation()
        {
            var output = Map();

            Assert.Equal(TestLocations.CLPHMJN, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static SchedulePass Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.IntermediateLocation, Timetable.SchedulePass>(
                Test.Cif.TestSchedules.CreatePassLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void MapPass()
        {
            var output = Map();

            Assert.Equal(TenFifteen, output.PassesAt);
        }

        [Fact]
        public void MapPlatform()
        {
            var output = Map();

            Assert.Equal("2", output.Platform);
        }

        [Fact]
        public void MapActivities()
        {
            var output = Map();

            Assert.Empty(output.Activities.Value);
        }    
        
        [Fact]
        public void SetAdvertisedStop()
        {
            var output = Map();

            Assert.Equal(PublicStop.No, output.AdvertisedStop);
        } 
    }
}