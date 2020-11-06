using DarwinClient.SchemaV16;
using Timetable.Web.Mapping.Darwin;
using Xunit;

namespace Timetable.Web.Test.Mapping.Darwin
{
    public class TocMapperTest
    {
        private readonly TocRef _avanti = Data.Darwin.Avanti;

        [Fact]
        public void MapCode()
        {
            var toc = TocMapper.Map(_avanti);
            Assert.Equal("VT", toc.Code);
        }
        
        [Fact]
        public void MapName()
        {
            var toc = TocMapper.Map(_avanti);
            Assert.Equal("Avanti West Coast", toc.Name);
        }
        
        [Fact]
        public void Url()
        {
            var toc = TocMapper.Map(_avanti);
            Assert.Equal(@"http://www.nationalrail.co.uk/tocs_maps/tocs/VT.aspx", toc.NationalRailUrl);
        }
    }
}