using System.Collections.Generic;
using System.Linq;
using DarwinClient.SchemaV16;
using NSubstitute;
using ReflectionMagic;
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

        private static readonly Location[] Locations = new[]
             {
                 TestLocations.Surbiton,
                 TestLocations.WaterlooMain,
                 TestLocations.WaterlooWindsor,
                 TestLocations.CLPHMJN,
                 TestLocations.CLPHMJC,
                 TestLocations.Wimbledon
             };
        
        private StationMapper CreateMapper(LocationData stations = null)
        {
            stations =  stations ?? new LocationData(Locations, Substitute.For<ILogger>());
            return new StationMapper(_tocs, stations, Substitute.For<ILogger>());
        }
        
        [Fact]
        public void NlcNotUpdated()
        {
            var station = TestStations.Waterloo;
            var mapper = CreateMapper();
            mapper.Update(station, _waterloo);
            Assert.Null(station.Nlc);
        }
        
        [Fact]
        public void MapName()
        {
            var station = TestStations.Waterloo;
            var mapper = CreateMapper();
            mapper.Update(station, _waterloo);
            Assert.Equal("London Waterloo", station.Name);
        }
        
        [Fact]
        public void MapStationOperator()
        {
            var station = TestStations.Waterloo;
            var mapper = CreateMapper();
            mapper.Update(station, _waterloo);
            Assert.Equal("NR", station.StationOperator.Code);
        }
        
        [Fact]
        public void MapWhenStationOperatorNotSet()
        {
            var station = TestStations.Vauxhall;
            LocationRef vauxhall = Test.Data.Darwin.Vauxhall;
            vauxhall.toc = "";
            var mapper = CreateMapper();
            mapper.Update(station, vauxhall);
            Assert.Equal(Toc.Unknown, station.StationOperator);
        }

        private Via CreateViaRule()
        {
            return new Via()
            {
                at = TestLocations.Surbiton.ThreeLetterCode,
                dest = TestLocations.WaterlooMain.Tiploc,
                loc1 = TestLocations.Wimbledon.Tiploc,
                loc2 = TestLocations.CLPHMJN.Tiploc,
                viatext = "via Test"
            };
        }
        
        [Fact]
        public void AddViaRule()
        {
            var stations = new LocationData(Locations, Substitute.For<ILogger>());
            var mapper = CreateMapper(stations);
            var rule = CreateViaRule();

            var surbiton = stations.Locations["SUR"];
            mapper.AddRule(surbiton, rule);

            var rules = GetViaRules(surbiton);
            var targetrule = rules[TestLocations.WaterlooMain][0];
            Assert.NotNull(targetrule);
            Assert.Equal(surbiton, targetrule.At);
            Assert.Equal(TestLocations.WaterlooMain, targetrule.Destination);
            Assert.Equal(TestLocations.Wimbledon, targetrule.Location1);
            Assert.Equal(TestLocations.CLPHMJN, targetrule.Location2);
            Assert.Equal("via Test", targetrule.Text);
        }

        public static Dictionary<Location, List<ViaRule>> GetViaRules(Station station)
        {
            return station.ViaTextRules.AsDynamic()._rules.RealObject as Dictionary<Location, List<ViaRule>>;
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddViaRuleWithNoLocation2(string loc2)
        {
            var stations = new LocationData(Locations, Substitute.For<ILogger>());
            var mapper = CreateMapper(stations);
            var rule = CreateViaRule();
            rule.loc2 = loc2;

            var surbiton = stations.Locations["SUR"];
            mapper.AddRule(surbiton, rule);

            var rules = GetViaRules(surbiton);
            var targetrule = rules[TestLocations.WaterlooMain][0];
            Assert.NotNull(targetrule);
            Assert.Equal(surbiton, targetrule.At);
            Assert.Equal(TestLocations.WaterlooMain, targetrule.Destination);
            Assert.Equal(TestLocations.Wimbledon, targetrule.Location1);
            Assert.Equal(Location.NotSet, targetrule.Location2);
            Assert.Equal("via Test", targetrule.Text);
        }
        
        [Fact]
        public void DoesNotAddIfCannotFindDestination()
        {
            var stations = new LocationData(Locations, Substitute.For<ILogger>());
            var mapper = CreateMapper(stations);
            var rule = CreateViaRule();
            rule.dest = TestLocations.Woking.Tiploc;

            var surbiton = stations.Locations["SUR"];
            mapper.AddRule(surbiton, rule);

            var rules = GetViaRules(surbiton);
            Assert.Empty(rules);
        }
        
        [Fact]
        public void DoesNotAddIfCannotFindLocation1()
        {
            var stations = new LocationData(Locations, Substitute.For<ILogger>());
            var mapper = CreateMapper(stations);
            var rule = CreateViaRule();
            rule.loc1 = TestLocations.Woking.Tiploc;

            var surbiton = stations.Locations["SUR"];
            mapper.AddRule(surbiton, rule);

            var rules = GetViaRules(surbiton);
            Assert.Empty(rules);
        }
        
        [Fact]
        public void DoesNotAddIfCannotFindLocation2()
        {
            var stations = new LocationData(Locations, Substitute.For<ILogger>());
            var mapper = CreateMapper(stations);
            var rule = CreateViaRule();
            rule.loc2 = TestLocations.Woking.Tiploc;

            var surbiton = stations.Locations["SUR"];
            mapper.AddRule(surbiton, rule);

            var rules = GetViaRules(surbiton);
            Assert.Empty(rules);
        }
    }
}