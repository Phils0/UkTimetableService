using System;
using System.Threading;
using System.Threading.Tasks;
using DarwinClient;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Loaders;
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