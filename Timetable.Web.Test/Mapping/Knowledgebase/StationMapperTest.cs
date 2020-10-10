using NreKnowledgebase.SchemaV4;
using Timetable.Test.Data;
using Timetable.Web.Mapping.Knowledgebase;
using Xunit;

namespace Timetable.Web.Test.Mapping.Knowledgebase
{
    public class StationMapperTest
    {
        private readonly StationStructure _waterloo = Timetable.Web.Test.Knowledgebase.TestStations.Stations.Station[0];

        [Fact]
        public void MapNlc()
        {
            var station = TestStations.Waterloo;
            StationMapper.Update(station, _waterloo);
            Assert.Equal("559800", station.Nlc);
        }
        
        [Fact]
        public void MapName()
        {
            var station = TestStations.Waterloo;
            StationMapper.Update(station, _waterloo);
            Assert.Equal("Waterloo", station.Name);
        }
        
    }
}