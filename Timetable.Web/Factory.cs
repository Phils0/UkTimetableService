using System;
using System.Collections.Generic;
using AutoMapper;
using CifParser.Archives;
using NreKnowledgebase;
using Serilog;
using Timetable.DataLoader;
using Timetable.Web.Loaders;
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

        internal static IKnowledgebaseAsync CreateKnowledgebase(Configuration config, ILogger logger)
        {
            var muteErrors = new DowngradeErrorLogger(logger);
            var source = new FileSource(new Dictionary<KnowedgebaseSubjects, string>()
            {
                {KnowedgebaseSubjects.Stations, config.StationsKnowledgebaseFile},
                {KnowedgebaseSubjects.Tocs, config.TocsKnowledgebaseFile}
            }, muteErrors);
            return new Knowledgebase(source, muteErrors);
        }
        
        internal static IDataLoader CreateLoader(Configuration config, ILogger logger)
        {
            var archive = CreateArchive(config, logger);
            var knowledgebase = CreateKnowledgebase(config, logger);
            return CreateLoader(archive, knowledgebase, logger);
        }
        
        internal static IDataLoader CreateLoader(IArchive archive, IKnowledgebaseAsync knowledgebase, ILogger logger)
        {
            var cifLoader = CreateCifLoader(archive, logger);
            var knowledgebaseLoader = new KnowledgebaseLoader(knowledgebase, logger);
            return new Loaders.DataLoader(cifLoader, knowledgebaseLoader, logger);
        }

        
        internal static CifLoader CreateCifLoader(IArchive archive, ILogger logger)
        {
            var mapperConfig = new MapperConfiguration(
                cfg => { cfg.AddProfile<FromCifProfile>(); });
            var cifLoader = new CifLoader(archive, mapperConfig.CreateMapper(), logger);
            return cifLoader;
        }
    }
}