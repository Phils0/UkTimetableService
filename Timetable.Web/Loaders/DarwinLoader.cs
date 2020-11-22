using System;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient;
using DarwinClient.SchemaV16;
using Serilog;
using Timetable.Web.Mapping.Darwin;

namespace Timetable.Web.Loaders
{
    public class DarwinLoader : IDarwinLoader
    {
        private readonly DateTime? _date;
        private readonly ITimetableDownloader _darwinData;
        private readonly ILogger _logger;
        
        public DarwinLoader(ITimetableDownloader darwinData, ILogger logger, DateTime? date = null)
        {
            _darwinData = darwinData;
            _logger = logger;
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
        
        public async Task<ILocationData> UpdateLocationsAsync(ILocationData locations, TocLookup lookup, CancellationToken token)
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
                    _logger.Information("Darwin Location not loaded: {tpl} : {name}", location.tpl, location.locname);
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

        public Task<Data> AddReasonsAsync(Data data, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Data> AddDarwinTimetableAsync(Data data, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}