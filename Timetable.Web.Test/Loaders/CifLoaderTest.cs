using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CifParser;
using CifParser.Archives;
using NSubstitute;
using Serilog;
using Timetable.DataLoader;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class CifLoaderTest
    {
        [Fact]
        public async Task LoadStations()
        {
            var loader = CreateLoader(MockRdgArchive);
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None) as LocationData;

            Assert.True(locations.IsLoaded);
            Assert.Equal(3, locations.LocationsByTiploc.Count());
            Assert.Equal(2, locations.Locations.Count());
        }

        private IArchive MockRdgArchive
        {
            get
            {
                var archive = Substitute.For<IArchive>();
                archive.IsRdgZip.Returns(true);
                return archive;
            }
        }

        private ICifLoader CreateLoader(IArchive archive = null, ICifParser cifParser = null)
        {
            archive = CreateMockArchive(archive, cifParser);
            
            return Factory.CreateCifLoader(archive, Substitute.For<ILogger>());
        }
        
        private IArchive CreateMockArchive(IArchive archive, ICifParser cifParser)
        {
            archive = archive ?? MockRdgArchive;

            var stationParser = Substitute.For<IArchiveParser>();
            stationParser.ReadFile(RdgZipExtractor.StationExtension)
                .Returns(new[]
                {
                    Cif.TestStations.Surbiton,
                    Cif.TestStations.WaterlooMain,
                    Cif.TestStations.WaterlooWindsor
                });
            archive.CreateParser().Returns(stationParser);

            cifParser = cifParser ?? Substitute.For<ICifParser>();
            archive.CreateCifParser().Returns(cifParser);

            archive.FullName.Returns(@"TestData.zip");
            return archive;
        }
        
        [Fact]
        public async Task LoadStationsThrowsExceptionIfNotRdgArchive()
        {
            var archive = Substitute.For<IArchive>();
            archive.IsRdgZip.Returns(false);
            
            var loader = CreateLoader(archive);

            var ex = await Assert.ThrowsAnyAsync<InvalidDataException>(() =>
                loader.LoadStationMasterListAsync(CancellationToken.None));
        }
    }
}