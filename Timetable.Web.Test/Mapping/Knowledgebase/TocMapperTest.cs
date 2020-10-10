using NreKnowledgebase;
using NreKnowledgebase.SchemaV4;
using NSubstitute;
using Serilog;
using Timetable.Web.Mapping.Knowledgebase;
using Timetable.Web.Test.Knowledgebase;
using Xunit;

namespace Timetable.Web.Test.Mapping.Knowledgebase
{
    public class TocMapperTest
    {
        private readonly TrainOperatingCompanyStructure _avanti = TestTocs.Tocs.TrainOperatingCompany[0];

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
        
    }
}