using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Cif = Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleStopTest
    {
        private static readonly Time TenFifteen = new Time(Cif.TestTime.TenFifteen);

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

            Assert.Equal(TestLocations.CLPHMJC, output.Location);
            Assert.Equal(1, output.Sequence);
        }

        private static ScheduleStop Map()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.IntermediateLocation, Timetable.ScheduleStop>(
                Cif.TestSchedules.CreateIntermediateLocation(),
                o => o.Items.Add("Locations", TestData.Locations));
        }

        [Fact]
        public void MapArrival()
        {
            var output = Map();

            Assert.Equal(TenFifteen.Subtract(Cif.TestTime.OneMinute), output.Arrival);
            Assert.Equal(TenFifteen.Subtract(Cif.TestTime.ThirtySeconds), output.WorkingArrival);
        }
        
        [Fact]
        public void MapDeparture()
        {
            var output = Map();

            Assert.Equal(TenFifteen, output.Departure);
            Assert.Equal(TenFifteen.Add(Cif.TestTime.ThirtySeconds), output.WorkingDeparture);
        }

        [Fact]
        public void MapPlatform()
        {
            var output = Map();

            Assert.Equal("10", output.Platform);
        }

        [Fact]
        public void MapActivities()
        {
            var output = Map();

            Assert.Contains("T", output.Activities);
        }    
        
        [Fact]
        public void SetAdvertisedStop()
        {
            var output = Map();

            Assert.Equal(PublicStop.Yes, output.AdvertisedStop);
        } 
    }
}