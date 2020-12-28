using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient;
using DarwinClient.SchemaV16;
using Serilog;
using Timetable.Web.Mapping.Darwin;

namespace Timetable.Web.Loaders
{
    public class DarwinLoader : IDataEnricher
    {
        private readonly DateTime? _date;
        private readonly ITimetableDownloader _darwinData;
        private readonly ILogger _logger;
        
        public DarwinLoader(ITimetableDownloader darwinData, ILogger logger, DateTime? date = null)
        {
            _darwinData = darwinData;
            _logger = logger.ForContext<DarwinLoader>();
            _date = date;
        }

        private PportTimetableRef _refData;

        internal async Task<bool> Initilise(CancellationToken token)
        {
            try
            {
                await GetReferenceData(token).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to initialise Darwin timetable");
                return false;
            }
        }
        
        private async Task<PportTimetableRef> GetReferenceData(CancellationToken token)
        {
                if (_refData == null)
                {
                    if(_date.HasValue)
                        _refData = await _darwinData.GetReference(_date.Value, token).ConfigureAwait(false);
                    else
                        _refData = await _darwinData.GetLatestReference(token).ConfigureAwait(false);
                }

                return _refData;
        }
        
        public async Task<ILocationData> EnrichLocationsAsync(ILocationData locations, TocLookup lookup, CancellationToken token)
        {
            var refData = await GetReferenceData(token).ConfigureAwait(false);
            var mapper = new StationMapper(lookup);
            
            foreach (var location in refData.LocationRef)
            {
                if (ShouldUpdate(location))
                {
                    try
                    {
                        if (locations.TryGetStation(location.crs, out var target))
                        {
                            mapper.Update(target, location);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(e, "Error updating station: {station} with Darwin data.", location.crs);
                    }
                }
                else
                    _logger.Debug("Darwin Location not loaded: {tpl} : {name}", location.tpl, location.locname);
            }

            return locations;
            
            bool ShouldUpdate(LocationRef location)
            {
                return !string.IsNullOrEmpty(location.crs);
            }
        }
        
        public async Task<TocLookup> UpdateTocsAsync(TocLookup tocs, CancellationToken token)
        {
            var refData = await GetReferenceData(token).ConfigureAwait(false);
            foreach (var toc in refData.TocRef)
            {
                var t = TocMapper.Map(toc);
                tocs.AddOrReplace(t.Code, t);
            }

            return tocs;
        }

        public async Task<RealtimeData> AddReasonsAsync(RealtimeData data, CancellationToken token)
        {
            var refData = await GetReferenceData(token).ConfigureAwait(false);
            data.CancelReasons = refData.CancellationReasons.ToDictionary(r => r.code, r => r.reasontext);
            data.LateRunningReasons = refData.LateRunningReasons.ToDictionary(r => r.code, r => r.reasontext);
            return data;
        }

        public async Task<RealtimeData> AddSourcesAsync(RealtimeData data, CancellationToken token)
        {
            var refData = await GetReferenceData(token).ConfigureAwait(false);
            data.Sources = refData.CISSource.ToDictionary(r => r.code, r => r.name);
            return data;
        }
        
        public async Task<Data> EnrichReferenceDataAsync(Data data, CancellationToken token)
        {
            var refData = await GetReferenceData(token).ConfigureAwait(false);
            data.Darwin = new RealtimeData()
            {
                Reference = refData.File
            };
            var tocLookup = data.Tocs as TocLookup;
            await UpdateTocsAsync(tocLookup, token).ConfigureAwait(false);
            await EnrichLocationsAsync(data.Locations, tocLookup, token).ConfigureAwait(false);
            await AddReasonsAsync(data.Darwin, token).ConfigureAwait(false);
            await AddSourcesAsync(data.Darwin, token).ConfigureAwait(false);
            return data;
        }

        public Task<Data> EnrichTimetableAsync(Data data, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}