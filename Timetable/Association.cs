using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public enum AssociationCategory
    {
        None,
        Join,          // JJ
        Split,         // VV
        NextPrevious   // NP
    }
    
    public enum AssociationDateIndicator
    {
        None,
        Standard,      // S
        NextDay,       // N
        PreviousDay    // P
    }

    /// <summary>
    /// An indivdual schedule
    /// </summary>
    public class Association
    {
        public AssociationService Main { get; set;  }
        
        public AssociationService Associated { get; set;  }
        
        public Location AtLocation { get; set; }

        /// <summary>
        /// STP (Short Term Plan) Indicator
        /// </summary>
        /// <remarks>
        /// P - Permanent schedule
        /// O - STP overlay of Permanent schedule
        /// N - New STP schedule (not an overlay)
        /// C - STP Cancellation of Permanent schedule
        /// </remarks>
        public StpIndicator StpIndicator { get; set; }

        public bool IsCancelled() => StpIndicator.Cancelled == StpIndicator;
        
        public ICalendar Calendar { get; set; }
        
        public AssociationDateIndicator DateIndicator { get; set; }
        
        public AssociationCategory Category { get; set; }
        
        public bool IsPassenger { get; set; }

        public void AddToService(Service service, bool isMain)
        {
            if (isMain)
            {
                CheckMatchingTimetableId(service, Main.TimetableUid,  "Main");
                Main.Service = service;
            }
            else
            {
                CheckMatchingTimetableId(service, Associated.TimetableUid, "Associated");
                Associated.Service = service;
            }
            service.AddAssociation(this, isMain);
        }

        private void CheckMatchingTimetableId(Service service, string associationUid, string whichUid)
        {
            if(service.TimetableUid != associationUid)
                throw new ArgumentException($"Service {service} not valid for association {this} ({whichUid})");
        }
        
        public bool AppliesOn(DateTime date)
        {
            return Calendar.IsActiveOn(date);
        }

        internal Service GetService(string timetableUid)
        {
            return Main.IsService(timetableUid) ? 
                Main.Service :
                Associated.Service;
        }

        internal Service GetOtherService(string timetableUid)
        {
            return Main.IsService(timetableUid) ? 
                Associated.Service :
                Main.Service;
        }
        
        public override string ToString()
        {
            return $"{Main}-{Associated} -{StpIndicator} {Calendar}";
        }
    }
}