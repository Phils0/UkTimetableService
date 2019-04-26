namespace Timetable.Web.Model
{
    /// <summary>
    /// Arrivals \ Departures from a location
    /// </summary>
    public class LocationTimetableRequest
    {
        /// <summary>
        /// Location (Three Letter code) to generate the board at
        /// </summary>
        public string Location { get; set; }

        public Window At { get; set; }

        /// <summary>
        /// Arrivals coming from \ Departures going to location (Three Letter code)
        /// </summary>
        public string ComingFromGoingTo { get; set; } = "";

        public override string ToString()
        {
            return $"{Location}@{At}";
        }

        public bool HasFilterLocation() => !string.IsNullOrWhiteSpace(ComingFromGoingTo);        
    }
}