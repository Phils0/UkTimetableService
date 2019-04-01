using CifStation = CifParser.RdgRecords.Station;
using CifStatus = CifParser.RdgRecords.InterchangeStatus;

namespace Timetable.Web.Test.Cif
{
    internal static class TestStations
    {
        internal static CifStation Surbiton => new CifStation()
        {
            RecordType = "A",
            Tiploc = "SURBITN",
            ThreeLetterCode = "SUR",
            Name = "SURBITON",
            InterchangeStatus = CifStatus.Normal,
            East = 15181,
            North = 61673,
            PositionIsEstimated = false,
            MinimumChangeTime = 6,
            SubsidiaryThreeLetterCode = "SUR"
        };

        internal static CifStation WaterlooMain => new CifStation()
        {
            RecordType = "A",
            Tiploc = "WATRLMN",
            ThreeLetterCode = "WAT",
            Name = "LONDON WATERLOO",
            InterchangeStatus = CifStatus.Main,
            East = 15312,
            North = 61798,
            PositionIsEstimated = false,
            MinimumChangeTime = 15,
            SubsidiaryThreeLetterCode = "WAT"
        };

        internal static CifStation WaterlooWindsor => new CifStation()
        {
            RecordType = "A",
            Tiploc = "WATRLOW",
            ThreeLetterCode = "WAT",
            Name = "LONDON WATERLOO",
            InterchangeStatus = CifStatus.SubsidiaryLocation,
            East = 15312,
            North = 61798,
            PositionIsEstimated = true,
            MinimumChangeTime = 15,
            SubsidiaryThreeLetterCode = "WAT"
        };
    }
}