using Microsoft.Extensions.Configuration;
using NSubstitute;
using Serilog;

namespace Timetable.Web.Test
{
    internal static class ConfigurationHelper
    {
        internal const string DarwinDate = "2020-04-29";
        
        internal static Configuration GetConfiguration(bool enablePrometheus, bool enableCustomPlugins = false, bool setDarwinDate = false)
        {
            return new Configuration(
                GetConfiguration(
                    enableCustomPlugins.ToString(), 
                    enablePrometheus.ToString(), 
                    setDarwinDate ? DarwinDate : null), 
                    Substitute.For<ILogger>());
        }
        internal static IConfigurationRoot GetConfiguration(
            string enableCustomPlugins = "false", string enablePrometheus = "true", string darwinDate = null)
        {
            var appSettings = Substitute.For<IConfigurationRoot>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            appSettings["StationKnowledgebase"].Returns("teststations.xml");
            appSettings["TocKnowledgebase"].Returns("testtocs.xml");
            if(!string.IsNullOrEmpty(darwinDate))
                appSettings["DarwinDate"].Returns(darwinDate);
            appSettings["EnableCustomPlugins"].Returns(enableCustomPlugins);
            appSettings["EnablePrometheusMonitoring"].Returns(enablePrometheus);
            
            return appSettings;
        }
    }
}