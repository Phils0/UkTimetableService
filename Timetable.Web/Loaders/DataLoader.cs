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
        private readonly IDataEnricher _darwin;
        private readonly IDataEnricher _knowledgebase;
        private readonly ILogger _logger;

        public DataLoader(ICifLoader cif, IDataEnricher darwin, IDataEnricher knowledgebase, ILogger logger)
        {
            _cif = cif;
            _darwin = darwin;
            _knowledgebase = knowledgebase;
            _logger = logger;
        }
       
        public async Task<Data> LoadAsync(CancellationToken token)
        {
            var tocs = new TocLookup(_logger);
            Data data = new Data()
            {
                Tocs = tocs,
                Locations = await LoadLocationsAsync(tocs, token).ConfigureAwait(false)
            };

            var enrichRefDataTask = Task.Run(async () =>
            {
                data = await _darwin.EnrichReferenceDataAsync(data, token).ConfigureAwait(false);
                data = await _knowledgebase.EnrichReferenceDataAsync(data,  token).ConfigureAwait(false);
            });
            var cifTask = Task.Run(async () =>
            {
                data = await _cif.LoadCif(data.Locations, tocs, token).ConfigureAwait(false);
            });

            if (Task.WaitAll(new Task[] {enrichRefDataTask, cifTask}, Timeout))
                return data;
            
            return new Data();
        }

        internal async Task<ILocationData> LoadLocationsAsync(TocLookup lookup, CancellationToken token)
        {
            return await _cif.LoadStationMasterListAsync(token).ConfigureAwait(false);
        }
    }
}