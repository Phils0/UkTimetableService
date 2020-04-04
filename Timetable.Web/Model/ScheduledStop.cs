using System;
using System.Collections.Generic;

namespace Timetable.Web.Model
{
    public class ScheduledStop
    {
        public LocationId Location { get; set; }

        public DateTime? Arrival { get; set; }

        public DateTime? Departure { get; set; }

        public DateTime? PassesAt { get; set; }

        public string Platform { get; set; }

        public string Activities { get; set; }
        
        public string AdvertisedStop { get; set; }
    }
}