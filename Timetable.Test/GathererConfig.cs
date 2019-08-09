using Timetable;

namespace Timetable.Test
{
    static internal class GathererConfig
    {
        public static GatherConfiguration OneService => new GatherConfiguration(0, 1);
        
        public static GatherConfiguration OneBefore => new GatherConfiguration(1, 0);
        
        public static GatherConfiguration OneBeforeTwoAfter => new GatherConfiguration(1, 2);

    }
}