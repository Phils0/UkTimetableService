using System;
using System.Collections.Generic;

namespace Timetable.Test.Data
{
    public static class TestScheduleLocations
    {
        internal static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        internal static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);     

        public static ScheduleOrigin CreateOrigin(Location location, Time departure)
        {
            return new ScheduleOrigin()
            {
                Location = location,
                Sequence = 1,
                Departure = departure,
                WorkingDeparture = departure.Add(ThirtySeconds),
                Platform = "1",
                Activities = CreateActivities("TB")
            };
        }

        private static ISet<string> CreateActivities(string activity)
        {
            return CreateActivities(new[] {activity});
        }
        
        private static ISet<string> CreateActivities(string[] activities)
        {
            return new HashSet<string>(activities);
        }
        
        public static ScheduleStop CreateStop(Location location, Time arrival)
        {
            return new ScheduleStop()
            {
                Location = location,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival.Subtract(ThirtySeconds),
                Departure = arrival.Add(OneMinute),
                WorkingDeparture = arrival.Add(ThirtySeconds),
                Platform = "10",
                Activities = CreateActivities("T")
            };
        }
        
        public static SchedulePass CreatePass(Location location, Time pass)
        {
            return new SchedulePass()
            {
                Location = location,
                Sequence = 1,
                PassesAt = pass,
                Platform = "",
                Activities = new HashSet<string>()
            };
        }
        
        public static ScheduleDestination CreateDestination(Location location, Time arrival)
        {
            return new ScheduleDestination()
            {
                Location = location,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival.Subtract(ThirtySeconds),
                Platform = "2",
                Activities = CreateActivities("TF")
            };
        }
    }
}