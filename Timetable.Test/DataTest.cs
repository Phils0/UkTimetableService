using System.Collections.Generic;
using NSubstitute;
using Serilog;
using Xunit;

namespace Timetable.Test
{
    public class DataTest
    {
        public static TheoryData<ILocationData, ITimetableLookup, bool> HasDataTests =>
            new TheoryData<ILocationData, ITimetableLookup, bool>()
            {
                {LoadedLocations(true), LoadedTimetable(true), true},
                {LoadedLocations(false), LoadedTimetable(true), false},
                {LoadedLocations(true), LoadedTimetable(false), false},
                {null, LoadedTimetable(true), false},
                {LoadedLocations(true), null, false},
                {null, null, false},
            };

        private static ILocationData LoadedLocations(bool isLoaded) =>
            new LocationData( Substitute.For<ICollection<Location>>(), Substitute.For<ILogger>()) { IsLoaded = isLoaded};
        
        private static ITimetableLookup LoadedTimetable(bool isLoaded) =>
            new TimetableData(Substitute.For<ILogger>()) { IsLoaded = isLoaded };
        
        [Theory]
        [MemberData(nameof(HasDataTests))]
        public void IsLoadedSet(ILocationData locations, ITimetableLookup timetable, bool expeected)
        {
            var data = new Timetable.Data();
            data.Locations = locations;
            data.Timetable = timetable;
            Assert.Equal(expeected, data.IsLoaded);
        }
    }
}