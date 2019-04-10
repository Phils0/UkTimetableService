using System;
using System.Collections.Generic;

namespace Timetable
{
    public class ScheduleOrigin : IScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public Time Departure { get; set; }

        public Time WorkingDeparture { get; set; }

        public string Platform { get; set; }

        public ISet<string> Activities { get; set; }
    }
}