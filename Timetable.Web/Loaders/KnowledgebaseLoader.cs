using System;
using System.Threading;
using System.Threading.Tasks;
using NreKnowledgebase;
using NreKnowledgebase.SchemaV4;
using Serilog;
using Timetable.Web.Mapping.Knowledgebase;

namespace Timetable.Web.Loaders
{
    public class KnowledgebaseLoader : IKnowledgebaseEnhancer
    {
        private readonly IKnowledgebaseAsync _knowledgebase;
        private readonly ILogger _logger;

        public KnowledgebaseLoader(IKnowledgebaseAsync knowledgebase, ILogger logger)
        {
            _knowledgebase = knowledgebase;
            _logger = logger;
        }

        public async Task<TocLookup> UpdateTocsAsync(TocLookup lookup, CancellationToken token)
        {
            TrainOperatingCompanyList tocs = null;
            try
            {
                tocs = await _knowledgebase.GetTocs(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Error loading Knowledgebase Tocs.");
            }

            if (tocs == null)
                return lookup;

            foreach (var toc in tocs.TrainOperatingCompany)
            {
                try
                {
                    var t = TocMapper.Map(toc);
                    lookup.AddIfNotExist(toc.AtocCode, t);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Error loading Knowledgebase Toc: {toc}.", toc.AtocCode);
                }
            }

            return lookup;
        }

        public async Task<ILocationData> UpdateLocationsWithKnowledgebaseStationsAsync(ILocationData locations,
            TocLookup lookup, CancellationToken token)
        {
            var mapper = new StationMapper(lookup);

            StationList stations = null;
            try
            {
                stations = await _knowledgebase.GetStations(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Error loading Knowledgebase Stations.");
            }

            if (stations == null)
                return locations;

            foreach (var station in stations.Station)
            {
                try
                {
                    if (locations.TryGetStation(station.CrsCode, out var target))
                    {
                        mapper.Update(target, station);
                    }
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Error updating station: {station} with knowledgebase.", station.CrsCode);
                }
            }

            return locations;
        }
    }
}