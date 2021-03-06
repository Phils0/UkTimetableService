using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public enum FindStatus
    {
        Error,
        Success,
        LocationNotFound,
        NoServicesForLocation
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
        /// Locations loaded
        /// </summary>
        bool IsLoaded { get; }
        
        /// <summary>
        /// Update a location with its NLC
        /// </summary>
        /// <param name="tiploc">Tiploc for Location to update</param>
        /// <param name="nlc">NLC to set</param>
        void UpdateLocationNlc(string tiploc, string nlc);

        /// <summary>
        /// Try find station
        /// </summary>
        /// <param name="threeLetterCode">Three letter code for Location to find</param>
        /// <param name="location">Found station</param>
        /// <returns></returns>
        bool TryGetStation(string threeLetterCode, out Station location);

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
        /// <param name="config">Configuration for gathering the results</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at, GatherConfiguration config, bool returnCancelled);

        /// <summary>
        /// Scheduled services departing on date
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="dayBoundary"></param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.</returns>
        (FindStatus status, ResolvedServiceStop[] services) AllDepartures(string location, DateTime onDate, GatherConfiguration.GatherFilter filter, bool returnCancelled, Time dayBoundary);
        
        /// <summary>
        /// Scheduled services arriving at location on date near to time
        /// </summary>
        /// <param name="location">Three Letter Code</param>
        /// <param name="at">Date and Time</param>
        /// <param name="config">Configuration for gathering the results</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at, GatherConfiguration config, bool returnCancelled);

        /// <summary>
        /// Scheduled services arriving on date
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <param name="dayBoundary"></param>
        /// <returns>Schedules of running services.</returns>
        (FindStatus status, ResolvedServiceStop[] services) AllArrivals(string location, DateTime onDate, GatherConfiguration.GatherFilter filter, bool returnCancelled,  Time dayBoundary);
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
        
        public bool IsLoaded { get; set; }
        
        private Station CreateStation(IGrouping<string, Location> locations)
        {
            var station = new Station();

            foreach (var location in locations)
            {
                station. Add(location);
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
            if (TryGetLocation(tiploc, out Location location))
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

        public bool TryGetStation(string threeLetterCode, out Station location)
        {
            location = Station.NotSet;
            return !string.IsNullOrEmpty(threeLetterCode) && Locations.TryGetValue(threeLetterCode, out location);
        }

        public bool TryGetLocation(string tiploc, out Location location)
        {
            location = Location.NotSet;
            if(!string.IsNullOrEmpty(tiploc) && LocationsByTiploc.TryGetValue(tiploc, out location))
                return location.IsActive;
            
            _logger.Information("Did not find location {tiploc}", tiploc);
            return false;
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at, GatherConfiguration config, bool returnCancelled)
        {
            return Find(location, (station) => station.Timetable.FindDepartures(at, config), returnCancelled);
        }

        public (FindStatus status, ResolvedServiceStop[] services) AllDepartures(string location, DateTime onDate, GatherConfiguration.GatherFilter filter, bool returnCancelled,  Time dayBoundary)
        {
            return Find(location, (station) => station.Timetable.AllDepartures(onDate, filter, dayBoundary), returnCancelled);
        }

        private (FindStatus status, ResolvedServiceStop[] services) Find(string location,  Func<Station, ResolvedServiceStop[]> findFunc, bool returnCancelled)
        {
            var status = FindStatus.LocationNotFound;

            if (!string.IsNullOrEmpty(location) && TryGetStation(location, out var station))
            {
                var departures = findFunc(station);
                departures = Filter(departures, returnCancelled);

                if (departures.Any())
                {
                    return (status: FindStatus.Success, services: departures);
                }

                status = FindStatus.NoServicesForLocation;
            }

            return (status: status, services: new ResolvedServiceStop[0]);
        }
        
        private ResolvedServiceStop[] Filter(IEnumerable<ResolvedServiceStop> services, bool returnCancelled)
        {
            var filter = returnCancelled ? (IServiceFilter) new ServiceDeduplicator() : new ServiceCancelledFilter();
            return filter.Filter(services).ToArray();
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at, GatherConfiguration config, bool returnCancelled)
        {
            return Find(location, (station) => station.Timetable.FindArrivals(at, config), returnCancelled);
        }

        public (FindStatus status, ResolvedServiceStop[] services) AllArrivals(string location, DateTime onDate, GatherConfiguration.GatherFilter filter, bool returnCancelled,  Time dayBoundary)
        {
            return Find(location, (station) => station.Timetable.AllArrivals(onDate, filter, dayBoundary), returnCancelled);
        }
    }
}