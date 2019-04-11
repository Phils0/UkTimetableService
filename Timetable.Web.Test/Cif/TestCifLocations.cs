using CifParser.Records;

namespace Timetable.Web.Test.Cif
{
    internal class TestCifLocations
    {
        internal static TiplocInsertAmend Surbiton => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "SURBITN",
            ThreeLetterCode = "SUR",
            Nalco = "557100",
            Description = "SURBITON",

        };
        
        internal static TiplocInsertAmend ClaphamJunction1 => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "CLPHMJN",
            ThreeLetterCode = "CLJ",
            Nalco = "559500",
            Description = "CLAPHAM JUNCTION",

        };
        
        internal static TiplocInsertAmend ClaphamJunction2 => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "CLPHMJC",
            ThreeLetterCode = "CLJ",
            Nalco = "559569",
            Description = "CLAPHAM JUNCTION",
        };
        
        internal static TiplocInsertAmend WaterlooMain => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "WATRLMN",
            ThreeLetterCode = "WAT",
            Nalco = "559801",
            Description = "LONDON WATERLOO",
        };

        internal static TiplocInsertAmend WaterlooWindsor => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "WATRLOW",
            ThreeLetterCode = "WAT",
            Nalco = "559803",
            Description = "LONDON WATERLOO",
        };
    }
}