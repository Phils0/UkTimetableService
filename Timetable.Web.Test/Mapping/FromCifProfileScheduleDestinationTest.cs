using System;
using AutoMapper;
using Timetable.Test.Data;
using Cif = Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleDestinationTest
    {
        private static readonly Time TenThirty = new Time(Cif.TestTime.TenThirty);

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

        private static ScheduleDestination Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.TerminalLocation, Timetable.ScheduleDestination>(
                Cif.TestSchedules.CreateTerminalLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void MapArrival()
        {
            var output = Map();

            Assert.Equal(TenThirty, output.Arrival);
            Assert.Equal(TenThirty.Subtract(Cif.TestTime.ThirtySeconds), output.WorkingArrival);
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

            Assert.Contains("TF", output.Activities);
        } 
        
        [Fact]
        public void SetAdvertisedStop()
        {
            var output = Map();

            Assert.Equal(StopType.SetDownOnly, output.AdvertisedStop);
        } 
    }
}