using System.Collections.Generic;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileStationTest
    {
        public static readonly MapperConfiguration ToViewProfileConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void StationMapThreeLetterCode()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Surbiton);
            
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void StationMapNlc()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Surbiton);
            
            Assert.Equal("5571", output.Nlc);
        }
        
        [Fact]
        public void StationMapLocations()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Waterloo);
            
            Assert.Equal(2, output.Locations.Count);
        }
        
        [Fact]
        public void StationMapTocs()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var station = TestStations.Waterloo;
            station.TocServices.Add(TestSchedules.VirginTrains);
            station.TocServices.Add(new Toc("SW"));

            var output = mapper.Map<Timetable.Station,  Model.Station>(station);
            
            Assert.Equal(2, output.TocServices.Count);
            Assert.Contains("VT", output.TocServices);
            Assert.Contains("SW", output.TocServices);
        }
        
        [Fact]
        public void StationArrayMap()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var stationDictionary = new Dictionary<string, Timetable.Station>()
            {
                {"WAT", TestStations.Waterloo},
                {"SUR", TestStations.Surbiton}
            };
            
            var output = mapper.Map<ICollection<Timetable.Station>,  Model.Station[]>(stationDictionary.Values);
            
            Assert.Equal(2, output.Length);
        }
    }
}