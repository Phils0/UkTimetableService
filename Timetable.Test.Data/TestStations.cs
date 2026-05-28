using System.Linq.Expressions;

namespace Timetable.Test.Data
{
    public static class TestStations
    {
        private readonly static Toc SWR = new Toc("SW", "South Western Railway");

        /// <summary>
        /// Creates a minimal <see cref="Station"/> for the given CRS code, with a single master-station
        /// location whose Tiploc and CRS are both that code. For tests that only care about station identity
        /// (e.g. station-group membership) rather than full reference data.
        /// </summary>
        public static Station Create(string crs)
        {
            var station = new Station();
            station.AddMasterStationLocation(new Location { Tiploc = crs, ThreeLetterCode = crs });
            return station;
        }

        public static Station Surbiton
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.Surbiton);
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
                s.AddMasterStationLocation(TestLocations.WaterlooMain);
                s.AddMasterStationLocation(TestLocations.WaterlooWindsor);
                s.StationOperator = new Toc("NR", "Network Rail");
                return s;
            }
        }
        
        public static Station ClaphamJunction
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.CLPHMJN);
                s.AddMasterStationLocation(TestLocations.CLPHMJC);
                return s;
            }
        }
        
        public static Station Woking
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.Woking);
                return s;
            }
        }
        
        public static Station Weybridge
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.Weybridge);
                s.StationOperator = SWR;
                return s;
            }
        }
        
        public static Station Wimbledon
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.Wimbledon);
                return s;
            }
        }
        
        public static Station Vauxhall
        {
            get
            {
                var s = new Station();
                s.AddMasterStationLocation(TestLocations.Vauxhall);
                return s;
            }
        }
    }
}