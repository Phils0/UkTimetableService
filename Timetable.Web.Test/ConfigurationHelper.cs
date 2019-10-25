using System.IO;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Timetable.Web.Test
{
    public static class ConfigurationHelper
    {
        public static IConfigurationRoot GetConfiguration()
        {
            var appSettings = Substitute.For<IConfigurationRoot>();
            appSettings["TimetableArchive"].Returns("ttis144.zip");
            
            return appSettings;
        }
    }
}