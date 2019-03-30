namespace Timetable.Web.Test.Cif
{
    internal static class TestStations
    {
        internal static CifParser.RdgRecords.Station Surbiton => new CifParser.RdgRecords.Station()
        {
            RecordType = "A",
            Tiploc = "SURBITN",
            ThreeLetterCode = "SUR",
            Name = "SURBITON",
            InterchangeStatus = CifParser.RdgRecords.InterchangeStatus.Normal,
            East = 15181,
            North = 61673,
            PositionIsEstimated = false,
            MinimumChangeTime = 6,
            SubsidiaryThreeLetterCode = "SUR"
        };

        internal static CifParser.RdgRecords.Station WaterlooMain => new CifParser.RdgRecords.Station()
        {
            RecordType = "A",
            Tiploc = "WATRLMN",
            ThreeLetterCode = "WAT",
            Name = "LONDON WATERLOO",
            InterchangeStatus = CifParser.RdgRecords.InterchangeStatus.Main,
            East = 15312,
            North = 61798,
            PositionIsEstimated = false,
            MinimumChangeTime = 15,
            SubsidiaryThreeLetterCode = "WAT"
        };

        internal static CifParser.RdgRecords.Station WaterlooWindsor => new CifParser.RdgRecords.Station()
        {
            RecordType = "A",
            Tiploc = "WATRLOW",
            ThreeLetterCode = "WAT",
            Name = "LONDON WATERLOO",
            InterchangeStatus = CifParser.RdgRecords.InterchangeStatus.SubsidiaryLocation,
            East = 15312,
            North = 61798,
            PositionIsEstimated = true,
            MinimumChangeTime = 15,
            SubsidiaryThreeLetterCode = "WAT"
        };
    }
}