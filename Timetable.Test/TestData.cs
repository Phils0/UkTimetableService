using System.Collections.Generic;

namespace Timetable.Test
{
    internal static class TestData
    {
        internal static readonly Data Instance = new Data()
        {
            Locations = new Dictionary<string, Station>()
            {
                {"SUR", TestStations.Surbiton},
                {"WAT", TestStations.Waterloo}
            }
        };
    }
}