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

namespace Timetable.Web.Loaders
{
    public class CifLoader : ICifLoader
    {
        private const int LogAfter = 100000;

        private IMapper _mapper;
        private readonly IArchive _archive;
        private readonly ILogger _logger;
        private readonly ServiceFilters _filters;

        public CifLoader(IArchive archive, IMapper mapper, ILogger logger, ServiceFilters filters)
        {
            _mapper = mapper;
            _archive = archive;
            _logger = logger;
            _filters = filters;
        }

        public string ArchiveFile => _archive.FullName;

        public async Task<ILocationData> LoadStationMasterListAsync(CancellationToken token)
        {
            if (!_archive.IsRdgZip)
                throw new InvalidDataException($"Not an RDG archive. {_archive.FullName}");

            return await Task.Run(() =>
            {
                _logger.Information("Loading Master Station List in {file}", _archive.FullName);
                var parser = _archive.CreateParser();
                var stationRecords = parser.ReadFile(RdgZipExtractor.StationExtension)
                    .OfType<CifParser.RdgRecords.Station>();
                var locations =
                    _mapper.Map<IEnumerable<CifParser.RdgRecords.Station>, IEnumerable<Timetable.Location>>(
                        stationRecords);
                _logger.Information("Loaded Master Station List");
                return new LocationData(locations.ToArray(), _logger, _filters)
                {
                    IsLoaded = true
                };
            }, token).ConfigureAwait(false);
        }

        public async Task<Data> LoadCif(Data data, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                _logger.Information("Loading Cif timetable in {file}", _archive.FullName);
                var parser = _archive.CreateCifParser();
                var records = parser.Read();
                AddSchedules(records, data);
                _logger.Information("Loaded timetable");
                return data;
            }, token).ConfigureAwait(false);
        }

        private Data AddSchedules(IEnumerable<IRecord> records, Data data)
        {
            var tocLookup = data.Tocs as TocLookup;
            var locations = data.Locations;
            var timetable = new TimetableData(_filters, _logger);
            var associations = new List<Association>(6000);

            void UpdateLocations(TiplocInsertAmend cifTiploc)
            {
                var location = _mapper.Map<CifParser.Records.TiplocInsertAmend, Timetable.Location>(cifTiploc);
                locations.Update(location);
            }
            
            Timetable.CifSchedule MapSchedule(CifParser.Schedule schedule)
            {
                return _mapper.Map<CifParser.Schedule, Timetable.CifSchedule>(schedule, o =>
                {
                    o.Items.Add("Tocs", tocLookup);
                    o.Items.Add("Locations", locations);
                    o.Items.Add("Timetable", timetable);
                });
            }

            Timetable.Association MapAssociation(CifParser.Records.Association association)
            {
                return _mapper.Map<CifParser.Records.Association, Timetable.Association>(association,
                    o => { o.Items.Add("Locations", locations); });
            }

            var dodgySchedules = new List<ISchedule>();
            int count = 0;

            foreach (var record in records)
            {
                switch (record)
                {
                    case CifParser.Records.TiplocInsertAmend tiploc:
                        UpdateLocations(tiploc);
                        break;
                    case CifParser.Schedule schedule:
                        var s = MapSchedule(schedule);
                        if (!s.IsPublicSchedule())
                            dodgySchedules.Add(s);
                        break;
                    case CifParser.Records.Association association:
                        var a = MapAssociation(association);
                        associations.Add(a);
                        break;
                    case CifParser.Records.Header header:
                    case CifParser.Records.Trailer trailer:
                        _logger.Information("Ignored record {recordType}: {record}", record.GetType(), record);
                        break;
                    default:
                        _logger.Warning("Unhandled record {recordType}: {record}", record.GetType(), record);
                        break;
                }

                count++;
                if (count % LogAfter == 0)
                    _logger.Information("Loaded records: {count}", count);
            }

            if (dodgySchedules.Any())
            {
                _logger.Warning("Dodgy schedules loaded: {count} {schedules}", dodgySchedules.Count, dodgySchedules);
            }
            
            
            _logger.Information("Loaded records: {count}", count);

            var applied = timetable.AddAssociations(associations.Where(a => a.IsPassenger || a.IsCancelled()));
            _logger.Information("Applied Associations: {applied} of {Count}", applied, associations.Count);

            timetable.IsLoaded = true;
            data.Timetable = timetable;
            return data;
        }
    }
}