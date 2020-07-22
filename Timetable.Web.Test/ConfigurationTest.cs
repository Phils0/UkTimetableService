using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Timetable.Web.Test
{
    public class ConfigurationTest
    {
        
        public static TheoryData<string, bool> BooleanConfigCheck =>
            new TheoryData<string, bool>()
            {
                {"true", true},
                {"True", true},
                {"TRUE", true},
                {"false", false},
                {"False", false},
                {"FALSE", false},
                {"test", false},
                {"", false}
            };
        
        [Theory]
        [MemberData(nameof(BooleanConfigCheck))]
        public void EnableCustomPlugins(string configValue, bool expected)
        {
            var config = ConfigurationHelper.GetConfiguration(enableCustomPlugins: configValue);
            var pluginConfig = new Configuration(config);

            pluginConfig.EnableCustomPlugins.Should().Be(expected);
        }
        
        [Fact]
        public void EnableCustomPluginsIsFalseWhenNoConfigValue()
        {
            var config = Substitute.For<IConfigurationRoot>();;
            var pluginConfig = new Configuration(config);

            pluginConfig.EnableCustomPlugins.Should().BeFalse();
        }
        
        [Theory]
        [MemberData(nameof(BooleanConfigCheck))]
        public void EnablePrometheus(string configValue, bool expected)
        {
            var config = ConfigurationHelper.GetConfiguration(enablePrometheus: configValue);
            var pluginConfig = new Configuration(config);

            pluginConfig.EnablePrometheusMonitoring.Should().Be(expected);
        }
        
        [Fact]
        public void EnablePrometheusIsFalseWhenNoConfigValue()
        {
            var config = Substitute.For<IConfigurationRoot>();;
            var pluginConfig = new Configuration(config);

            pluginConfig.EnablePrometheusMonitoring.Should().BeFalse();
        }
        
        [Fact]
        public void GetPropertiesFromAppSettings()
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            
            var config = new Configuration(appSettings);
            
            Assert.Matches(new Regex("ttis144.zip$"),  config.TimetableArchiveFile);
        }
    }
}