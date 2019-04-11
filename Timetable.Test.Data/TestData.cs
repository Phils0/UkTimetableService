using NSubstitute;
using Serilog;

namespace Timetable.Test.Data
{
    public static class TestData
    {
        public static Timetable.ILocationData Instance => new Timetable.LocationData(
            new []
            {
                TestLocations.Surbiton,
                TestLocations.WaterlooMain,
                TestLocations.WaterlooWindsor,
                TestLocations.CLPHMJN,
                TestLocations.CLPHMJC
            }, Substitute.For<ILogger>());
    }
}