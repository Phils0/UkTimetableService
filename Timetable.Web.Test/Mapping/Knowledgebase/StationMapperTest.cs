using NreKnowledgebase.SchemaV4;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping.Knowledgebase;
using Xunit;

namespace Timetable.Web.Test.Mapping.Knowledgebase
{
    public class StationMapperTest
    {
        private readonly StationStructure _waterloo = Timetable.Web.Test.Knowledgebase.TestStations.Stations.Station[0];

        private readonly TocLookup _tocs = new TocLookup(Substitute.For<ILogger>());
        
        [Fact]
        public void MapNlc()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Equal("559800", station.Nlc);
        }
        
        [Fact]
        public void MapName()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Equal("Waterloo", station.Name);
        }
        
        [Fact]
        public void MapCoordinates()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Equal(new decimal(-0.113897), station.Coordinates.Longitude);
            Assert.Equal(new decimal(51.503507), station.Coordinates.Latitude);
        }
        
        [Fact]
        public void MapStationOperator()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Equal("NR", station.StationOperator.Code);
        }
        
        [Fact]
        public void MapWhenStationOperatorNotSet()
        {
            var station = TestStations.Vauxhall;
            StationStructure vauxhall = Timetable.Web.Test.Knowledgebase.TestStations.Stations.Station[1];
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, vauxhall);
            Assert.Equal(Toc.Unknown, station.StationOperator);
        }
    }
}