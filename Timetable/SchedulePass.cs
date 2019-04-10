using System;
using System.Collections.Generic;

namespace Timetable
{
    public class SchedulePass : IScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public Time PassesAt { get; set; }

        public string Platform { get; set; }

        public ISet<string> Activities { get; set; }
    }
}