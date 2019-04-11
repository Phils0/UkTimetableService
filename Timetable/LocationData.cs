using System.Collections.Generic;
using System.Linq;

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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="masterLocations">Master list of locations (used as a filter)</param>
        public LocationData(ICollection<Location> masterLocations)
        {
            Locations = masterLocations.
                GroupBy(l => l.ThreeLetterCode, l => l).
                ToDictionary(g => g.Key, CreateStation);

            LocationsByTiploc = masterLocations.ToDictionary(l => l.Tiploc, l => l);
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
            if (LocationsByTiploc.TryGetValue(tiploc, out var location))
            {
                location.Nlc = nlc;
            }
        }

        public bool TryGetLocation(string tiploc, out Location location)
        {
            return LocationsByTiploc.TryGetValue(tiploc, out location);
        }
    }
}