using AutoMapper;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileLocationsTest
    {
        public static readonly MapperConfiguration FromCifProfileConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());

        [Fact]
        public void ValidMapping()
        {
            FromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void LocationMapThreeLetterCode()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output = mapper.Map<CifParser.RdgRecords.Station, Timetable.Location>(Cif.TestStations.Surbiton);
            
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void LocationMapTiploc()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output = mapper.Map<CifParser.RdgRecords.Station, Timetable.Location>(Cif.TestStations.Surbiton);
            
            Assert.Equal("SURBITN", output.Tiploc);
        }
        
        [Fact]
        public void LocationMapName()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output = mapper.Map<CifParser.RdgRecords.Station, Timetable.Location>(Cif.TestStations.Surbiton);
            
            Assert.Equal("SURBITON", output.Name);
        }
        
        [Fact]
        public void LocationMapCoordinates()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output = mapper.Map<CifParser.RdgRecords.Station, Timetable.Location>(Cif.TestStations.WaterlooWindsor);

            var expected = new Coordinates()
            {
                Eastings = 15312,
                Northings = 61798,
                IsEstimate = true
            };
            
            Assert.Equal(expected, output.Coordinates);
            Assert.Equal(expected.IsEstimate, output.Coordinates.IsEstimate);
        }
        
        [Fact]
        public void LocationMapInterchangeStatus()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output = mapper.Map<CifParser.RdgRecords.Station, Timetable.Location>(Cif.TestStations.Surbiton);
            
            Assert.Equal("Normal", output.InterchangeStatus.ToString());
        }

    }
}