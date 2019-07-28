using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifExtractor;
using CifParser;
using NSubstitute;
using Serilog;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test
{
    public class DataLoaderTest
    {
        private static readonly MapperConfiguration _mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());
        
        [Fact]
        public async Task LoadStations()
        {
            var loader = CreateLoader(RdgArchive);
            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None);
            
            Assert.Equal(3, locations.Count());
        }

        private IArchive RdgArchive
        {
            get
            {
                var archive = Substitute.For<IArchive>();
                archive.IsRdgZip.Returns(true);
                return archive;
            }
        }

        private DataLoader CreateLoader(IArchive archive, IParser cifParser = null)
        {
            var reader = Substitute.For<TextReader>();
            var extractor = Substitute.For<IArchiveFileExtractor>();
            extractor.ExtractFile(RdgZipExtractor.StationExtension).Returns(reader);

            cifParser = cifParser ?? Substitute.For<IParser>();

            var stationParser = Substitute.For<IParser>();
            stationParser.Read(reader)
                .Returns(new[]
                {
                    Cif.TestStations.Surbiton,
                    Cif.TestStations.WaterlooMain,
                    Cif.TestStations.WaterlooWindsor
                });

            var logger = Substitute.For<ILogger>();

            var loader = new DataLoader(extractor, cifParser, stationParser, _mapperConfig.CreateMapper(), archive, logger);
            return loader;
        }

        [Fact]
        public async Task LoadStationsThrowsExceptionIfNotRdgArchive()
        {
            var archive = Substitute.For<IArchive>();
            archive.IsRdgZip.Returns(false);

            var loader = CreateLoader(archive);
  
            var ex = await Assert.ThrowsAnyAsync<InvalidDataException>(() =>  loader.LoadStationMasterListAsync(CancellationToken.None));
        }
        
        [Fact]
        public async Task LoadTimetableSetsLocations()
        {
            var loader = CreateLoader(RdgArchive);
            var data = await loader.LoadAsync(CancellationToken.None);

            var locationData = data.Locations;
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);          
        }
            
        [Fact]
        public async Task LoadTimetableDataSetsNlcs()
        {
            var parser = Substitute.For<IParser>();
            parser.Read(Arg.Any<TextReader>()).Returns(new IRecord[]
            {
                Cif.TestCifLocations.Surbiton,
                Cif.TestCifLocations.WaterlooMain,
                Cif.TestCifLocations.WaterlooWindsor
            });
            
            var loader = CreateLoader(RdgArchive, parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;

            var location = locationData.LocationsByTiploc["SURBITN"];
            
            Assert.Equal("557100", location.Nlc);            
        }
        
        [Fact]
        public async Task LoadSchedulesSetsTimetableUidMap()
        {
            var parser = Substitute.For<IParser>();
            parser.Read(Arg.Any<TextReader>()).Returns(new IRecord[]
            {
                Cif.TestSchedules.Test
            });
            
            var loader = CreateLoader(RdgArchive, parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var services = data.Timetable;

            var schedule = services.GetScheduleByTimetableUid(Cif.TestSchedules.X12345, new DateTime(2019, 8, 1));
            
            Assert.NotNull(schedule.schedule);            
        }
        
        [Fact]
        public async Task LoadSchedulesSetsRetailServiceIdMap()
        {
            var parser = Substitute.For<IParser>();
            parser.Read(Arg.Any<TextReader>()).Returns(new IRecord[]
            {
                Cif.TestSchedules.Test
            });
            
            var loader = CreateLoader(RdgArchive, parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var services = data.Timetable;

            var schedule = services.GetScheduleByRetailServiceId(Cif.TestSchedules.SW1234, new DateTime(2019, 8, 1));
            
            Assert.NotEmpty(schedule.schedule);            
        }
    }
}