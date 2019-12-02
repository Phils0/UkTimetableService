namespace Timetable
{
    public class AssociationService
    {
        public string TimetableUid { get; set; }
        public Location AtLocation { get; set; }
        public int Sequence { get; set; }
        public Service Service { get; internal set;  }

        public ScheduleLocation AtStop { get;  }

        public bool IsService(string timetableId) => TimetableUid == timetableId;
        
        public override string ToString()
        {
            return TimetableUid;
        }
    }
}