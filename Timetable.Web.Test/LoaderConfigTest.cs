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
        
        [Theory]
        [InlineData("ttis144.zip", true)]
        [InlineData("RJTTF293.zip", true)]
        [InlineData("toc-update-tue.CIF.gz", false)]
        public void IsRdgFile(string file, bool expected)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns(file);
            
            var config = new LoaderConfig(appSettings);

            Assert.Equal(expected, config.IsRdgZip);
        }
        
        [Theory]
        [InlineData("ttis144.zip", true)]
        [InlineData("RJTTF293.zip", false)]
        [InlineData("toc-update-tue.CIF.gz", false)]
        public void IsTtisFile(string file, bool expected)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns(file);
            
            var config = new LoaderConfig(appSettings);

            Assert.Equal(expected, config.IsTtisZip);
        }
        
        [Theory]
        [InlineData("ttis144.zip", false)]
        [InlineData("RJTTF293.zip", true)]
        [InlineData("toc-update-tue.CIF.gz", false)]
        public void IsDtdFile(string file, bool expected)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns(file);
            
            var config = new LoaderConfig(appSettings);

            Assert.Equal(expected, config.IsDtdZip);
        }
    }
}