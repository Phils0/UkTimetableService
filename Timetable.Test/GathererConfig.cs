using Timetable;

namespace Timetable.Test
{
    static internal class GathererConfig
    {
        internal static GatherConfiguration OneService => Create(0, 1);
        internal static GatherConfiguration OneBefore => Create(1, 0);
        internal static GatherConfiguration OneBeforeTwoAfter => Create(1, 2);
        
        internal static GatherConfiguration Create(ushort before, ushort after)
        {
            return new GatherConfiguration(before, after, GatherFilterFactory.NoFilter);
        }
    }
}