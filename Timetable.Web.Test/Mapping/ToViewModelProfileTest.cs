using System.Collections.Generic;
using AutoMapper;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        [Fact]
        public void ValidMapping()
        {
            _config.AssertConfigurationIsValid();
        }

        [Fact]
        public void LocationMapThreeLetterCode()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Location, Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void LocationMapTiploc()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SURBITN", output.Tiploc);
        }
        
        [Fact]
        public void LocationMapName()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SURBITON", output.Name);
        }
        
        [Fact]
        public void LocationMapCoordinates()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);

            var expected = new  Model.Coordinates()
            {
                Eastings = 15181,
                Northings = 61673,
                IsEstimate = false
            };
            Assert.Equal(expected, output.Coordinates);
        }
        
        [Fact]
        public void LocationMapInterchangeStatus()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Location, Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("Normal", output.InterchangeStatus.ToString());
        }
        
        [Fact]
        public void StationMapThreeLetterCode()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Surbiton);
            
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void StationMapLocations()
        {
            var mapper = _config.CreateMapper();

            var output = mapper.Map<Timetable.Station,  Model.Station>(TestStations.Waterloo);
            
            Assert.Equal(2, output.Locations.Count);
        }
        
        [Fact]
        public void StationArrayMap()
        {
            var mapper = _config.CreateMapper();

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