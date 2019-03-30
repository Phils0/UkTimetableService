using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Timetable
{
    public class Loader
    {
        private readonly IDataLoader _loader;

        public Loader(IDataLoader loader)
        {
            _loader = loader;
        }

        public async Task<Data> Load(CancellationToken token)
        {
            return new Data()
            {
                Locations = await LoadLocations(token).ConfigureAwait(false)
            };
            
        }

        private async Task<IDictionary<string, Station>> LoadLocations(CancellationToken token)
        {
            var locations = await _loader.GetStationMasterListAsync(token).ConfigureAwait(false);
            return locations.
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
    }
}