using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Serilog;
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
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnableCustomPlugins.Should().Be(expected);
        }
        
        [Fact]
        public void EnableCustomPluginsIsFalseWhenNoConfigValue()
        {
            var config = Substitute.For<IConfigurationRoot>();;
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnableCustomPlugins.Should().BeFalse();
        }
        
        [Theory]
        [MemberData(nameof(BooleanConfigCheck))]
        public void EnablePrometheus(string configValue, bool expected)
        {
            var config = ConfigurationHelper.GetConfiguration(enablePrometheus: configValue);
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnablePrometheusMonitoring.Should().Be(expected);
        }
        
        [Fact]
        public void EnablePrometheusIsFalseWhenNoConfigValue()
        {
            var config = Substitute.For<IConfigurationRoot>();;
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnablePrometheusMonitoring.Should().BeFalse();
        }
        
        [Theory]
        [MemberData(nameof(BooleanConfigCheck))]
        public void EnableDebugResponses(string configValue, bool expected)
        {
            var config = ConfigurationHelper.GetConfiguration(enableDebugResponses: configValue);
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnableDebugResponses.Should().Be(expected);
        }
        
        [Fact]
        public void EnableDebugResponsesIsFalseWhenNoConfigValue()
        {
            var config = Substitute.For<IConfigurationRoot>();;
            var pluginConfig = new Configuration(config, Substitute.For<ILogger>());

            pluginConfig.EnableDebugResponses.Should().BeFalse();
        }
        
        [Fact]
        public void GetTimetableFromAppSettings()
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            
            var config = new Configuration(appSettings, Substitute.For<ILogger>());
            
            Assert.Matches(new Regex("ttis144.zip$"),  config.TimetableArchiveFile);
        }
        
        public static TheoryData<string, DateTime?> DarwinDateCheck =>
            new TheoryData<string, DateTime?>()
            {
                {"2020-10-05", new DateTime(2020, 10 ,5)},
                {"INVALID", null},
                {"", null},
                {null, null},
            };
        
        [Theory]
        [MemberData(nameof(DarwinDateCheck))]
        public void GetDarwinFromAppSettings(string configValue, DateTime? expected)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["DarwinDate"].Returns(configValue);
            
            var config = new Configuration(appSettings, Substitute.For<ILogger>());
            
            Assert.Equal(expected,  config.DarwinDate);
        }
    }
}