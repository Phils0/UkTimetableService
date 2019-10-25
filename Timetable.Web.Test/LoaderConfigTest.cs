using System.Text.RegularExpressions;
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
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            
            var config = new LoaderConfig(appSettings);
            
            Assert.Matches(new Regex("ttis144.zip$"),  config.TimetableArchiveFile);
        }
    }
}