using System.Linq;

namespace Timetable
{
    public class Data
    {
        public string Archive { get; set; }
        public ILocationData Locations { get; set; }
        public ITimetableLookup Timetable { get; set; }
        public ITocLookup Tocs { get; set; }
        
        public RealtimeData Darwin { get; set; }

        // Optional station-group reference data. Defaults to an empty lookup so the feature is simply
        // inert (group lookups return false) when the file is absent, rather than the property being null.
        public StationGroupLookup StationGroups { get; set; } = new (Enumerable.Empty<StationGroup>());

        public bool IsLoaded => (Locations?.IsLoaded ?? false)
            && (Timetable?.IsLoaded ?? false);
    }
}