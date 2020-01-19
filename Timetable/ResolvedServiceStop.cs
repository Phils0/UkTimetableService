using System;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceStop
    {
        public ResolvedService Service { get; }
        public ScheduleLocation Stop { get; }
        public ScheduleLocation FoundToStop { get; private set; } = null;
        public ScheduleLocation FoundFromStop { get; private set; } = null;

        public DateTime On => Service.On;

        public Toc Operator => Service.Details.Operator;
        
        public ResolvedServiceStop(ResolvedService service, ScheduleLocation stop)
        {
            Service = service;
            Stop = stop;
        }

        public override string ToString()
        {
            return $"{Service} {Stop}";
        }

        public bool IsNextDay(bool useDeparture)
        {
            if (useDeparture)
            {
                var departure = Stop as IDeparture;
                return departure?.IsNextDay ?? false;
            }
            
            var arrival = Stop as IArrival;
            return arrival?.IsNextDay ?? false;
        }
        
        public bool GoesTo(Station destination)
        {
            var departureTime = ((IDeparture) Stop).Time;
            foreach (var arrival in Service.Details.Locations.Where(l => l.HasAdvertisedTime(false)).OfType<IArrival>().Reverse())
            {
                // If arrival before Stop departure got to found stop so know that it does not go to location
                if (arrival.Time.IsBefore(departureTime))
                    return false;

                if (destination.Equals(arrival.Station))
                {
                    FoundToStop = (ScheduleLocation) arrival;
                    return true;
                }
            }

            return false;
        }

        public bool ComesFrom(Station origin)
        {
            var arrivalTime = ((IArrival) Stop).Time;
            foreach (var departure in Service.Details.Locations.Where(l => l.HasAdvertisedTime(true)).OfType<IDeparture>())
            {
                // If arrival at Stop before departure got to found stop so know that it does not come from location
                if (arrivalTime.IsBefore(departure.Time))
                    return false;

                if (origin.Equals(departure.Station))
                {
                    FoundFromStop = (ScheduleLocation) departure;
                    return true;
                }
            }

            return false;
        }
    }
}