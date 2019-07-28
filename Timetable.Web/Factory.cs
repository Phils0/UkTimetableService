using System;
using AutoMapper;
using CifExtractor;
using CifParser;
using Microsoft.Extensions.Configuration;
using Serilog;
using Timetable.Web.Mapping;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Timetable.Web
{
    internal class Factory
    {
        internal static readonly MapperConfiguration MapperConfiguration = new MapperConfiguration(
            cfg =>
            {
                cfg.AddProfile<FromCifProfile>();
                cfg.AddProfile<ToViewModelProfile>();
            });
        
        private readonly IConfigurationProvider _mapperConfiguration;
        private readonly ILogger _logger;
        
        internal Factory(IConfigurationProvider mapperConfiguration, IConfiguration config, ILogger logger) :
            this(mapperConfiguration, new LoaderConfig(config), logger)
        {
        }
        
        internal Factory(IConfigurationProvider mapperConfiguration, ILoaderConfig config, ILogger logger)
        {
            _mapperConfiguration = mapperConfiguration;
            Archive = new Archive(config.TimetableArchiveFile, logger);
            _logger = logger;
        }

        internal IArchive Archive { get; }
        
        internal IMapper CreateMapper() => _mapperConfiguration.CreateMapper();
        
        internal IDataLoader CreateDataLoader()
        {
            var extractor = new RdgZipExtractor(Archive, _logger);
            return new DataLoader(extractor, CreateCifParser(), CreateStationParser(), CreateMapper(), Archive, _logger);
            
            IParser CreateCifParser()
            {
                var cifFactory = new ConsolidatorFactory(_logger);
                return cifFactory.CreateParser();
            }
            
            IParser CreateStationParser()
            {
                var stationParserFactory = new StationParserFactory(_logger);
                var ignoreLines = Archive.IsDtdZip
                    ? StationParserFactory.DtdIgnoreLines
                    : StationParserFactory.TtisIgnoreLines;
                return stationParserFactory.CreateParser(ignoreLines);
            }
        }
    }
}