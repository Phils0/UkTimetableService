using System.Linq;
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
        public void NlcIsMainNlc()
        {
            var waterloo = TestLocations.WaterlooMain;
            var station = new Station();
            station.Add(waterloo);

            Assert.Equal("5598", station.Nlc);
        }
        
        [Fact]
        public void NlcIsNullIfNoMain()
        {
            var waterloo = TestLocations.WaterlooWindsor;
            var station = new Station();
            station.Add(waterloo);

            Assert.Null(station.Nlc);
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
    }
}