using Microsoft.Extensions.Configuration;
using Serilog;

namespace Timetable.Web
{
    internal static class Logging
    {
        internal static void Configure(IConfiguration configuration, LoggerConfiguration logConfig)
        {
            logConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext();
        }        
    }
}