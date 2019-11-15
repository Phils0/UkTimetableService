using System;
using System.Linq;

namespace Timetable
{
    public class ResolvedService
    {
        public bool IsCancelled { get; }
        
        public DateTime On { get; }

        public Schedule Details { get; }

        public ResolvedService(Schedule service, DateTime on, bool isCancelled)
        {
            Details = service;
            On = on;
            IsCancelled = isCancelled;
        }

        public bool HasRetailServiceId(string retailServiceId)
        {
            return Details.HasRetailServiceId(retailServiceId);
        }
        
        public bool OperatedBy(string toc)
        {
            return Details.OperatedBy(toc);
        }

        public bool TryFindStop(StopSpecification find, out ResolvedServiceStop stop)
        {
            if (Details.TryFindStop(find, out var found))
            {
                stop = new ResolvedServiceStop(this, found);
                return true;
            }

            stop = null;
            return false;
        }
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details.TimetableUid} {On.ToYMD()} CANCELLED" : $"{Details.TimetableUid} {On.ToYMD()}";
        }
    }
}