using DarwinClient.SchemaV16;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping.Darwin;
using Xunit;

namespace Timetable.Web.Test.Mapping.Darwin
{
    public class StationMapperTest
    {
        private readonly LocationRef _waterloo = Test.Data.Darwin.Waterloo;

        private readonly TocLookup _tocs = new TocLookup(Substitute.For<ILogger>());
        
        [Fact]
        public void NlcNotUpdated()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Null(station.Nlc);
        }
        
        [Fact]
        public void MapName()
        {
            var station = TestStations.Waterloo;
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, _waterloo);
            Assert.Equal("London Waterloo", station.Name);
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
            LocationRef vauxhall = Test.Data.Darwin.Vauxhall;
            vauxhall.toc = "";
            var mapper = new StationMapper(_tocs);
            mapper.Update(station, vauxhall);
            Assert.Equal(Toc.Unknown, station.StationOperator);
        }
    }
}