using System;
using System.Collections.Generic;

namespace Timetable.Test.Data
{
    public static class TestScheduleLocations
    {
        public static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        public static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);     

        public static ScheduleOrigin CreateOrigin(Location location, Time departure)
        {
            var origin = new ScheduleOrigin()
            {
                Location = location,
                Sequence = 1,
                Departure = departure,
                WorkingDeparture = departure.Add(ThirtySeconds),
                Platform = "1",
                Activities = CreateActivities("TB")
            };
            origin.UpdateAdvertisedStop();
            return origin;
        }

        private static ISet<string> CreateActivities(string activity)
        {
            return CreateActivities(new[] {activity});
        }
        
        private static ISet<string> CreateActivities(string[] activities)
        {
            return new HashSet<string>(activities);
        }
        
        public static ScheduleStop CreateStop(Location location, Time arrival, string activity = "T")
        {
            var stop = new ScheduleStop()
            {
                Location = location,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival.Subtract(ThirtySeconds),
                Departure = arrival.Add(OneMinute),
                WorkingDeparture = arrival.Add(ThirtySeconds),
                Platform = "10",
                Activities = CreateActivities(activity)
            };
            stop.UpdateAdvertisedStop();
            return stop;
        }
        
        public static ScheduleStop CreatePickupOnlyStop(Location location, Time departure)
        {
            var stop = new ScheduleStop()
            {
                Location = location,
                Sequence = 1,
                Arrival = Time.NotValid,
                WorkingArrival = departure.Subtract(ThirtySeconds),
                Departure = departure,
                WorkingDeparture = departure.Add(ThirtySeconds),
                Platform = "10",
                Activities = CreateActivities("U")
            };
            stop.UpdateAdvertisedStop();
            return stop;
        }
        
        public static ScheduleStop CreateSetdownOnlyStop(Location location, Time arrival)
        {
            var stop = new ScheduleStop()
            {
                Location = location,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival.Subtract(ThirtySeconds),
                Departure = Time.NotValid,
                WorkingDeparture = arrival.Add(ThirtySeconds),
                Platform = "10",
                Activities = CreateActivities("D")
            };
            stop.UpdateAdvertisedStop();
            return stop;
        }
        public static SchedulePass CreatePass(Location location, Time pass)
        {
            var schedulePass = new SchedulePass()
            {
                Location = location,
                Sequence = 1,
                PassesAt = pass,
                Platform = "",
                Activities = new HashSet<string>()
            };
            schedulePass.UpdateAdvertisedStop();
            return schedulePass;
        }
        
        public static ScheduleDestination CreateDestination(Location location, Time arrival)
        {
            var destination = new ScheduleDestination()
            {
                Location = location,
                Sequence = 1,
                Arrival = arrival,
                WorkingArrival = arrival.Subtract(ThirtySeconds),
                Platform = "2",
                Activities = CreateActivities("TF")
            };
            destination.UpdateAdvertisedStop();
            return destination;
        }
    }
}