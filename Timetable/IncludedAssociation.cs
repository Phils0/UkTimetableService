namespace Timetable
{
    public class IncludedAssociation
    {
        public static IncludedAssociation NoAssociation = new IncludedAssociation(false, null);
        
        public bool IsIncluded { get; }
        public string TimetableUid { get; }

        public IncludedAssociation(string timetableUid) : this(true, timetableUid)
        {
        }
        
        private IncludedAssociation(bool value, string timetableUid)
        {
            IsIncluded = value;
            TimetableUid = timetableUid;
        }
    }
}