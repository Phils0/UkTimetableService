using System;

namespace Timetable
{
    public class AssociationService
    {
        public string TimetableUid { get; set; }
        public Location AtLocation { get; set; }
        public int Sequence { get; set; }
        
        public Service Service { get; private set;  }

        public bool IsService(string timetableId) => TimetableUid == timetableId;
        
        public bool TrySetService(Service service)
        {
            if (!IsService(service.TimetableUid))
                return false;
            
            Service = service;
            return true;
        }
        
        public override string ToString()
        {
            return TimetableUid;
        }
    }
}