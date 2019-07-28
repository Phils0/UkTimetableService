using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Timetable.Web.Test
{
    public class LoaderConfigTest
    {
        [Fact]
        public void GetPropertiesFromAppSettings()
        {
            var config = new LoaderConfig(ConfigurationHelper.GetIConfigurationRoot("."));

            Assert.Contains("ttis144.zip", config.TimetableArchiveFile);
        }
    }
}