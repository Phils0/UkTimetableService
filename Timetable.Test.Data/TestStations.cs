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
        
        public static Station Woking
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Woking);
                return s;
            }
        }
        
        public static Station Weybridge
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Weybridge);
                return s;
            }
        }
        
        public static Station Wimbledon
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Wimbledon);
                return s;
            }
        }
        
        public static Station Vauxhall
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Vauxhall);
                return s;
            }
        }
    }
}