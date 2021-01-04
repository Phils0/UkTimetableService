using DarwinClient.SchemaV16;
using Serilog;

namespace Timetable.Web.Mapping.Darwin
{
    internal class StationMapper
    {
        private readonly TocLookup _tocLookup;
        private readonly ILocationData _locations;
        private readonly ILogger _log;

        internal StationMapper(TocLookup lookup, ILocationData locations, ILogger log)
        {
            _tocLookup = lookup;
            _locations = locations;
            _log = log;
        }
        
        internal void Update(Station station, LocationRef darwinLocation)
        {
            station.Name = darwinLocation.locname;
            var stationOperator = string.IsNullOrEmpty(darwinLocation.toc) ?
                Toc.Unknown : 
                _tocLookup.FindOrAdd(darwinLocation.toc);
            station.StationOperator = stationOperator;
        }
        
        internal void AddRule(Station station, Via viaRule)
        {
            Location location2 = Location.NotSet;
            if (TryGetLocation(viaRule.dest, "Destination", out var destination) &&
                TryGetLocation(viaRule.loc1, "Location1", out var location1)  &&
                (string.IsNullOrEmpty(viaRule.loc2) || TryGetLocation(viaRule.loc2, "Location2", out location2)))
            {
                var rule = new ViaRule()
                {
                    At = station,
                    Destination = destination,
                    Location1 = location1,
                    Location2 = location2,
                    Text = viaRule.viatext
                };
                station.Add(rule);
            }

            bool TryGetLocation(string tiploc, string property, out Location location)
            {
                location = Location.NotSet;
                if (_locations.LocationsByTiploc.TryGetValue(tiploc, out location))
                    return true;

                _log.Warning("Did not find {property} for via rule {@viaRule}", property, viaRule);
                return false;
            }
        }
    }
}