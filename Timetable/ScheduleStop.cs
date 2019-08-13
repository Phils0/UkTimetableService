using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleStop : ScheduleLocation, IArrival, IDeparture
    {
        bool IArrival.IsPublic => Arrival.IsValid;
        
        Time IArrival.Time => ((IArrival) this).IsPublic ? Arrival : WorkingArrival;

        public Time Arrival { get; set; }

        public Time WorkingArrival { get; set; }

        bool IDeparture.IsPublic => Departure.IsValid;
        
        Time IDeparture.Time => ((IDeparture) this).IsPublic ? Departure : WorkingDeparture;

        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }

        public override void AddDay(Time start)
        {
            Arrival = Arrival.MakeAfterByAddingADay(start);
            WorkingArrival = WorkingArrival.MakeAfterByAddingADay(start);
            Departure = Departure.MakeAfterByAddingADay(start);
            WorkingDeparture = WorkingDeparture.MakeAfterByAddingADay(start);
        }

        public override bool IsStopAt(Station location, Time time)
        {
            return Station.Equals(location) && IsAt(time);
        }

        private bool IsAt(Time time)
        {
            var arrival = (IArrival) this;
            var departure = (IDeparture) this;
            return arrival.IsPublic && arrival.Time.Equals(time) || 
                   departure.IsPublic && departure.Time.Equals(time);
        }

        public override string ToString()
        {
            var time = AdvertisedStop == PublicStop.PickUpOnly ? Departure : Arrival;
            return $"{time} {base.ToString()}";
        }
    }
}