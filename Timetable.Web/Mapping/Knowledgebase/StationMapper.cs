using NreKnowledgebase.SchemaV4;

namespace Timetable.Web.Mapping.Knowledgebase
{
    public static class StationMapper
    {
        public static void Update(Station station, StationStructure kbStation)
        {
            station.Name = kbStation.Name;
            station.Nlc = kbStation.AlternativeIdentifiers?.NationalLocationCode;
            station.Coordinates = new Coordinates()
            {
                Longitude = kbStation.Longitude,
                Latitude = kbStation.Latitude
            };
        }
    }
}