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
        
        public string TimetableUid => Details.TimetableUid;

        public bool OperatedBy(string toc) => Details.OperatedBy(toc);

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

        public ResolvedServiceStop GetStop(Location at, int sequence)
        {
            return  new ResolvedServiceStop(this, Details.GetStop(at, sequence));
        }
        
        public ResolvedServiceStop Origin => new ResolvedServiceStop(this, Details.Origin);
        public ResolvedServiceStop Destination => new ResolvedServiceStop(this, Details.Destination);
        
        public override string ToString()
        {
            return IsCancelled ? $"{Details.TimetableUid} {On.ToYMD()} CANCELLED" : $"{Details.TimetableUid} {On.ToYMD()}";
        }
    }
}