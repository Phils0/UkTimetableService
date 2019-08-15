using System;

namespace Timetable
{
    public class StopSpecification
    {
        public Station Location { get; }
        public Time Time { get; }
        public DateTime OnDate { get; }
        public TimesToUse ArrivalOrDeparture { get; }

        public StopSpecification(Station location, Time time, DateTime onDate, TimesToUse arrivalOrDeparture)
        {
            Location = location;
            Time = time;
            OnDate = onDate;
            if(arrivalOrDeparture == TimesToUse.NotSet)
                throw new ArgumentException("Must be set to Arrival or Departure", nameof(arrivalOrDeparture));
            ArrivalOrDeparture = arrivalOrDeparture;
        }
        
        public bool UseDeparture => TimesToUse.Departures == ArrivalOrDeparture;
        
        public bool UseArrival => TimesToUse.Arrivals == ArrivalOrDeparture;

        public StopSpecification ChangeDate(DateTime newDate)
        {
            return new StopSpecification(Location, Time, newDate, ArrivalOrDeparture);
        }
        
        public StopSpecification MoveToPreviousDay()
        {
            return ChangeDate(OnDate.AddDays(-1));
        }
    }
}