using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser.Archives;
using DarwinClient;
using NreKnowledgebase;
using Serilog;
using Timetable.Web.Loaders;
using Timetable.Web.Mapping.Cif;
using FileSource = NreKnowledgebase.FileSource;

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

        internal static async Task<IDarwinLoader> CreateDarwinLoader(Configuration config, ILogger logger)
        {
            var muteErrors = new DowngradeErrorLogger(logger);
            var source = new DarwinClient.FileSource(
                new DirectoryInfo(Configuration.DataDirectory), muteErrors);
            var darwin = new TimetableDownloader(source, muteErrors);

            var darwinLoader = new DarwinLoader(darwin, logger, config.DarwinDate);
            if(await darwinLoader.Initilise(CancellationToken.None).ConfigureAwait(false))
                return darwinLoader;
            
            return new NopLoader();
        }
        
        internal static IKnowledgebaseEnhancer CreateKnowledgebase(Configuration config, ILogger logger)
        {
            var muteErrors = new DowngradeErrorLogger(logger);
            var source = new NreKnowledgebase.FileSource(new Dictionary<KnowedgebaseSubjects, string>()
            {
                {KnowedgebaseSubjects.Stations, config.StationsKnowledgebaseFile},
                {KnowedgebaseSubjects.Tocs, config.TocsKnowledgebaseFile}
            }, muteErrors);
            return new KnowledgebaseLoader(new Knowledgebase(source, muteErrors), logger);
        }
        
        internal static async Task<IDataLoader> CreateLoader(Configuration config, ILogger logger)
        {
            var archive = CreateArchive(config, logger);
            var darwin = await CreateDarwinLoader(config, logger);
            var knowledgebase = CreateKnowledgebase(config, logger);
            return CreateLoader(archive, darwin, knowledgebase, logger);
        }
        
        internal static IDataLoader CreateLoader(IArchive archive, IDarwinLoader darwin, IKnowledgebaseEnhancer knowledgebase, ILogger logger)
        {
            var cifLoader = CreateCifLoader(archive, logger);
            return new Loaders.DataLoader(cifLoader, darwin, knowledgebase, logger);
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