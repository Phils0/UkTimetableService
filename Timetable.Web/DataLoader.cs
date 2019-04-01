using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifExtractor;
using CifParser;

namespace Timetable.Web
{
    public class DataLoader : IDataLoader
    {
        private IArchiveFileExtractor _extractor;
        private IParser _parser;
        private IMapper _mapper;
        private readonly ILoaderConfig _config;

        public DataLoader(IArchiveFileExtractor extractor, IParser parser, IMapper mapper, ILoaderConfig config)
        {
            _extractor = extractor;
            _parser = parser;
            _mapper = mapper;
            _config = config;
        }
        
        public async Task<IEnumerable<Location>> GetStationMasterListAsync(CancellationToken cancellationToken)
        {
            if(!_config.IsRdgZip)
                throw new InvalidDataException($"Not an RDG archive. {_config.TimetableArchiveFile}");
            
            return await Task.Run( () =>
            {
                var reader = _extractor.ExtractFile(_config.TimetableArchiveFile, RdgZipExtractor.StationExtension);
                var records = _parser.Read(reader);
                var stationRecords = records.OfType<CifParser.RdgRecords.Station>();
                return _mapper.
                    Map<IEnumerable<CifParser.RdgRecords.Station>, IEnumerable<Timetable.Location>>(stationRecords);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}