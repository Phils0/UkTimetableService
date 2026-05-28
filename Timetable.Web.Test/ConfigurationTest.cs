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

        [Fact]
        public void GetStationGroupsFileFromAppSettings()
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["StationGroupsFile"].Returns("station-groups.json");

            var config = new Configuration(appSettings, Substitute.For<ILogger>());

            Assert.Matches(new Regex("station-groups.json$"), config.StationGroupsFile);
        }

        [Fact]
        public void StationGroupsFileIsNullWhenNoConfigValue()
        {
            var config = new Configuration(Substitute.For<IConfiguration>(), Substitute.For<ILogger>());

            Assert.Null(config.StationGroupsFile);
        }

        public static TheoryData<string, JourneyHeuristic> OptimisationStrategyCheck =>
            new TheoryData<string, JourneyHeuristic>()
            {
                {"Longest", JourneyHeuristic.Longest},
                {"Shortest", JourneyHeuristic.Shortest},
                {"shortest", JourneyHeuristic.Shortest},
                {"LONGEST", JourneyHeuristic.Longest},
            };

        [Theory]
        [MemberData(nameof(OptimisationStrategyCheck))]
        public void GetStationGroupOptimisationStrategyFromAppSettings(string configValue, JourneyHeuristic expected)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["StationGroupOptimisationStrategy"].Returns(configValue);

            var config = new Configuration(appSettings, Substitute.For<ILogger>());

            Assert.Equal(expected, config.StationGroupOptimisationStrategy);
        }

        public static TheoryData<string> OptimisationStrategyDefaultCheck =>
            new TheoryData<string>()
            {
                "nonsense",
                "",
                null,
                "42",  // numeric outside the enum range: TryParse accepts it as (JourneyHeuristic)42; the
                       // Enum.IsDefined guard must reject it so it doesn't silently fall into Shortest.
            };

        [Theory]
        [MemberData(nameof(OptimisationStrategyDefaultCheck))]
        public void StationGroupOptimisationStrategyDefaultsToLongestWhenMissingOrUnparseable(string configValue)
        {
            var appSettings = Substitute.For<IConfiguration>();
            appSettings["StationGroupOptimisationStrategy"].Returns(configValue);

            var config = new Configuration(appSettings, Substitute.For<ILogger>());

            Assert.Equal(JourneyHeuristic.Longest, config.StationGroupOptimisationStrategy);
        }
    }
}