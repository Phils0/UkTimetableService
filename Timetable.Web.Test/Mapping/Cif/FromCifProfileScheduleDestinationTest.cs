using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
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
        public void MapLocation()
        {
            var output = Map();

            Assert.Equal(TestLocations.WaterlooMain, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static ScheduleStop Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.TerminalLocation, Timetable.ScheduleStop>(
                Test.Cif.TestSchedules.CreateTerminalLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void MapArrival()
        {
            var output = Map();

            Assert.Equal(TenThirty, output.Arrival);
            Assert.Equal(TenThirty.Subtract(TestTime.ThirtySeconds), output.WorkingArrival);
        }
        
        [Fact]
        public void MapPlatform()
        {
            var output = Map();

            Assert.Equal("5", output.Platform);
        }

        [Fact]
        public void MapActivities()
        {
            var output = Map();

            Assert.Contains("TF", output.Activities.Value);
        } 
        
        [Fact]
        public void SetAdvertisedStop()
        {
            var output = Map();

            Assert.Equal(PublicStop.SetDownOnly, output.AdvertisedStop);
        } 
    }
}