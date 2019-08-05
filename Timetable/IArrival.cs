namespace Timetable
{
    public interface IArrival
    {
        Time Arrival { get; }
        Time WorkingArrival { get; }
        Service Service { get; }
    }
}