using System;

namespace Timetable
{
    public enum TimesToUse
    {
        Arrivals,
        Departures
    }

    public class StopSpecification
    {
        public Station Location { get; }
        public Time Time { get; }
        public DateTime OnDate { get; }

        public StopSpecification(Station location, Time time, DateTime onDate, TimesToUse arrivalOrDeparture) :
            this(location, time, onDate, TimesToUse.Departures == arrivalOrDeparture)
        {
        }

        private StopSpecification(Station location, Time time, DateTime onDate, bool useDeparture)
        {
            Location = location;
            Time = time;
            OnDate = onDate;
            UseDeparture = useDeparture;
        }

        public bool UseDeparture { get; private set; }

        public bool UseArrival => !UseDeparture;

        private StopSpecification ChangeDate(DateTime newDate)
        {
            return new StopSpecification(Location, Time, newDate, UseDeparture);
        }

        public StopSpecification MoveToPreviousDay()
        {
            return ChangeDate(OnDate.AddDays(-1));
        }
    }
}