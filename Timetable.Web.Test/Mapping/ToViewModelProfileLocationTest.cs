using System.Collections.Generic;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileLocationTest
    {
        public static readonly MapperConfiguration ToViewProfileConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void LocationMapThreeLetterCode()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location, Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void LocationMapTiploc()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SURBITN", output.Tiploc);
        }
        
        [Fact]
        public void LocationMapNlc()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("557100", output.Nlc);
        }
        
        [Fact]
        public void LocationMapName()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location,  Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("SURBITON", output.Name);
        }
        
        [Fact]
        public void LocationMapCoordinates()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

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
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location, Model.Location>(TestLocations.Surbiton);
            
            Assert.Equal("Normal", output.InterchangeStatus.ToString());
        }
    }
}