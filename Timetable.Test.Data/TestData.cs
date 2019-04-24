using System;
using NSubstitute;
using NSubstitute.Core.Arguments;
using Serilog;

namespace Timetable.Test.Data
{
    public static class TestData
    {
        public static ILocationData Locations => new Timetable.LocationData(
            new[]
            {
                TestLocations.Surbiton,
                TestLocations.WaterlooMain,
                TestLocations.WaterlooWindsor,
                TestLocations.CLPHMJN,
                TestLocations.CLPHMJC
            }, Substitute.For<ILogger>());
    }
}