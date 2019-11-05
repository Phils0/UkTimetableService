using System;

namespace Timetable.Web.Mapping
{
    public static class AssociationConverter
    {
        public static AssociationCategory ConvertCategory(string input)
        {
            switch (input)
            {
                case "JJ":
                    return AssociationCategory.Join;
                case "VV":
                    return AssociationCategory.Split;
                case "NP":
                    return AssociationCategory.NextPrevious;
                case "":
                case null:
                    return AssociationCategory.None;
                default:
                    throw  new ArgumentException($"Unknown association category {input}");
            }
        }
        
        public static AssociationDateIndicator ConvertDateIndicator(string input)
        {
            switch (input)
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
                    throw  new ArgumentException($"Unknown association date indicator {input}");
            }
        }
    }
}