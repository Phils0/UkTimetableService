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

        private static readonly Time First = new Time(new TimeSpan(0, 7, 0));
        
        public static ILocationData CreateTimetabledLocations(int duration = 1440)
        {
            var data = Locations;
            var surbiton = data.LocationsByTiploc["SURBITN"];
            var waterloo = data.LocationsByTiploc["WATRLMN"];
            var clapham = data.LocationsByTiploc["CLPHMJN"];


            for (int i = 0; i < duration; i += 15)
            {
                var start = First.AddMinutes(i);
                var stops = new[]
                {
                    (ScheduleLocation) TestScheduleLocations.CreateOrigin(surbiton, start),
                    TestScheduleLocations.CreateStop(clapham, start.AddMinutes(5)),
                    TestScheduleLocations.CreateDestination(waterloo, start.AddMinutes(10))
                };

                var uid = $"X{i:D5}";
                var testSchedule = TestSchedules.CreateScheduleWithService(timetableId: uid,  stops: stops);
            }

            return data;
        }
    }
}