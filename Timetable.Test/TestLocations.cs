namespace Timetable.Test
{
    public static class TestLocations
    {
        internal static Location Surbiton => new Location()
        {
            Tiploc = "SURBITN",
            ThreeLetterCode = "SUR",
            Name = "SURBITON",
            InterchangeStatus = InterchangeStatus.Normal,
            Coordinates = new Coordinates()
            {
                Eastings = 15181,
                Northings = 61673,
                IsEstimate = false
            }
        };

        internal static Location WaterlooMain => new Location()
        {
            Tiploc = "WATRLMN",
            ThreeLetterCode = "WAT",
            Name = "LONDON WATERLOO",
            InterchangeStatus = InterchangeStatus.Main,
            Coordinates = new Coordinates()
            {
                Eastings = 15312,
                Northings = 61798,
                IsEstimate = false
            }
        };

        internal static Location WaterlooWindsor => new Location()
        {
            Tiploc = "WATRLOW",
            ThreeLetterCode = "WAT",
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