using System;

namespace Timetable.Web.Model
{
    /// <summary>
    /// Arrivals \ Departures from a location
    /// </summary>
    public class SearchRequest
    {
        public const string ARRIVALS = "Arrivals";
        public const string DEPARTURES = "Departures";

        private string _location = "";
        /// <summary>
        /// Location (Three Letter code) to generate the board at
        /// </summary>
        public string Location
        {
            get { return _location; }
            set { _location = string.IsNullOrEmpty(value) ? "" : value.ToUpper(); }
        }
        /// <summary>
        /// When
        /// </summary>
        public Window At { get; set; }

        private string _comingFromGoingTo = "";
        /// <summary>
        /// Arrivals coming from \ Departures going to location (Three Letter code)
        /// </summary>
        public string ComingFromGoingTo
        {
            get { return _comingFromGoingTo; }
            set { _comingFromGoingTo = string.IsNullOrEmpty(value) ? "" : value.ToUpper(); }
        }
        /// <summary>
        /// Request type
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Only services from these TOCs returned
        /// </summary>
        public string TocFilter { get; set; } = "";
        
        public override string ToString()
        {
            var tocs = string.IsNullOrEmpty(TocFilter) ? "" : $" ({TocFilter}) ";
            var fullDay = At.FullDay ? "Day " : "";
            
            var request =  string.IsNullOrEmpty(ComingFromGoingTo) ? 
                $"{Location}@{At}" :
                Type == DEPARTURES ?
                    $"{Location}@{At} to {ComingFromGoingTo}" :
                    $"{Location}@{At} from {ComingFromGoingTo}" ;

            return $"{fullDay}{request}{tocs}";
        }
    }
}