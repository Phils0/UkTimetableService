namespace Timetable
{
    public interface IDeparture : IStop
    {
        bool IsPublic { get; }
        Time Time { get; }
        bool IsNextDay { get; }
        Service Service { get; }   
        Station Station { get; }
    }
}