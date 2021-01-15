using System;
using Serilog;

namespace Timetable
{
    public enum AssociationCategory
    {
        None,
        Join,          // JJ
        Split,         // VV
        NextPrevious,  // NP
        Linked,        // LK 
    }

    public static class AssociationCategoryExtensions
    {
        public static bool IsJoin(this AssociationCategory category) => category == AssociationCategory.Join;
        public static bool IsSplit(this AssociationCategory category) => category == AssociationCategory.Split;
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
        private readonly ILogger _logger;
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

        public Association(ILogger logger)
        {
            _logger = logger;
        }
        
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

        internal bool IsMain(string timetableUid)
        {
            return Main.IsService(timetableUid);
        }

        internal Service GetOtherService(string timetableUid)
        {
            return IsMain(timetableUid) ? 
                Associated.Service :
                Main.Service;
        }
        
        internal bool HasConsistentLocation(Schedule service, bool isMain)
        {
            try
            {
                var stop = service.GetStop(Main.AtLocation, Main.Sequence);
                return isMain ? stop.IsMainConsistent(Category) : stop.IsAssociatedConsistent(Category);
            }
            catch (Exception e)
            {
                _logger.Warning(e, 
                    "Error when matching association location {location} {service} {association}:{main}", Main.AtLocation, service, this, isMain);
                return false;
            }
        }
        
        public override string ToString()
        {
            return $"{Main}-{Associated} -{StpIndicator} {Calendar}";
        }
    }
}