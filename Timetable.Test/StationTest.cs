using System.Linq;
using NSubstitute;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class StationTest
    {
        [Fact]
        public void SetMainWhenNotSubsidiaryLocation()
        {
            var waterloo = TestLocations.WaterlooMain;
            var station = new Station();
            station.Add(waterloo);

            Assert.Equal(waterloo, station.Main);
        }

        [Fact]
        public void DoNotSetMainWhenSubsidiaryLocation()
        {
            var waterloo = TestLocations.WaterlooWindsor;
            var station = new Station();
            station.Add(waterloo);

            Assert.Equal(Location.NotSet, station.Main);
        }

        [Fact]
        public void SetMainTwiceLastOneWins()
        {
            var waterloo = TestLocations.WaterlooMain;
            var surbiton = TestLocations.Surbiton;
            var station = new Station();
            station.Add(waterloo);
            station.Add(surbiton);

            Assert.Equal(surbiton, station.Main);
        }

        [Fact]
        public void SetMainTwiceIsLogged()
        {
            using (var log = new LogHelper())
            {
                Log.Logger = log.Logger;

                using (TestCorrelator.CreateContext())
                {
                    var waterloo = TestLocations.WaterlooMain;
                    var surbiton = TestLocations.Surbiton;
                    var station = new Station();
                    station.Add(waterloo);
                    station.Add(surbiton);

                    var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().Single();
                    var message = logEvent.RenderMessage();
                    Assert.Equal(LogEventLevel.Warning, logEvent.Level);
                    Assert.Contains("Overriding main location \"WAT-WATRLMN\"", message);
                }
            }
        }

        [Fact]
        public void CodeIsMainCode()
        {
            var waterloo = TestLocations.WaterlooMain;
            var station = new Station();
            station.Add(waterloo);

            Assert.Equal(waterloo.ThreeLetterCode, station.ThreeLetterCode);
        }

        [Fact]
        public void NlcIsNullByDefault()
        {
            var waterloo = TestLocations.WaterlooMain;
            var station = new Station();
            station.Add(waterloo);

            Assert.Null(station.Nlc);
        }
        
        [Fact]
        public void NlcIsManuallySet()
        {
            var station = new Station();
            station.Nlc = "123456";
            var waterloo = TestLocations.WaterlooMain;
            station.Add(waterloo);

            Assert.Equal("123456", station.Nlc);
        }
        
        [Fact]
        public void NameIsNullByDefault()
        {
            var waterloo = TestLocations.WaterlooMain;
            var station = new Station();
            station.Add(waterloo);

            Assert.Null(station.Name);
        }
        
        [Fact]
        public void NameIsManuallySet()
        {
            var station = new Station();
            station.Name = "MyName";
            var waterloo = TestLocations.WaterlooMain;
            station.Add(waterloo);

            Assert.Equal("MyName", station.Name);
        }
        
        [Fact]
        public void ToStringWhenNoMainReturnsNotSet()
        {
            var station = new Station();
            Assert.Equal("Not Set", station.ToString());
        }

        [Fact]
        public void ToStringReturnsCode()
        {
            var station = new Station();
            station.Add(TestLocations.WaterlooMain);

            Assert.Equal("WAT", station.ToString());
        }

        public static TheoryData<Station, bool> StationEquality =>
            new TheoryData<Station, bool>()
            {
                {TestStations.Surbiton, false},
                {TestStations.Waterloo, true},
                {WaterlooWindsor, true},
                {null, false}
            };

        private static Station WaterlooWindsor
        {
            get
            {
                var main = new Location()
                {
                    Tiploc = "WATRLOW",
                    ThreeLetterCode = "WAT",
                    Nlc = "559803",
                    Name = "LONDON WATERLOO",
                    InterchangeStatus = InterchangeStatus.Main,
                    Coordinates = new Coordinates()
                    {
                        Eastings = 15312,
                        Northings = 61798,
                        IsEstimate = true
                    }
                };

                var s = new Station();
                s.Add(main);
                return s;
            }
        }

        [Theory]
        [MemberData(nameof(StationEquality))]
        public void EqualityUsesThreeLetterCode(Station other, bool expected)
        {
            var test = TestStations.Waterloo;
            Assert.Equal(expected, test.Equals(other));
        }

        [Fact]
        public void AddScheduleLocationSetsToc()
        {
            var test = TestStations.Waterloo;
            var stop = CreateStop("X12345");
            
            test.Add(stop);
            
            Assert.NotEmpty(test.TocServices);
            Assert.Equal(TestSchedules.VirginTrains, test.TocServices.First());
        }
        
        private static ScheduleStop CreateStop(string timetableUid)
        {
            var service = new Service(timetableUid, Substitute.For<ILogger>());
            var schedule = TestSchedules.CreateSchedule(timetableId: timetableUid, service: service);
            var stop = schedule.Destination;
            return stop;
        }

        [Fact]
        public void DoesNotAddTocIfDoesNotStop()
        {
            var test = TestStations.Waterloo;
            var service = new Service( "X12345", Substitute.For<ILogger>());
            var schedule = TestSchedules.CreateSchedule(timetableId: "X12345", service: service, stops: TestSchedules.CreateThreeStopSchedule(TestSchedules.Ten));
            var stop = schedule.Locations[2];    // Pass
            
            test.Add(stop);
            
            Assert.Empty(test.TocServices);
        }
        
        [Fact]
        public void AddScheduleLocationAddsTocIfNotAlreadyAdded()
        {
            var test = TestStations.Waterloo;
            var stop = CreateStop("X12345");

            test.Add(stop);
            
            var service = new Service("X98765", Substitute.For<ILogger>());
            var schedule = TestSchedules.CreateSchedule("X98765", retailServiceId: "SW123400");
            var stop2 = schedule.Destination;
            test.Add(stop2);
            
            Assert.Equal(2, test.TocServices.Count);
        }
        
        [Fact]
        public void AddScheduleLocationDoesNotAddsTocAgain()
        {
            var test = TestStations.Waterloo;
            var stop = CreateStop("X12345");

            test.Add(stop);
            
            var stop2 = CreateStop("X98765");
            test.Add(stop2);
            
            Assert.Single(test.TocServices);
        }
    }
}