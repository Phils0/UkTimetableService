using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public enum FindStatus
    {
        Success,
        LocationNotFound,
        NoServicesForLocation,
        NoServicesForLocationOnDate
    }
    
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

        /// <summary>
        /// Scheduled services departing from location on date near to time 
        /// </summary>
        /// <param name="location">Three Letter Code</param>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <param name="to">Optional filter to location</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at, int before, int after, string to = "");

        /// <summary>
        /// Scheduled services arriving at location on date near to time
        /// </summary>
        /// <param name="location">Three Letter Code</param>
        /// <param name="at">Date and Time</param>
        /// <param name="before">Number of services before to return</param>
        /// <param name="after">Number of services after to return</param>
        /// <param name="from">Optional filter from location</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at, int before, int after, string @from = "");
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
            _locations = masterLocations.ToDictionary(l => l.Tiploc, l => l);
            
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
        public IReadOnlyDictionary<string, Location> LocationsByTiploc => _locations;

        private Dictionary<string, Location> _locations;

        public void UpdateLocationNlc(string tiploc, string nlc)
        {
            if (TryGetLocation(tiploc, out var location))
            {
                location.Nlc = nlc;
            }
            else
            {
                location = new Location()
                {
                    Tiploc = tiploc,
                    Nlc = nlc,
                    IsActive = false
                };
                _locations.Add(tiploc, location);
            }
        }

        public bool TryGetLocation(string tiploc, out Location location)
        {
            if(LocationsByTiploc.TryGetValue(tiploc, out location))
                return location.IsActive;
            
            _logger.Information("Did not find location {tiploc}", tiploc);
            return false;
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at, int before, int after, string to = "")
        {
            throw new NotImplementedException();
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at, int before, int after, string @from = "")
        {
            throw new NotImplementedException();
        }
    }
}