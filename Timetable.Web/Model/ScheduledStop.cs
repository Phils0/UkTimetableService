using System;
using System.Collections.Generic;

namespace Timetable.Web.Model
{
    public class ScheduledStop
    {
        public Timetable.Web.Model.ScheduleLocation Location { get; set; }

        public string Arrival { get; set; }

        public string Departure { get; set; }

        public string PassesAt { get; set; }

        public string Platform { get; set; }

        public string[] Activities { get; set; }
    }
}