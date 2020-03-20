using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable.Test.Data
{
    public static class TestScheduleLocations
    {
        public static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        public static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);     

        public static ScheduleStop CreateOrigin(Station location, Time departure)
        {
            return CreateOrigin(location.Locations.First(), departure);
        }
        
        public static ScheduleStop CreateOrigin(Location location, Time departure)
        {
            var origin = CreatePickupOnlyStop(location, departure);
            origin.Platform = "1";
            origin.Activities = CreateActivities("TB");
            origin.WorkingArrival = Time.NotValid;
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
        
        public static ScheduleStop CreateStop(Station location, Time arrival, string activity = "T", int sequence = 1)
        {
            return CreateStop(location.Locations.First(), arrival, activity, sequence);
        }
        
        public static ScheduleStop CreateStop(Location location, Time arrival, string activity = "T" , int sequence = 1)
        {
            var stop = new ScheduleStop()
            {
                Location = location,
                Sequence = sequence,
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
        
        public static ScheduleStop CreatePickupOnlyStop(Station location, Time departure)
        {
            return CreatePickupOnlyStop(location.Locations.First(), departure);
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
        
        public static ScheduleStop CreateSetdownOnlyStop(Station location, Time arrival)
        {
            return CreateSetdownOnlyStop(location.Locations.First(), arrival);
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
        
        public static SchedulePass CreatePass(Station location, Time pass)
        {
            return CreatePass(location.Locations.First(), pass);
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
        
        public static ScheduleStop CreateDestination(Station location, Time arrival)
        {
            return CreateDestination(location.Locations.First(), arrival);
        }
        
        public static ScheduleStop CreateDestination(Location location, Time arrival)
        {
            var destination = CreateSetdownOnlyStop(location, arrival);
            destination.Platform = "2";
            destination.Activities = CreateActivities("TF");
            destination.WorkingDeparture = Time.NotValid;
            destination.UpdateAdvertisedStop();
            return destination;
        }
    }
}