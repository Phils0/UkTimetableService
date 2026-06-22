using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Timetable.Web.IntegrationTest
{
    /// <summary>
    /// Boots a TestServer hosting the real <see cref="Startup"/> pinned to the prod CIF
    /// archive <c>RJTTF870.ZIP</c>, overriding the appsettings default. Used by the
    /// overnight-board-asymmetry repro so the test exercises the exact prod data that
    /// surfaced the bug.
    ///
    /// The archive must be present in <c>Timetable.Web/Data/</c> (copied to the test output
    /// by the project's CopyData target; the .ZIP itself is gitignored). When it is absent -
    /// e.g. in CI - the host is not built and <see cref="Available"/> is false, so the tests
    /// skip rather than fail. Host build blocks on the data load (see SetData), so by the time
    /// the constructor returns the timetable is fully loaded — assertions about a service being
    /// absent are therefore not load-timing artifacts.
    /// </summary>
    public class Rjttf870Fixture : IDisposable
    {
        public const string ArchiveFileName = "RJTTF870.ZIP";

        // Generous: the synchronous data load (SetData) blocks the host build for up to
        // DataLoader.Timeout (5 min); the wait here only covers the quick StartAsync after.
        public static TimeSpan Timeout = new TimeSpan(0, 0, 30);

        public ILogger Logger { get; }
        public IHost Host { get; private set; }

        /// <summary>True when the pinned archive is present and the host loaded; false otherwise (tests skip).</summary>
        public bool Available { get; }

        public Rjttf870Fixture(IMessageSink logging)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(logging, LogEventLevel.Verbose)
                .CreateLogger();

            Available = File.Exists(Path.Combine(AppContext.BaseDirectory, "Data", ArchiveFileName));
            if (!Available)
                return;

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(new string[0])
                .UseSerilog((context, config) =>
                {
                    config.WriteTo.TestOutput(logging);
                    config.Enrich.FromLogContext();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    // Added after the default sources, so it wins over appsettings.json
                    // (which pins RJTTF847.ZIP).
                    webBuilder.ConfigureAppConfiguration(config =>
                        config.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string?>("TimetableArchive", "RJTTF870.ZIP")
                        }));
                    webBuilder.UseStartup<Startup>();
                });

            var task = hostBuilder.StartAsync();
            if (!task.Wait(Timeout))
                throw new Exception("Failed to start Test Server");
            Host = task.Result;
        }

        public void Dispose()
        {
            if (Host == null)
                return;

            var cancellation = new CancellationTokenSource();
            var task = Host.StopAsync(cancellation.Token);
            if (!task.Wait(Timeout))
            {
                cancellation.Cancel();
                task.Wait(Timeout);
            }
            Host = null;
        }
    }

    [CollectionDefinition("Rjttf870")]
    public class Rjttf870Collection : ICollectionFixture<Rjttf870Fixture>
    {
    }
}
