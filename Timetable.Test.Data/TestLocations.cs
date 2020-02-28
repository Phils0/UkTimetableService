namespace Timetable.Test.Data
{
    public static class TestLocations
    {
        public static Location Surbiton => new Location()
        {
            Tiploc = "SURBITN",
            ThreeLetterCode = "SUR",
            Nlc = "557100",
            Name = "SURBITON",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15181,
                Northings = 61673,
                IsEstimate = false
            }
        };

        public static Location WaterlooMain => new Location()
        {
            Tiploc = "WATRLMN",
            ThreeLetterCode = "WAT",
            Nlc = "559801",
            Name = "LONDON WATERLOO",
            InterchangeStatus = InterchangeStatus.Main,
            Coordinates = new Coordinates()
            {
                Eastings = 15312,
                Northings = 61798,
                IsEstimate = false
            }
        };

        public static Location WaterlooWindsor => new Location()
        {
            Tiploc = "WATRLOW",
            ThreeLetterCode = "WAT",
            Nlc = "559803",
            Name = "LONDON WATERLOO",
            InterchangeStatus = InterchangeStatus.SubsidiaryLocation,
            Coordinates = new Coordinates()
            {
                Eastings = 15312,
                Northings = 61798,
                IsEstimate = true
            }
        };
        
        public static Location CLPHMJN => new Location()
        {
            Tiploc = "CLPHMJN",
            ThreeLetterCode = "CLJ",
            Nlc = "559500",
            Name = "CLAPHAM JUNCTION",
            InterchangeStatus = InterchangeStatus.SubsidiaryLocation,
            Coordinates = new Coordinates()
            {
                Eastings = 15272,
                Northings = 61755,
                IsEstimate = false
            }
        };
        
        public static Location CLPHMJC => new Location()
        {
            Tiploc = "CLPHMJC",
            ThreeLetterCode = "CLJ",
            Nlc = "559569",
            Name = "CLAPHAM JUNCTION",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15272,
                Northings = 61755,
                IsEstimate = false
            }
        };
        
        public static Location Woking => new Location()
        {
            Tiploc = "WOKING",
            ThreeLetterCode = "WOK",
            Nlc = "568500",
            Name = "WOKING",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15006,
                Northings = 61587,
                IsEstimate = false
            }
        };
        
        public static Location Weybridge => new Location()
        {
            Tiploc = "WEYBDGE",
            ThreeLetterCode = "WYB",
            Nlc = "557700",
            Name = "WEYBRIDGE",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15075,
                Northings = 61636,
                IsEstimate = false
            }
        };
        
        public static Location Wimbledon => new Location()
        {
            Tiploc = "WIMBLDN",
            ThreeLetterCode = "WIM",
            Nlc = "557800",
            Name = "WIMBLEDON",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15075,
                Northings = 61636,
                IsEstimate = false
            }
        };
        
        public static Location Vauxhall => new Location()
        {
            Tiploc = "VAUXHLM",
            ThreeLetterCode = "VXH",
            Nlc = "559700",
            Name = "VAUXHALL",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15304,
                Northings = 61780,
                IsEstimate = false
            }
        };
    }
}