using System.Threading.Tasks;
using NSubstitute;
using Serilog;
using Timetable.Web.Loaders;
using Xunit;

namespace Timetable.Web.Test
{
    public class FactoryTest
    {
        [Fact]
        public async Task ReturnsDarwinLoaderWhenHasDarwinFiles()
        {
            var loader = await CreateDarwinLoader(null);
            Assert.IsType<DarwinLoader>(loader);
        }
        
        private static async Task<IDataEnricher> CreateDarwinLoader(string darwinDate)
        {
            var config = new Configuration(ConfigurationHelper.GetConfiguration(darwinDate: darwinDate), Substitute.For<ILogger>());
            return await Factory.CreateDarwinLoader(config, Substitute.For<ILogger>());;
        }
        
        [Fact]
        public async Task ReturnsNopLoaderWhenHasNoFiles()
        {
            var loader = await CreateDarwinLoader("2020-01-01");
            Assert.IsType<NopLoader>(loader);            
        }
    }
}