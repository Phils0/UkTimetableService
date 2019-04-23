using System;
using System.Collections.Generic;

namespace Timetable
{
    public class SchedulePass : ScheduleLocation
    {
        public Time PassesAt { get; set; }

        public override void AddDay(Time start)
        {
            PassesAt = PassesAt.MakeAfterByAddingADay(start);
        }
        
        public override string ToString()
        {
            return $"{PassesAt} {base.ToString()}";
        }
    }
}