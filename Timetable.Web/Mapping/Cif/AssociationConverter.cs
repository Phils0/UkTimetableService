using System;

namespace Timetable.Web.Mapping.Cif
{
    public static class AssociationConverter
    {
        
        public static AssociationCategory ConvertCategory(string source)
        {
            switch (source)
            {
                case "JJ":
                    return AssociationCategory.Join;
                case "VV":
                    return AssociationCategory.Split;
                case "NP":
                    return AssociationCategory.NextPrevious;
                case "LK":
                    return AssociationCategory.Linked;
                case "":
                case null:
                    return AssociationCategory.None;
                default:
                    throw  new ArgumentException($"Unknown association category {source}");
            }
        }
        
        public static AssociationDateIndicator ConvertDateIndicator(string source)
        {
            switch (source)
            {
                case "S":
                    return AssociationDateIndicator.Standard;
                case "N":
                    return AssociationDateIndicator.NextDay;
                case "P":
                    return AssociationDateIndicator.PreviousDay;
                case "":
                case null:
                    return AssociationDateIndicator.None;
                default:
                    throw  new ArgumentException($"Unknown association date indicator {source}");
            }
        }

        public static AssociationService ConvertMain(CifParser.Records.Association source)
        {
            return new AssociationService()
            {
                TimetableUid = source.MainUid,
                Sequence = source.MainSequence,
            };
        }

        public static AssociationService ConvertAssociated(CifParser.Records.Association source)
        {
            return new AssociationService()
            {
                TimetableUid = source.AssociatedUid,
                Sequence = source.AssociationSequence,
            };
        }

        public static void SetServiceLocations(Timetable.Association association)
        {
            association.Main.AtLocation = association.AtLocation;
            association.Associated.AtLocation = association.AtLocation;
        }
    }
}