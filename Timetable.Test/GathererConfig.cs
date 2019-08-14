using Timetable;

namespace Timetable.Test
{
    static internal class GathererConfig
    {
        internal static GatherConfiguration OneService => Create(0, 1);
        
        internal static GatherConfiguration OneBefore => Create(1, 0);
        
        internal static GatherConfiguration OneBeforeTwoAfter => Create(1, 2);

        internal static GatherConfiguration OneDepartureService => Create(0, 1, TimesToUse.Departures);
        
        internal static GatherConfiguration OneDepartureBefore => Create(1, 0, TimesToUse.Departures);
        
        internal static GatherConfiguration OneBeforeTwoAfterDeparture => Create(1, 2, TimesToUse.Departures);

        internal static GatherConfiguration Create(int before, int after, TimesToUse arrivalsOrDepartures = TimesToUse.Arrivals)
        {
            return new GatherConfiguration(before, after, GatherFilterFactory.NoFilter, arrivalsOrDepartures);
        }
    }
}