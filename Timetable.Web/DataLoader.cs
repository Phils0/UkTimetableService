using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser;
using CifParser.Archives;
using CifParser.Records;
using NreKnowledgebase;
using Serilog;
using Timetable.Web.Mapping.Knowledgebase;

namespace Timetable.Web
{ 
    public class DataLoader : IDataLoader
    {
        private const int LogAfter = 100000;
        
        private IMapper _mapper;
        private readonly IArchive _archive;
        private readonly IKnowledgebaseAsync _knowledgebase;
        private readonly ILogger _logger;

        public DataLoader(IArchive archive, IKnowledgebaseAsync knowledgebase, IMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _archive = archive;
            _knowledgebase = knowledgebase;
            _logger = logger;
        }

        public async Task<TocLookup> LoadKnowledgebaseTocsAsync(CancellationToken token)
        {
            var lookup = new TocLookup(_logger, new Dictionary<string, Toc>());
            
            try
            {
                var tocs = await _knowledgebase.GetTocs(token);
                foreach (var toc in tocs.TrainOperatingCompany)
                {
                    var t = TocMapper.Map(toc);
                    lookup.Add(toc.AtocCode, t);
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e,"Error loading Knowledgebase Tocs.");
            }
            
            return lookup;
        }

        public Task<IEnumerable<Location>> UpdateLocationsWithKnowledgebaseStationsAsync(IEnumerable<Location> locations, CancellationToken token)
        {
            throw new System.NotImplementedException();
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
            var tocs = await LoadKnowledgebaseTocsAsync(token);
            var masterLocations = await LoadStationMasterListAsync(token).ConfigureAwait(false);
            var data = new LocationData(masterLocations.ToArray(), _logger);
            return await LoadCif(data, tocs, token);
        }

        private async Task<Data> LoadCif(LocationData locations, TocLookup tocs, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                _logger.Information("Loading Cif timetable in {file}", _archive.FullName);
                var parser = _archive.CreateCifParser();
                var records = parser.Read();
                var data = Add(records, locations, tocs, _archive.FullName); 
                _logger.Information("Loaded timetable");
                return data;
            }, token).ConfigureAwait(false);
        }

        private Data Add(IEnumerable<IRecord> records, LocationData locations, TocLookup tocLookup, string archiveFile)
        {
            var timetable = new TimetableData(_logger);
            var associations = new List<Association>(6000);
            
            Timetable.Schedule MapSchedule(CifParser.Schedule schedule)
            {
                return _mapper.Map<CifParser.Schedule, Timetable.Schedule>(schedule, o =>
                {
                    o.Items.Add("Tocs", tocLookup);
                    o.Items.Add("Locations", locations);
                    o.Items.Add("Timetable", timetable);
                });
            }
            
            Timetable.Association MapAssociation(CifParser.Records.Association association)
            {
                return _mapper.Map<CifParser.Records.Association, Timetable.Association>(association, o =>
                {
                    o.Items.Add("Locations", locations);
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
                    case CifParser.Schedule schedule:
                        var s = MapSchedule(schedule);
                        break;
                    case CifParser.Records.Association association:
                        var a = MapAssociation(association);
                        associations.Add(a);
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

            var applied = timetable.AddAssociations(associations.Where(a => a.IsPassenger || a.IsCancelled()));
            _logger.Information("Applied Associations: {applied} of {Count}", applied, associations.Count);
            
            return new Data()
            {
                Archive = archiveFile,
                Locations = locations,
                Timetable = timetable
            };
        }
    }
}