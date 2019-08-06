namespace Timetable
{
    public interface IServiceTime
    {
        Time Time { get; }
        Service Service { get; }        
    }
    
    public class ArrivalServiceTime : IServiceTime
    {
        public IArrival Arrival { get; }
        public Time Time => Arrival.Time;
        public Service Service => Arrival.Service;
        
        internal ArrivalServiceTime(IArrival arrival)
        {
            Arrival = arrival;
        }
    }
    
    public class DepartureServiceTime : IServiceTime
    {
        public IDeparture Departure { get; }
        public Time Time => Departure.Time;
        public Service Service => Departure.Service;
        
        internal DepartureServiceTime(IDeparture departure)
        {
            Departure = departure;
        }
    }

}