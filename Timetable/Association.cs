using System;
using Serilog;

namespace Timetable
{
    public enum AssociationCategory
    {
        None,
        Join, // JJ
        Split, // VV
        NextPrevious, // NP
        Linked, // LK 
    }

    public static class AssociationCategoryExtensions
    {
        public static bool IsJoin(this AssociationCategory category) => category == AssociationCategory.Join;
        public static bool IsSplit(this AssociationCategory category) => category == AssociationCategory.Split;
    }

    public enum AssociationDateIndicator
    {
        None,
        Standard, // S
        NextDay, // N
        PreviousDay // P
    }

    /// <summary>
    /// An indivdual schedule
    /// </summary>
    public class Association
    {
        internal ILogger Logger { get; }
        public AssociationService Main { get; set; }

        public AssociationService Associated { get; set; }

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
            Logger = logger;
        }

        public void SetService(IService service, bool isMain)
        {
            var associationService = isMain ? Main : Associated;
            if (!associationService.TrySetService(service))
            {
                var msg = isMain ? "Main" : "Associated";
                throw new ArgumentException($"Service {service} not valid for association {this} ({msg})");
            }
        }

        public bool AppliesOn(DateTime date, string timetableUid)
        {
            // Default to date if cancelled as no 
            return IsMain(timetableUid)
                ? Calendar.IsActiveOn(date)
                : Calendar.IsActiveOn(ResolveDate(date, timetableUid));
        }

        internal DateTime ResolveDate(DateTime onDate, string timetableUid)
        {
            var isMain = IsMain(timetableUid);

            switch (DateIndicator)
            {
                case AssociationDateIndicator.Standard:
                    return onDate;
                case AssociationDateIndicator.NextDay:
                    return isMain ? onDate.AddDays(1) : onDate.AddDays(-1);
                case AssociationDateIndicator.PreviousDay:
                    return isMain ? onDate.AddDays(-1) : onDate.AddDays(1);
                default:
                {
                    if (IsCancelled())
                        return onDate;
                    
                    throw new ArgumentException($"Unhandled DateIndicator value {DateIndicator}", nameof(DateIndicator));
                }
            }
        }

        internal bool IsMain(string timetableUid)
        {
            return Main.IsService(timetableUid);
        }

        internal IService GetOtherService(string timetableUid)
        {
            return IsMain(timetableUid) ? Associated.Service : Main.Service;
        }

        internal bool HasConsistentLocation(ISchedule service, bool isMain)
        {
            try
            {
                var stop = service.GetStop(Main.AtLocation, Main.Sequence);
                return isMain ? stop.IsMainConsistent(Category) : stop.IsAssociatedConsistent(Category);
            }
            catch (Exception e)
            {
                Logger.Warning(e,
                    "Error when matching association location {location} {service} {association}:{main}",
                    Main.AtLocation, service, this, isMain);
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Main}-{Associated} -{StpIndicator} {Calendar}";
        }
    }
}