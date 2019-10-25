using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser;
using CifParser.Archives;
using CifParser.Records;
using Serilog;

namespace Timetable.Web
{
    public class DataLoader : IDataLoader
    {
        private const int LogAfter = 100000;
        
        private IMapper _mapper;
        private readonly IArchive _archive;
        private readonly ILogger _logger;

        public DataLoader(IArchive archive, IMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _archive = archive;
            _logger = logger;
        }

        public async Task<IEnumerable<Location>> LoadStationMasterListAsync(CancellationToken token)
        {
            if (!_archive.IsRdgZip)
                throw new InvalidDataException($"Not an RDG archive. {_archive.FullName}");

            return await Task.Run(() =>
            {
                _logger.Information("Loading Master Station List in {file}", _archive.FullName);
                var parser = _archive.CreateParser();
                var stationRecords = parser.ReadFile(RdgZipExtractor.StationExtension).OfType<CifParser.RdgRecords.Station>();
                var locations = _mapper.Map<IEnumerable<CifParser.RdgRecords.Station>, IEnumerable<Timetable.Location>>(stationRecords);
                _logger.Information("Loaded Master Station List");
                return locations;
            }, token).ConfigureAwait(false);
        }

        public async Task<Data> LoadAsync(CancellationToken token)
        {
            var masterLocations = await LoadStationMasterListAsync(token).ConfigureAwait(false);
            var data = new LocationData(masterLocations.ToArray(), _logger);
            return await LoadCif(data, token);
        }

        private async Task<Data> LoadCif(LocationData locations, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                _logger.Information("Loading Cif timetable in {file}", _archive.FullName);
                var parser = _archive.CreateCifParser();
                var records = parser.Read();
                var data = Add(records, locations, _archive.FullName); 
                _logger.Information("Loaded timetable");
                return data;
            }, token).ConfigureAwait(false);
        }

        private Data Add(IEnumerable<IRecord> records, LocationData locations, string archiveFile)
        {
            var tocLookup = new TocLookup(_logger, new Dictionary<string, Toc>());
            var timetable = new TimetableData();
            
            Schedule MapSchedule(CifParser.Schedule schedule)
            {
                return _mapper.Map<CifParser.Schedule, Timetable.Schedule>(schedule, o =>
                {
                    o.Items.Add("Tocs", tocLookup);
                    o.Items.Add("Locations", locations);
                    o.Items.Add("Timetable", timetable);
                });
            }
            
            int count = 0;

            foreach (var record in records)
            {
                switch (record)
                {
                    case TiplocInsertAmend tiploc:
                        locations.UpdateLocationNlc(tiploc.Code, tiploc.Nalco);
                        break;
                    case Association association:
                        break;
                    case CifParser.Schedule schedule:
                        var s = MapSchedule(schedule);
                        break;
                    default:
                        _logger.Warning("Unhandled record {recordType}: {record}", record.GetType(), record);
                        break;
                }

                count++;
                if (count % LogAfter == 0)
                    _logger.Information("Loaded records: {count}", count);
            }

            _logger.Information("Loaded records: {count}", count);

            return new Data()
            {
                Archive = archiveFile,
                Locations = locations,
                Timetable = timetable
            };
        }
    }
}