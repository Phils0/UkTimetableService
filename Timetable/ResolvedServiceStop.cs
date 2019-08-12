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
            : this(service.Details, stop, service.On, service.IsCancelled)
        {
        }

        public ResolvedServiceStop(Schedule service, ScheduleLocation stop, DateTime on, bool isCancelled)
            : base(service, on, isCancelled)
        {
            Stop = stop;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Stop}";
        }

        public bool GoesTo(Station destination)
        {
            foreach (var stop in Details.Locations.Reverse())
            {
                // Check if got to Stop, if so can shortcut as it doesn't go there although this is slightly dodgy
                if (Stop.Station.Equals(stop.Station))
                    return false;

                if (destination.Equals(stop.Station))
                {
                    FoundToStop = stop;
                    return true;
                }
            }

            return false;
        }
        
        public bool ComesFrom(Station origin)
        {
            foreach (var stop in Details.Locations)
            {
                // Check if got to Stop, if so can shortcut as it doesn't go there although this is slightly dodgy
                if (Stop.Station.Equals(stop.Station))
                    return false;

                if (origin.Equals(stop.Station))
                {
                    FoundFromStop = stop;
                    return true;
                }
            }

            return false;        
        }
    }
}