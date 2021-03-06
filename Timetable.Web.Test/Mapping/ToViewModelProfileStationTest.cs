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
            
            Assert.Equal("557100", output.Nlc);
        }
        
        [Fact]
        public void StationMapName()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Surbiton);
            
            Assert.Equal("Surbiton", output.Name);
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
        
        [Fact]
        public void StationMapCordinates()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var surbiton = TestStations.Surbiton;
            var output = mapper.Map<Timetable.Station,  Model.Station>(surbiton);
            
            Assert.Equal(new decimal(-0.303959858), output.Coordinates.Longitude);
            Assert.Equal(new decimal(51.39246129), output.Coordinates.Latitude);
        }
        
        [Fact]
        public void StationMapNoCordinates()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Waterloo);
            
            Assert.Null(output.Coordinates);
        }
        
        [Fact]
        public void StationMapStationOperator()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Surbiton);
            
            Assert.Equal("SW", output.StationOperator);
        }
    }
}