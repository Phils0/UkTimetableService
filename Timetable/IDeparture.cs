namespace Timetable
{
    public interface IDeparture : IStop
    {
        bool IsPublic { get; }
        Time Time { get; }
        bool IsNextDay { get; }
        IService Service { get; }   
        Station Station { get; }
    }
}