using System.Linq;

namespace Timetable
{
    public class Data
    {
        public string Archive { get; set; }
        public ILocationData Locations { get; set; }
        public ITimetable Timetable { get; set; }
        public ITocLookup Tocs { get; set; }
    }
}