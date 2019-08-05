namespace Timetable
{
    public interface IDeparture 
    {
        Time Departure { get; }
        Time WorkingDeparture { get; }
        Service Service { get; }
    }
}