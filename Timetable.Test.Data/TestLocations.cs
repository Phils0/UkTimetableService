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
    }
}