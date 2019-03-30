namespace Timetable.Web.Test
{
    internal static class TestStations
    {
        internal static Station Surbiton
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Surbiton);
                return s;
            }
        }
        
        internal static Station Waterloo
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.WaterlooMain);
                s.Add(TestLocations.WaterlooWindsor);
                return s;
            }
        }
    }
}