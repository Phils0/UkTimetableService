using DarwinClient.SchemaV16;

namespace Timetable.Web.Mapping.Darwin
{
    internal class StationMapper
    {
        private readonly TocLookup _tocLookup;

        internal StationMapper(TocLookup lookup)
        {
            _tocLookup = lookup;
        }
        
        internal void Update(Station station, LocationRef darwinLocation)
        {
            station.Name = darwinLocation.locname;
            var stationOperator = string.IsNullOrEmpty(darwinLocation.toc) ?
                Toc.Unknown : 
                _tocLookup.FindOrAdd(darwinLocation.toc);
            station.StationOperator = stationOperator;
        }
    }
}