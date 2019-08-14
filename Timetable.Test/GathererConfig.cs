using Timetable;

namespace Timetable.Test
{
    static internal class GathererConfig
    {
        internal static GatherConfiguration OneService => Create(0, 1);
        
        internal static GatherConfiguration OneBefore => Create(1, 0);
        
        internal static GatherConfiguration OneBeforeTwoAfter => Create(1, 2);

        internal static GatherConfiguration Create(int before, int after, TimesToUse arrivalsOrDepartures = TimesToUse.Departures)
        {
            return new GatherConfiguration(before, after, GatherFilterFactory.NoFilter, arrivalsOrDepartures);
        }
    }
}