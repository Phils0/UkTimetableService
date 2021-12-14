using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class FromCifProfileTiplocTest
    {
        private readonly MapperConfiguration _fromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            _fromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapTiploc()
        {
            var output = MapCifTiploc();
            Assert.Equal("SURBITN", output.Tiploc);
        }
        
        public Location MapCifTiploc(CifParser.Records.TiplocInsertAmend input = null, IMapper mapper = null)
        {
            input = input ?? Test.Cif.TestCifLocations.Surbiton;
            mapper = mapper ?? _fromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.TiplocInsertAmend, Timetable.Location>(input, o =>
            {
                o.Items.Add("Locations", TestData.Locations);
            });
        }
        
        [Fact]
        public void MapCrs()
        {
            var output = MapCifTiploc();
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void MapName()
        {
            var output = MapCifTiploc();
            Assert.Equal("SURBITON", output.Name);
        }
        
        [Fact]
        public void MapNlc()
        {
            var output = MapCifTiploc();
            Assert.Equal("557100", output.Nlc);
        }
        
        [Fact]
        public void IsActiveIsFalse()
        {
            var output = MapCifTiploc();
            Assert.False(output.IsActive);
        }
        
        [Fact]
        public void OtherPropertiesNotMapped()
        {
            var output = MapCifTiploc();
            Assert.Equal(InterchangeStatus.NotSet, output.InterchangeStatus);
            Assert.Null(output.Coordinates);
            Assert.Null(output.Station);
        }
        
        [Fact]
        public void CrsNotMapped()
        {
            var tiploc = Test.Cif.TestCifLocations.Surbiton;
            tiploc.ThreeLetterCode = "";
            
            var output = MapCifTiploc(tiploc);
            Assert.Empty(output.ThreeLetterCode);
        }
    }
}