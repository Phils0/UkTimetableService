using System.Collections.Generic;
using ReflectionMagic;

namespace Timetable.Test
{
    internal static class InternalsHelperExtensions
    {
        internal static Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> GetAssociations(this IService service)
        {
            var associations = service.AsDynamic()._associations;
            return associations == null ? null : (Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>) associations.RealObject;
        }

        internal static SortedList<(StpIndicator indicator, ICalendar calendar), CifSchedule> GetSchedules(this CifService service)
        {
            return (SortedList<(StpIndicator indicator, ICalendar calendar), CifSchedule>) service.AsDynamic()._multipleSchedules.RealObject;
        }
        
        internal static IService GetService(this TimetableData timetable, string timetableUid)
        {
            var services = (Dictionary<string, IService>) timetable.AsDynamic()._timetableUidMap.RealObject;
            return services[timetableUid];
        }
        
        internal static IService GetService(this PublicSchedule schedule, Time time)
        {
            return GetServices(schedule, time)[0];
        }

        internal static IService[] GetServices(this PublicSchedule schedule, Time time)
        {
            var services = (SortedList<Time, IService[]>) schedule.AsDynamic()._services.RealObject;
            return services.TryGetValue(time, out var found) ? found : new IService[0];
        }
        
        internal static PublicSchedule GetDepartureTimes(this LocationTimetable timetable)
        {
            var departures = (PublicSchedule) timetable.AsDynamic()._departures.RealObject;
            return departures;
        }
        
        internal static PublicSchedule GetArrivalTimes(this LocationTimetable timetable)
        {
            var arrivals = (PublicSchedule) timetable.AsDynamic()._arrivals.RealObject;
            return arrivals;
        }
    }
}