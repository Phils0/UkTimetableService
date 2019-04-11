namespace Timetable.Test.Data
{
    public static class TestStations
    {
        public static Station Surbiton
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Surbiton);
                return s;
            }
        }
        
        public static Station Waterloo
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.WaterlooMain);
                s.Add(TestLocations.WaterlooWindsor);
                return s;
            }
        }
        
        public static Station ClaphamJunction
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.CLPHMJN);
                s.Add(TestLocations.CLPHMJC);
                return s;
            }
        }
    }
}