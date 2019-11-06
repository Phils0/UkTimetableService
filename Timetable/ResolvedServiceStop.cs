using System;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceStop : ResolvedService
    {
        public ScheduleLocation Stop { get; }
        public ScheduleLocation FoundToStop { get; private set; } = null;
        public ScheduleLocation FoundFromStop { get; private set; } = null;

        public ResolvedServiceStop(ResolvedService service, ScheduleLocation stop)
            : this(service.Details, stop, service.On, service.IsCancelled, service.Associations)
        {
        }

        public ResolvedServiceStop(Schedule service, ScheduleLocation stop, DateTime on, bool isCancelled, ResolvedAssociation[] associations)
            : base(service, on, isCancelled, associations)
        {
            Stop = stop;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Stop}";
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
            foreach (var arrival in Details.Locations.OfType<IArrival>().Where(a => a.IsPublic).Reverse())
            {
                // Check if got to Stop, if so can shortcut as it doesn't go there although this is slightly dodgy
                if (Stop.Station.Equals(arrival.Station))
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
            foreach (var departure in Details.Locations.OfType<IDeparture>().Where(a => a.IsPublic))
            {
                // Check if got to Stop, if so can shortcut as it doesn't go there although this is slightly dodgy
                if (Stop.Station.Equals(departure.Station))
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