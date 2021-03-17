using System;
using Serilog;
using Serilog.Events;

namespace Timetable
{
    public class ResolvedAssociation
    {
        public bool IsCancelled { get; }
        public ResolvedService AssociatedService { get; }
        public DateTime On { get; }
        public Association Details { get; }
        public AssociationCategory Category => Details.Category;

        public bool IsJoin => AssociationCategory.Join == Category;
        public bool IsSplit => AssociationCategory.Split == Category;

        public ResolvedAssociation(Association association, DateTime on, bool isCancelled, ResolvedService associatedService)
        {
            Details = association;
            On = on;
            IsCancelled = isCancelled;
            AssociatedService = associatedService;
        }

        internal ILogger Logger => Details.Logger;
        
        internal ResolvedServiceStop GetStop(ResolvedService service)
        {
            try
            {
                var association = IsMain(service.TimetableUid) ?
                    Details.Main :
                    Details.Associated;

                var stop = service.GetStop(association.AtLocation, association.Sequence);
                return stop;
            }
            catch (Exception e)
            {
                var level = IsCancelled ? LogEventLevel.Information : LogEventLevel.Warning;
                Logger.Write(level, e, "Did not find association stop in service {source} : {service}", this, service);
                return null;
            }
        }
        
        public bool IsMain(string timetableUid)
        {
            return Details.IsMain(timetableUid);
        }
        
        public bool IsAssociated(string timetableUid)
        {
            return !IsMain(timetableUid);
        }
        
        public ResolvedAssociationStop Stop { get; private set; }
        
        internal  void SetAssociationStop(ResolvedService service)
        {
            var stop = GetStop(service)?.Stop;
            var associatedServiceStop = GetStop(AssociatedService)?.Stop;
            Stop = new ResolvedAssociationStop(stop, associatedServiceStop);
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details} {On.ToYMD()} {AssociatedService} CANCELLED" : $"{Details} {On.ToYMD()} {AssociatedService}";
        }
    }
}