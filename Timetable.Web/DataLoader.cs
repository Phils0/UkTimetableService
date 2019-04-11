using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifExtractor;
using CifParser;
using CifParser.Records;
using Serilog;

namespace Timetable.Web
{
    public class DataLoader : IDataLoader
    {
        private const int LogAfter = 100000;

        private IArchiveFileExtractor _extractor;
        private readonly IParser _cifParser;
        private IParser _stationParser;
        private IMapper _mapper;
        private readonly ILoaderConfig _config;
        private readonly ILogger _logger;

        public DataLoader(IArchiveFileExtractor extractor, IParser cifParser, IParser stationParser, IMapper mapper,
            ILoaderConfig config, ILogger logger)
        {
            _extractor = extractor;
            _cifParser = cifParser;
            _stationParser = stationParser;
            _mapper = mapper;
            _config = config;
            _logger = logger;
        }

        public async Task<IEnumerable<Location>> LoadStationMasterListAsync(CancellationToken token)
        {
            if (!_config.IsRdgZip)
                throw new InvalidDataException($"Not an RDG archive. {_config.TimetableArchiveFile}");

            return await Task.Run(() =>
            {
                _logger.Information("Loading Master Station List in {file}", _config.TimetableArchiveFile);
                var reader = _extractor.ExtractFile(_config.TimetableArchiveFile, RdgZipExtractor.StationExtension);
                var stationRecords = _stationParser.Read(reader).OfType<CifParser.RdgRecords.Station>();
                var locations = _mapper.Map<IEnumerable<CifParser.RdgRecords.Station>, IEnumerable<Timetable.Location>>(
                    stationRecords);
                _logger.Information("Loaded Master Station List");
                return locations;
            }, token).ConfigureAwait(false);
        }

        public async Task<ILocationData> LoadAsync(CancellationToken token)
        {
            var masterLocations = await LoadStationMasterListAsync(token).ConfigureAwait(false);
            var data = new LocationData(masterLocations.ToArray(), _logger);
            return await LoadCif(data, token);
        }

        private async Task<LocationData> LoadCif(LocationData data, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                _logger.Information("Loading Cif timetable in {file}", _config.TimetableArchiveFile);
                var reader = _extractor.ExtractFile(_config.TimetableArchiveFile, RdgZipExtractor.CifExtension);
                var records = _cifParser.Read(reader);
                data = Add(records, data);
                _logger.Information("Loaded timetable");
                return data;
            }, token).ConfigureAwait(false);
        }

        private LocationData Add(IEnumerable<IRecord> records, LocationData data)
        {
            int count = 0;
            
            foreach (var record in records)
            {
                switch (record)
                {
                    case TiplocInsertAmend tiploc:
                        data.UpdateLocationNlc(tiploc.Code, tiploc.Nalco);
                        break;
                    case Association association:
                        break;
                    case Schedule schedule:
                        break;
                    default:
                        _logger.Warning("Unhandled record {recordType}: {record}", record.GetType(), record);
                        break;
                }

                count++;
                if(count % LogAfter == 0)
                    _logger.Debug("Loaded records: {count}", count);
            }

            return data;
        }
    }
}