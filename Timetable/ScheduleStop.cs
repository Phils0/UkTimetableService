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

        public override bool IsStopAt(StopSpecification spec)
        {
            
            return Station.Equals(spec.Location) && IsAt(spec);
        }

        private bool IsAt(StopSpecification spec)
        {
            return spec.UseDeparture ? IsDeparture(spec.Time) : IsArrival(spec.Time);
        }

        private bool IsArrival(Time time)
        {
            var arrival = (IArrival) this;
            return arrival.IsPublic && arrival.Time.IsSameTime(time);
        }

        private bool IsDeparture(Time time)
        {
            var departure = (IDeparture) this;
            return departure.IsPublic && departure.Time.IsSameTime(time);
        }
        
        public override string ToString()
        {
            var time = AdvertisedStop == PublicStop.PickUpOnly ? Departure : Arrival;
            return $"{time} {base.ToString()}";
        }
    }
}