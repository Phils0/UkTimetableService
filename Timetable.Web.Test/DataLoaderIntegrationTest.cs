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
            
            var factory = new Factory(_mapperConfig, config, Substitute.For<ILogger>());
            var loader = factory.CreateDataLoader();

            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None);
            
            Assert.NotEmpty(locations);
        }
        
        [Fact]
        public async Task LoadCif()
        {
            var config = new LoaderConfig(ConfigurationHelper.GetIConfigurationRoot("."));
            
            var factory = new Factory(_mapperConfig, config, Substitute.For<ILogger>());
            var loader = factory.CreateDataLoader();

            var data = await loader.LoadAsync(CancellationToken.None);
            
            Assert.NotEmpty(data.Locations);
            Assert.NotEmpty(data.LocationsByTiploc);
        }
    }
}