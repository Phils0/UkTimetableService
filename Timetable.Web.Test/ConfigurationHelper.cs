using System.IO;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Timetable.Web.Test
{
    internal static class ConfigurationHelper
    {
        internal static Configuration GetConfiguration(bool enablePrometheus, bool enableCustomPlugins = false)
        {
            return new Configuration(GetConfiguration(enableCustomPlugins.ToString(), enablePrometheus.ToString()));
        }
        internal static IConfigurationRoot GetConfiguration(string enableCustomPlugins = "false", string enablePrometheus = "true")
        {
            var appSettings = Substitute.For<IConfigurationRoot>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            appSettings["StationKnowledgebase"].Returns("teststations.xml");
            appSettings["TocKnowledgebase"].Returns("testtocs.xml");
            appSettings["EnableCustomPlugins"].Returns(enableCustomPlugins);
            appSettings["EnablePrometheusMonitoring"].Returns(enablePrometheus);
            
            return appSettings;
        }
    }
}