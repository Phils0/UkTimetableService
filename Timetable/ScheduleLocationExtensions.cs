using System.Collections.Generic;

namespace Timetable
{
    public interface IArrival
    {
        Time Arrival { get; }
        Time WorkingArrival { get; }        
        Service Service { get; }
    }

    public interface IDeparture
    {
        Time Departure { get; }
        Time WorkingDeparture { get; }
        Service Service { get; }
    }
    
    public static class ScheduleLocationExtensions
    {
        public static Time GetTime(this IArrival x)
        {
            return x.Arrival.IsValid ? x.Arrival : x.WorkingArrival;
        }
        
        public static Time GetTime(this IDeparture x)
        {
            return x.Departure.IsValid ? x.Departure : x.WorkingDeparture;
        }
    }
}