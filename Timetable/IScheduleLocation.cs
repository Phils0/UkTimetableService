using System.Collections.Generic;

namespace Timetable
{
    public interface IScheduleLocation
    {
        Location Location { get; set; }
        int Sequence { get; set; }
        void AddDay(Time start);
    }
    
    public abstract class ScheduleLocation : IScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public string Platform { get; set; }

        public ISet<string> Activities { get; set; }
        
        public abstract void AddDay(Time start);
    }
}