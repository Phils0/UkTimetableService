namespace Timetable
{
    public interface IServiceTime
    {
        bool IsPublic { get; }
        Time Time { get; }
        Service Service { get; }        
    }
    
    public class ArrivalServiceTime : IServiceTime
    {
        public bool IsPublic { get; }
        public Time Time { get; }
        public Service Service { get; }
        
        internal ArrivalServiceTime(IArrival a)
        {
            IsPublic = a.Arrival.IsValid;
            Time =  IsPublic ? a.Arrival : a.WorkingArrival;
            Service = a.Service;
        }
    }
    
    public class DepartureServiceTime : IServiceTime
    {
        public bool IsPublic { get; }
        public Time Time { get; }
        public Service Service { get; }
        
        internal DepartureServiceTime(IDeparture a)
        {
            IsPublic = a.Departure.IsValid;
            Time =  IsPublic ? a.Departure : a.WorkingDeparture;
            Service = a.Service;
        }
    }

}