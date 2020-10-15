using System.Linq;
using Serilog;
using Serilog.Events;

namespace Timetable.Web
{
    /// <summary>
    /// Logger that downgrades Errors to Warnings
    /// </summary>
    internal class DowngradeErrorLogger : ILogger
    {
        private readonly ILogger _logger;

        internal DowngradeErrorLogger(ILogger logger)
        {
            _logger = logger;
        }
        public void Write(LogEvent logEvent)
        {
            if (logEvent.Level > LogEventLevel.Warning)
                logEvent = new LogEvent(
                    logEvent.Timestamp, 
                    LogEventLevel.Warning, 
                    logEvent.Exception, 
                    logEvent.MessageTemplate, 
                    logEvent.Properties.Select(k => new LogEventProperty(k.Key, k.Value)));
            
            _logger.Write(logEvent);
        }
    }
}