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
        
        public static Location ClaphamJunction1 => new Location()
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
        
        public static Location ClaphamJunction2 => new Location()
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
    }
}