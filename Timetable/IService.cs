using System;

namespace Timetable
{
    public interface IService
    {
        /// <summary>
        /// CIF Timetable ID
        /// </summary>
        string TimetableUid { get; }
        /// <summary>
        /// Resolve schedule for date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="schedule"></param>
        /// <param name="resolveAssociations"></param>
        /// <returns></returns>
        bool TryResolveOn(DateTime date, out ResolvedService schedule, bool resolveAssociations = true);
        /// <summary>
        /// Find stop for date
        /// </summary>
        /// <param name="find"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        bool TryFindScheduledStop(StopSpecification find, out ResolvedServiceStop stop);
        /// <summary>
        /// Service has associations
        /// </summary>
        /// <returns></returns>
        bool HasAssociations();
        /// <summary>
        /// Add Association
        /// </summary>
        /// <param name="association"></param>
        /// <param name="isMain"></param>
        bool AddAssociation(Association association, bool isMain);
        /// <summary>
        /// Starts before time (date agnostic)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        bool StartsBefore(Time time);
    }
}