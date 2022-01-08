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
        /// <param name="cifLocation"></param>
        void Update(Location cifLocation);

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

        delegate (FindStatus status, ResolvedServiceStop[] services) Find(string location, DateTime at,
            GatherConfiguration config);

        delegate (FindStatus status, ResolvedServiceStop[] services) AllOnDay(string location, DateTime onDate,
            GatherConfiguration.GatherFilter filter, Time dayBoundary);

        /// <summary>
        /// Scheduled services departing from location on date near to time 
        /// </summary>
        /// <param name="location">Three Letter Code</param>
        /// <param name="at">Date and Time</param>
        /// <param name="config">Configuration for gathering the results</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.  If a service departs at time counts as first of after.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at,
            GatherConfiguration config);

        /// <summary>
        /// Scheduled services departing on date
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="dayBoundary"></param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.</returns>
        (FindStatus status, ResolvedServiceStop[] services) AllDepartures(string location, DateTime onDate,
            GatherConfiguration.GatherFilter filter, Time dayBoundary);

        /// <summary>
        /// Scheduled services arriving at location on date near to time
        /// </summary>
        /// <param name="location">Three Letter Code</param>
        /// <param name="at">Date and Time</param>
        /// <param name="config">Configuration for gathering the results</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <returns>Schedules of running services.  If a service arrives at time counts as first of before.</returns>
        (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at,
            GatherConfiguration config);

        /// <summary>
        /// Scheduled services arriving on date
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onDate">Date and Time</param>
        /// <param name="filter">Any filter</param>
        /// <param name="returnCancelled">Whether to return cancelled services</param>
        /// <param name="dayBoundary"></param>
        /// <returns>Schedules of running services.</returns>
        (FindStatus status, ResolvedServiceStop[] services) AllArrivals(string location, DateTime onDate,
            GatherConfiguration.GatherFilter filter, Time dayBoundary);

        /// <summary>
        /// Results filter
        /// </summary>
        ServiceFilters Filters { get; }
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
        public LocationData(ICollection<Location> masterLocations, ILogger logger, ServiceFilters filters)
        {
            _logger = logger;
            Filters = filters;
            _locations = masterLocations.ToDictionary(l => l.Tiploc, l => l);

            _stations = masterLocations.GroupBy(l => l.ThreeLetterCode, l => l).ToDictionary(g => g.Key, CreateStation);
        }

        public bool IsLoaded { get; set; }

        private Station CreateStation(IGrouping<string, Location> locations)
        {
            var station = new Station();

            foreach (var location in locations)
            {
                station.AddMasterStationLocation(location);
            }

            return station;
        }

        /// <summary>
        /// Stations by Three Letter Code (CRS)
        /// </summary>
        public IReadOnlyDictionary<string, Station> Locations => _stations;

        /// <summary>
        /// Locations by TIPLOC
        /// </summary>
        public IReadOnlyDictionary<string, Location> LocationsByTiploc => _locations;

        private Dictionary<string, Location> _locations;
        private Dictionary<string, Station> _stations;

        public void Update(Location cifLocation)
        {
            if (TryGetLocation(cifLocation.Tiploc, out Location location))
            {
                location.Nlc = cifLocation.Nlc;
            }
            else
            {
                _locations.Add(cifLocation.Tiploc, cifLocation);
                if (cifLocation.HasThreeLetterCode)
                    AddOrUpdateStation(cifLocation);
            }
        }

        private void AddOrUpdateStation(Location location)
        {
            // Has CRS so should be active
            location.IsActive = true;
            if (!TryGetStation(location.ThreeLetterCode, out var station))
            {
                station = new Station();
                _stations.Add(location.ThreeLetterCode, station);
            }

            station.AddCifLocation(location);
        }

        public bool TryGetStation(string threeLetterCode, out Station location)
        {
            location = Station.NotSet;
            return !string.IsNullOrEmpty(threeLetterCode) && Locations.TryGetValue(threeLetterCode, out location);
        }

        public bool TryGetLocation(string tiploc, out Location location)
        {
            location = Location.NotSet;
            if (!string.IsNullOrEmpty(tiploc) && LocationsByTiploc.TryGetValue(tiploc, out location))
                return location.IsActive;

            _logger.Information("Did not find location {tiploc}", tiploc);
            return false;
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindDepartures(string location, DateTime at,
            GatherConfiguration config)
        {
            return Find(location, (station) => station.Timetable.FindDepartures(at, config));
        }

        public (FindStatus status, ResolvedServiceStop[] services) AllDepartures(string location, DateTime onDate,
            GatherConfiguration.GatherFilter filter, Time dayBoundary)
        {
            return Find(location, (station) => station.Timetable.AllDepartures(onDate, filter, dayBoundary));
        }

        private (FindStatus status, ResolvedServiceStop[] services) Find(string location,
            Func<Station, ResolvedServiceStop[]> findFunc)
        {
            var status = FindStatus.LocationNotFound;

            if (!string.IsNullOrEmpty(location) && TryGetStation(location, out var station))
            {
                var results = findFunc(station);
                if (results.Any())
                {
                    return (status: FindStatus.Success, services: results);
                }

                status = FindStatus.NoServicesForLocation;
            }

            return (status: status, services: new ResolvedServiceStop[0]);
        }

        public (FindStatus status, ResolvedServiceStop[] services) FindArrivals(string location, DateTime at,
            GatherConfiguration config)
        {
            return Find(location, (station) => station.Timetable.FindArrivals(at, config));
        }

        public (FindStatus status, ResolvedServiceStop[] services) AllArrivals(string location, DateTime onDate,
            GatherConfiguration.GatherFilter filter, Time dayBoundary)
        {
            return Find(location, (station) => station.Timetable.AllArrivals(onDate, filter, dayBoundary));
        }

        public ServiceFilters Filters { get; }
    }
}