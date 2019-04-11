using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public interface ILocationData
    {
        /// <summary>
        /// Stations by Three Letter Code (CRS)
        /// </summary>
        IReadOnlyDictionary<string, Station> Locations { get; }

        /// <summary>
        /// Locations by TIPLOC
        /// </summary>
        IReadOnlyDictionary<string, Location> LocationsByTiploc { get; }


        /// <summary>
        /// Update a location with its NLC
        /// </summary>
        /// <param name="tiploc">Tiploc for Location to update</param>
        /// <param name="nlc">NLC to set</param>
        void UpdateLocationNlc(string tiploc, string nlc);


        /// <summary>
        /// Try find location
        /// </summary>
        /// <param name="tiploc">Tiploc for Location to find</param>
        /// <param name="location">Found location</param>
        /// <returns></returns>
        bool TryGetLocation(string tiploc, out Location location);
    }

    /// <summary>
    /// Data container to hold loaded timetable
    /// </summary>
    public class LocationData : ILocationData
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="masterLocations">Master list of locations (used as a filter)</param>
        public LocationData(ICollection<Location> masterLocations, ILogger logger)
        {
            _logger = logger;
            
            LocationsByTiploc = masterLocations.ToDictionary(l => l.Tiploc, l => l);
            Locations = masterLocations.
                GroupBy(l => l.ThreeLetterCode, l => l).
                ToDictionary(g => g.Key, CreateStation);
        }
        
        private Station CreateStation(IGrouping<string, Location> locations)
        {
            var station = new Station();

            foreach (var location in locations)
            {
                station.Add(location);
            }

            return station;
        }
        
        /// <summary>
        /// Stations by Three Letter Code (CRS)
        /// </summary>
        public IReadOnlyDictionary<string, Station> Locations { get; }
        /// <summary>
        /// Locations by TIPLOC
        /// </summary>
        public IReadOnlyDictionary<string, Location> LocationsByTiploc { get; }

        public void UpdateLocationNlc(string tiploc, string nlc)
        {
            if (TryGetLocation(tiploc, out var location))
            {
                location.Nlc = nlc;
            }
        }

        public bool TryGetLocation(string tiploc, out Location location)
        {
            if(LocationsByTiploc.TryGetValue(tiploc, out location))
                return true;
            
            _logger.Information("Did not find location {tiploc}", tiploc);
            return false;
        }
    }
}