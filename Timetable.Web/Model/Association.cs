using System;

namespace Timetable.Web.Model
{
    public class AssociationBase
    {
        /// <summary>
        /// Parent schedule is main service
        /// the one being joined to / split from
        /// </summary>
        public bool IsMain { get; set; }
        
        /// <summary>
        /// Association is broken
        /// </summary>
        public bool IsBroken { get; set; }

        /// <summary>
        /// Association is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Running date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Split or Join
        /// </summary>
        public string AssociationCategory { get; set; }

        /// <summary>
        /// Split/Join stop for this service
        /// </summary>
        public ScheduledStop Stop { get; set; }

        /// <summary>
        /// Split/Join stop for other service
        /// </summary>
        public ScheduledStop AssociatedServiceStop { get; set; }
    }
    
    public class Association : AssociationBase
    {
        /// <summary>
        /// Other service
        /// </summary>
        public Service AssociatedService { get; set; }
    }
    
    public class AssociationSummary : AssociationBase
    {
        /// <summary>
        /// Other service
        /// </summary>
        public ServiceSummary AssociatedService { get; set; }
    }
}