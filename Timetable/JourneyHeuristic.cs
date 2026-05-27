namespace Timetable
{
    /// <summary>
    /// Strategy for choosing the canonical stop when a service calls at multiple members of a station group.
    /// </summary>
    public enum JourneyHeuristic
    {
        /// <summary>
        /// Prefer the stop that maximises the journey length within the group:
        /// earliest departure for origin-side groups, latest arrival for destination-side groups.
        /// </summary>
        Longest = 0,

        /// <summary>
        /// Prefer the stop that minimises the journey length within the group:
        /// latest departure for origin-side groups, earliest arrival for destination-side groups.
        /// </summary>
        Shortest = 1
    }
}