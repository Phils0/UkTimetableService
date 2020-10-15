using System;
using System.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test
{
    public class DowngradeErrorLoggerTest
    {
        [Theory]
        [InlineData((LogEventLevel.Debug))]
        [InlineData((LogEventLevel.Information))]
        [InlineData((LogEventLevel.Warning))]
        public void UnchangedMessages(LogEventLevel level)
        {
            using (var log = new LogHelper())
            {
                using (TestCorrelator.CreateContext())
                {
                    ILogger downgradeLogger = new DowngradeErrorLogger(log.Logger);

                    var random = Guid.NewGuid();
                    var ex = new Exception("Test");
                    downgradeLogger.Write(level, ex,"{level} log: {random}", level, random);
                    var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().Single();
                    var message = logEvent.RenderMessage();
                    Assert.Equal(level, logEvent.Level);
                    Assert.Equal(ex, logEvent.Exception);
                    Assert.Contains($"{level.ToString()} log: {random.ToString()}", message);
                }
            }
        }
        
        [Theory]
        [InlineData((LogEventLevel.Error))]
        [InlineData((LogEventLevel.Fatal))]
        public void LoweredLevelMessages(LogEventLevel level)
        {
            using (var log = new LogHelper())
            {
                using (TestCorrelator.CreateContext())
                {
                    ILogger downgradeLogger = new DowngradeErrorLogger(log.Logger);

                    var random = Guid.NewGuid();
                    var ex = new Exception("Test");
                    downgradeLogger.Write(level, ex,"{level} log: {random}", level, random);
                    var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().Single();
                    var message = logEvent.RenderMessage();
                    Assert.Equal(LogEventLevel.Warning, logEvent.Level);
                    Assert.Equal(ex, logEvent.Exception);
                    Assert.Contains($"{level.ToString()} log: {random.ToString()}", message);
                }
            }
        }
    }
}