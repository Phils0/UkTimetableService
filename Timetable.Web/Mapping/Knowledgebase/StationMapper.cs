using NreKnowledgebase.SchemaV4;

namespace Timetable.Web.Mapping.Knowledgebase
{
    internal class StationMapper
    {
        private readonly TocLookup _tocLookup;

        internal StationMapper(TocLookup lookup)
        {
            _tocLookup = lookup;
        }
        
        
        internal void Update(Station station, StationStructure kbStation)
        {
            station.Name = kbStation.Name;
            station.Nlc = kbStation.AlternativeIdentifiers?.NationalLocationCode;
            station.Coordinates = new Coordinates()
            {
                Longitude = kbStation.Longitude,
                Latitude = kbStation.Latitude
            };
            var stationOperator = string.IsNullOrEmpty(kbStation.StationOperator) ?
                Toc.Unknown : 
                _tocLookup.FindOrAdd(kbStation.StationOperator);
            station.StationOperator = stationOperator;
        }
    }
}