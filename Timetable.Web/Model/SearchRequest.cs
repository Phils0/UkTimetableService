namespace Timetable.Web.Model
{
    /// <summary>
    /// Arrivals \ Departures from a location
    /// </summary>
    public class SearchRequest
    {
        public const string ARRIVALS = "Arrivals";
        public const string DEPARTURES = "Departures";
        
        /// <summary>
        /// Location (Three Letter code) to generate the board at
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// When
        /// </summary>
        public Window At { get; set; }
        /// <summary>
        /// Arrivals coming from \ Departures going to location (Three Letter code)
        /// </summary>
        public string ComingFromGoingTo { get; set; } = "";
        /// <summary>
        /// Request type
        /// </summary>
        public string Type { get; set; } = "";
        public override string ToString()
        {
            return string.IsNullOrEmpty(ComingFromGoingTo) ? 
                $"{Location}@{At}" :
                Type == DEPARTURES ?
                    $"{Location}@{At} to {ComingFromGoingTo}" :
                    $"{Location}@{At} from {ComingFromGoingTo}" ;
        }
    }
}