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

        public void SetService(Service service, bool isMain)
        {
            var associationService = isMain ? Main : Associated;
            if (!associationService.TrySetService(service))
            {
                var msg = isMain ? "Main" : "Associated";
                throw new ArgumentException($"Service {service} not valid for association {this} ({msg})");                    
            }
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