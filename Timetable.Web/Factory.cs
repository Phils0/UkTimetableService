using System;
using AutoMapper;
using CifParser;
using CifParser.Archives;
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
            return new DataLoader(Archive, CreateMapper(), _logger);
        }
    }
}