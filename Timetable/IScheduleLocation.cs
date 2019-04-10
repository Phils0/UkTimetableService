namespace Timetable
{
    public interface IScheduleLocation
    {
        Location Location { get; set; }
        int Sequence { get; set; }
    }
}