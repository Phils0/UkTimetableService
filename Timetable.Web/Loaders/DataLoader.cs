using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Timetable.DataLoader;

namespace Timetable.Web.Loaders
{ 
    public class DataLoader : IDataLoader
    {    
        private readonly ICifLoader _cif;
        private readonly IKnowledgebaseEnhancer _knowledgebase;
        private readonly ILogger _logger;

        public DataLoader(ICifLoader cif, IKnowledgebaseEnhancer knowledgebase, ILogger logger)
        {
            _cif = cif;
            _knowledgebase = knowledgebase;
            _logger = logger;
        }
       
        public async Task<Data> LoadAsync(CancellationToken token)
        {
            var tocs = await _knowledgebase.UpdateTocsWithKnowledgebaseAsync(new TocLookup(_logger),  token);
            var masterLocations = await _cif.LoadStationMasterListAsync(token).ConfigureAwait(false);
            masterLocations = await _knowledgebase.UpdateLocationsWithKnowledgebaseStationsAsync(masterLocations, tocs, token).ConfigureAwait(false);
            return await _cif.LoadCif(masterLocations, tocs, token);
        }
    }
}