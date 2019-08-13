namespace Timetable
{
    public interface IDeparture
    {
        bool IsPublic { get; }
        Time Time { get; }
        Service Service { get; }   
        Station Station { get; }
    }
}