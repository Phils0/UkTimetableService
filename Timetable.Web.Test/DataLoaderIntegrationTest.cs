using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifExtractor;
using CifParser;
using NSubstitute;
using Serilog;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test
{
    public class DataLoaderIntegrationTest
    {
        private static readonly MapperConfiguration _mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());
        
        [Fact]
        public async Task LoadStations()
        {
            var config = new LoaderConfig(ConfigurationHelper.GetIConfigurationRoot("."));
            var extractor = new RdgZipExtractor(Log.Logger);
            
            var factory = new TtisParserFactory(Log.Logger);
            var parser = factory.CreateStationParser();            
            
            var loader = new DataLoader(extractor, parser, _mapperConfig.CreateMapper(), config);

            var locations = await loader.GetStationMasterListAsync(CancellationToken.None);
            
            Assert.NotEmpty(locations);
        }
    }
}