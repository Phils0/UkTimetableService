using System;

namespace Timetable
{
    public class ResolvedStop
    {
        public ScheduleLocation Stop { get; }
        
        public DateTime On { get; }

        public ResolvedStop(ScheduleLocation stop, DateTime on)
        {
            Stop = stop;
            On = on;
        }
        
        public override string ToString()
        {
            return $"{On.ToYMD()} {Stop}";
        }
    }
}