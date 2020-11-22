using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Timetable.Web.Loaders
{ 
    public class DataLoader : IDataLoader
    {
        public static readonly TimeSpan Timeout = new TimeSpan(0, 5, 0);

        private readonly ICifLoader _cif;
        private readonly IDarwinLoader _darwin;
        private readonly IKnowledgebaseEnhancer _knowledgebase;
        private readonly ILogger _logger;

        public DataLoader(ICifLoader cif, IDarwinLoader darwin, IKnowledgebaseEnhancer knowledgebase, ILogger logger)
        {
            _cif = cif;
            _darwin = darwin;
            _knowledgebase = knowledgebase;
            _logger = logger;
        }
       
        public async Task<Data> LoadAsync(CancellationToken token)
        {
            Data data = null;
            var tocs = await LoadTocsAsync(token).ConfigureAwait(false);
            var masterLocations = await LoadLocationsAsync(tocs, token).ConfigureAwait(false);

            var updateLocationsTask = Task.Run(async () =>
            {
                 var stations = await UpdateLocationsAsync(masterLocations, tocs, token).ConfigureAwait(false);
            });
            var cifTask = Task.Run(async () =>
            {
                data = await _cif.LoadCif(masterLocations, tocs, token);
            });

            if (Task.WaitAll(new Task[] {updateLocationsTask, cifTask}, Timeout))
                return data;
            
            return new Data();
        }

        internal async Task<TocLookup> LoadTocsAsync(CancellationToken token)
        {
            var tocs = new TocLookup(_logger);
            tocs = await _darwin.UpdateTocsAsync(tocs,  token).ConfigureAwait(false);
            tocs = await _knowledgebase.UpdateTocsAsync(tocs,  token).ConfigureAwait(false);
            return tocs;
        }

        internal async Task<ILocationData> LoadLocationsAsync(TocLookup lookup, CancellationToken token)
        {
            return await _cif.LoadStationMasterListAsync(token).ConfigureAwait(false);
        }
        
        internal async Task<ILocationData> UpdateLocationsAsync(ILocationData stations, TocLookup lookup, CancellationToken token)
        {
            stations = await _darwin.UpdateLocationsAsync(stations, lookup, token).ConfigureAwait(false);
            stations = await _knowledgebase.UpdateLocationsWithKnowledgebaseStationsAsync(stations, lookup, token).ConfigureAwait(false);
            return stations;
        }
    }
}