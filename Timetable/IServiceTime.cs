namespace Timetable
{
    public interface IServiceTime
    {
        Time Time { get; }
        IService Service { get; }        
    }
    
    public class ArrivalServiceTime : IServiceTime
    {
        public IArrival Arrival { get; }
        public Time Time => Arrival.Time;
        public IService Service => Arrival.Service;
        
        internal ArrivalServiceTime(IArrival arrival)
        {
            Arrival = arrival;
        }
    }
    
    public class DepartureServiceTime : IServiceTime
    {
        public IDeparture Departure { get; }
        public Time Time => Departure.Time;
        public IService Service => Departure.Service;
        
        internal DepartureServiceTime(IDeparture departure)
        {
            Departure = departure;
        }
    }

}