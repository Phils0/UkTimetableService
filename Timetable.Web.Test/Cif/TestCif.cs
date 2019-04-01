using CifParser.Records;

namespace Timetable.Web.Test.Cif
{
    internal class TestCif
    {
        internal static TiplocInsertAmend Surbiton => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "SURBITN",
            ThreeLetterCode = "SUR",
            Nalco = "557100",
            Description = "SURBITON",

        };
        
        public static TiplocInsertAmend WaterlooMain => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "WATRLMN",
            ThreeLetterCode = "WAT",
            Nalco = "559801",
            Description = "LONDON WATERLOO",
        };

        public static TiplocInsertAmend WaterlooWindsor => new TiplocInsertAmend()
        {
            Action = RecordAction.Create,
            Code = "WATRLOW",
            ThreeLetterCode = "WAT",
            Nalco = "559803",
            Description = "LONDON WATERLOO",
        };
    }
}