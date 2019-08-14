using System.Collections.Generic;
using ReflectionMagic;

namespace Timetable.Test
{
    internal static class PublicScheduleHelperExtensions
    {
        internal static Service GetService(this PublicSchedule schedule, Time time)
        {
            return GetServices(schedule, time)[0];
        }

        internal static Service[] GetServices(this PublicSchedule schedule, Time time)
        {
            var services = (SortedList<Time, Service[]>) schedule.AsDynamic()._services.RealObject;
            return services.TryGetValue(time, out var found) ? found : new Service[0];
        }
    }
}