namespace Timetable
{
    public interface IArrival
    {
        bool IsPublic { get; }
        Time Time { get; }
        Service Service { get; } 
        Station Station { get; }
    }
}