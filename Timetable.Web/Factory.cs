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
      
        internal ILoaderConfig Configuration { get; }

        internal Factory(IConfigurationProvider mapperConfiguration, IConfiguration config, ILogger logger) :
            this(mapperConfiguration, new LoaderConfig(config), logger)
        {
        }
        
        internal Factory(IConfigurationProvider mapperConfiguration, ILoaderConfig config, ILogger logger)
        {
            _mapperConfiguration = mapperConfiguration;
            Configuration = config;
            _logger = logger;
        }

        internal IMapper CreateMapper() => _mapperConfiguration.CreateMapper();
        
        internal IDataLoader CreateDataLoader()
        {
            var extractor = new RdgZipExtractor(_logger);
            
            var cifFactory = new ConsolidatorFactory(_logger);
            var cifParser = cifFactory.CreateParser();
            
            var ttisFactory = new StationParserFactory(_logger);
            var ignoreLines = Configuration.IsDtdZip
                ? StationParserFactory.DtdIgnoreLines
                : StationParserFactory.TtisIgnoreLines;
            var stationParser = ttisFactory.CreateStationParser(ignoreLines);         
    
            return new DataLoader(extractor, cifParser, stationParser, CreateMapper(), Configuration, _logger);
        }
    }
}