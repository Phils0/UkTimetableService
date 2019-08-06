using System;

namespace Timetable
{
    public class ResolvedService
    {
        public bool IsCancelled { get; private set; }

        public DateTime On { get; private set; }

        public Schedule Details { get; private set; }

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

        public bool TryFindStop(Station location, Time time, out ResolvedServiceStop stop)
        {
            if (Details.TryFindStop(location, time, out var found))
            {
                stop = new ResolvedServiceStop(this, found);
                return true;
            }

            stop = null;
            return false;
        }

        public override string ToString()
        {
            return IsCancelled ? $"{Details.TimetableUid} {On:d} CANCELLED" : $"{Details.TimetableUid} {On:d}";
        }
    }
}