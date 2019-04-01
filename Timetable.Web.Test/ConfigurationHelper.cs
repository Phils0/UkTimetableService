using System.IO;
using Microsoft.Extensions.Configuration;

namespace Timetable.Web.Test
{
    public static class ConfigurationHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string path)
        {
            var directory = new DirectoryInfo(path);
            
            return new ConfigurationBuilder()
                .SetBasePath(directory.FullName)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}