namespace Timetable
{
    public interface IDeparture
    {
        bool IsPublic { get; }
        Time Time { get; }
        bool IsNextDay { get; }
        Service Service { get; }   
        Station Station { get; }
    }
}