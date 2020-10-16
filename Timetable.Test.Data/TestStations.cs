using System.Linq.Expressions;

namespace Timetable.Test.Data
{
    public static class TestStations
    {
        private readonly static Toc SWR = new Toc("SW", "South Western Railway"); 

        public static Station Surbiton
        {
            get
            {
                var s = new Station();
                s.Add(TestLocations.Surbiton);
                s.Name = "Surbiton";
                s.Nlc = "557100";
                s.Coordinates = new Coordinates()
                {
                    Longitude = new decimal(-0.303959858),
                    Latitude = new decimal(51.39246129)
                };
                s.StationOperator = SWR;
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
                s.StationOperator = new Toc("NR", "Network Rail");
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
                s.StationOperator = SWR;
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