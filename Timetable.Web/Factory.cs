using AutoMapper;
using CifParser.Archives;
using Serilog;
using Timetable.Web.Mapping.Cif;

namespace Timetable.Web
{
    internal static class Factory
    {
        internal static Model.Configuration CreateConfigurationResponse(string dataArchive)
        {
            var t = typeof(Factory);
            return new Model.Configuration()
            {
                // ReSharper disable once PossibleNullReferenceException
                Version = t.Assembly.GetName().Version.ToString(),
                Data = dataArchive
            };
        }

        internal static IArchive CreateArchive(Configuration config, ILogger logger)
        {
            return new Archive(config.TimetableArchiveFile, logger);
        }
        
        internal static IDataLoader CreateLoader(IArchive archive,ILogger logger)
        {
            var mapperConfig = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<FromCifProfile>();
                });
            return new DataLoader(archive, mapperConfig.CreateMapper(),logger);
        }
        
        internal static IDataLoader CreateLoader(Configuration config, ILogger logger)
        {
            var archive = CreateArchive(config, logger);
            return CreateLoader(archive, logger);
        }
    }
}