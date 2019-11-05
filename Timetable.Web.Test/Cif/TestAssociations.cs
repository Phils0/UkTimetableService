using System;

namespace Timetable.Web.Test.Cif
{
    internal static class TestAssociations
    {
        internal const string X12345 = "X12345";
        internal const string A98765 = "A98765";

        internal static CifParser.Records.Association CreateAssociation(
            string mainUid = X12345, 
            string otherUid = A98765,
            CifParser.Records.StpIndicator stp = CifParser.Records.StpIndicator.P,
            string category = "JJ")
        {
            return new CifParser.Records.Association()
            {
                MainUid = mainUid,
                AssociatedUid = otherUid,
                StpIndicator = stp,
                RunsFrom = new DateTime(2019, 8, 1),
                RunsTo = new DateTime(2019, 8, 31),
                DayMask = "1111100",
                Location =  "SURBITN",
                Category = category,
                DateIndicator = "S",
                AssociationType = "P"
            };
        }
        
    }
}