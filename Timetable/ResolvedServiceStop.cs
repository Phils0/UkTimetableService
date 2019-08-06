using System;

namespace Timetable
{
    public class ResolvedServiceStop : ResolvedService
    {
        public ScheduleLocation Stop { get; private set; }

        public ResolvedServiceStop(ResolvedService service, ScheduleLocation stop)
            : this(service.Details, stop, service.On, service.IsCancelled)
        {
        }

        public ResolvedServiceStop(Schedule service, ScheduleLocation stop, DateTime on, bool isCancelled)
            : base(service, on, isCancelled)
        {
            Stop = stop;
        }

        public override string ToString()
        {
            return $"{base.ToString()} {Stop}";
        }
    }
}