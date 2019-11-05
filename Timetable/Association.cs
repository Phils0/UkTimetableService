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
        /// <summary>
        /// Main Timetable Id
        /// </summary>
        public string MainTimetableUid { get; set; }
        
        public Service MainService { get; private set;  }

        /// <summary>
        /// Associated Timetable Id
        /// </summary>
        public string AssociatedTimetableUid { get; set; }
        
        public Service AssociatedService { get; private set;  }
        
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
        
        public bool IsPublic { get; set; }

        public void AddToService(Service service, bool isMain)
        {
            if (isMain)
            {
                CheckMatchingTimetableId(service, MainTimetableUid,  "Main");
                MainService = service;
            }
            else
            {
                CheckMatchingTimetableId(service, AssociatedTimetableUid, "Associated");
                AssociatedService = service;
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
        
        public override string ToString()
        {
            return $"{MainTimetableUid}-{AssociatedTimetableUid} -{StpIndicator} {Calendar}";
        }
    }
}