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
        
        internal Loader CreateDataLoader()
        {
            var extractor = new RdgZipExtractor(_logger);
            
            var factory = new TtisParserFactory(_logger);
            var parser = factory.CreateStationParser();            
    
            var loader = new DataLoader(extractor, parser, CreateMapper(), Configuration);
            return new Loader(loader);
        }
        
        internal IReference CreateReferenceService(Data data) => new ReferenceService(data);
    }
}