﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Loaders;
using Timetable.Web.Test.Mapping.Darwin;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class DarwinLoaderTest
    {
        private readonly static DateTime TestDate = new DateTime(2020, 11, 2);
        
        private DarwinLoader CreateLoader(ITimetableDownloader darwin, DateTime? date = null)
        {
            
            return new DarwinLoader(darwin, Substitute.For<ILogger>(), date);
        }
        
        private ITimetableDownloader MockDownloader
        {
            get
            {
                var darwin = Substitute.For<ITimetableDownloader>();
                darwin.GetLatestReference( Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Data.Darwin.Reference));
                darwin.GetReference(TestDate, Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Data.Darwin.Reference));
                return darwin;
            }            
        }
        
        [Fact]
        public async Task LoadTocs()
        {
            var loader = CreateLoader(darwin: MockDownloader);
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);
            
            Assert.NotEmpty(tocs);
        }
        
        [Fact]
        public async Task EnrichAddsTocs()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = TestData.Locations
            };
            var loader = CreateLoader(darwin: MockDownloader);

            data =  await loader.EnrichReferenceDataAsync(data, CancellationToken.None);

            Assert.NotEmpty(data.Tocs);
        }
        
        [Fact]
        public async Task UpdateLocations()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = TestData.Locations
            };
            var loader = CreateLoader(darwin: MockDownloader);

            data =  await loader.EnrichReferenceDataAsync(data, CancellationToken.None);

            data.Locations.TryGetStation("WAT", out Station waterloo);
            Assert.Equal("London Waterloo", waterloo.Name);
        }
        
        [Fact]
        public async Task AddViaRulesToStations()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = new Timetable.LocationData(
                    new[]
                    {
                        TestLocations.Surbiton,
                        TestLocations.WaterlooMain,
                        TestLocations.WaterlooWindsor,
                        TestLocations.CLPHMJN,
                        TestLocations.CLPHMJC,
                        TestLocations.Woking,
                        TestLocations.Guildford
                    }, Substitute.For<ILogger>(),
                    Timetable.Test.Data.Filters.Instance)
            };
            var loader = CreateLoader(darwin: MockDownloader);

            data =  await loader.EnrichReferenceDataAsync(data, CancellationToken.None);

            data.Locations.TryGetStation("SUR", out Station surbiton);
            var rules = StationMapperTest.GetViaRules(surbiton);
            Assert.NotEmpty(rules);
        }
        
        [Fact]
        public async Task LoadCancellationReasons()
        {
            var loader = CreateLoader(darwin: MockDownloader);
            var data = await loader.AddReasonsAsync(new RealtimeData(),  CancellationToken.None);
            
            Assert.NotEmpty(data.CancelReasons);
            foreach (var reason in data.CancelReasons.Values)
            {
                Assert.Contains("cancelled", reason);
            }
        }
        
        [Fact]
        public async Task LoadLateRunningReasons()
        {
            var loader = CreateLoader(darwin: MockDownloader);
            var data = await loader.AddReasonsAsync(new RealtimeData(),  CancellationToken.None);
            
            Assert.NotEmpty(data.LateRunningReasons);
            foreach (var reason in data.LateRunningReasons.Values)
            {
                Assert.Contains("delayed", reason);
            }       
        }
        
        [Fact]
        public async Task EnrichAddsReasons()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = TestData.Locations
            };
            var loader = CreateLoader(darwin: MockDownloader);

            data =  await loader.EnrichReferenceDataAsync(data, CancellationToken.None);

            Assert.NotEmpty(data.Darwin.CancelReasons);
            Assert.NotEmpty(data.Darwin.LateRunningReasons);
        }
        
        [Fact]
        public async Task LoadDarwinSources()
        {
            var loader = CreateLoader(darwin: MockDownloader);
            var data = await loader.AddSourcesAsync(new RealtimeData(),  CancellationToken.None);
            
            Assert.NotEmpty(data.Sources);
            
            Assert.Equal("Southern Metropolitan", data.Sources["AM01"]);
            Assert.Equal("Southern Suburban  (Legacy)", data.Sources["AMO2"]);
            Assert.Equal("East Midlands", data.Sources["at07"]);
        }
        
        [Fact]
        public async Task EnrichAddsSources()
        {
            var data = new Timetable.Data()
            {
                Tocs = new TocLookup(Substitute.For<ILogger>()),
                Locations = TestData.Locations
            };
            var loader = CreateLoader(darwin: MockDownloader);

            data =  await loader.EnrichReferenceDataAsync(data, CancellationToken.None);

            Assert.NotEmpty(data.Darwin.Sources);
        }
        
        [Fact]
        public async Task NoDateGetsLatest()
        {
            var darwin = MockDownloader;
            var loader = CreateLoader(darwin: darwin);
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);

            await darwin.Received().GetLatestReference(Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task DateGetsSpecificSource()
        {
            var darwin = MockDownloader;
            var loader = CreateLoader(darwin: darwin, TestDate);
            var tocs = await loader.UpdateTocsAsync(new TocLookup(Substitute.For<ILogger>()),  CancellationToken.None);

            await darwin.Received().GetReference(TestDate, Arg.Any<CancellationToken>());
        }
    }
}