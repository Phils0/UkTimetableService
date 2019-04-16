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
        private const string TestArchive = "TestArchive.zip";

        private static readonly MapperConfiguration _mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());
        
        [Fact]
        public async Task LoadStations()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(true);
            config.TimetableArchiveFile.Returns(TestArchive);

            var loader = CreateLoader(config);

            var locations = await loader.LoadStationMasterListAsync(CancellationToken.None);
            
            Assert.Equal(3, locations.Count());
        }

        private DataLoader CreateLoader(ILoaderConfig config, IParser cifParser = null)
        {
            var reader = Substitute.For<TextReader>();
            var extractor = Substitute.For<IArchiveFileExtractor>();
            extractor.ExtractFile(TestArchive, RdgZipExtractor.StationExtension).Returns(reader);

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

            var loader = new DataLoader(extractor, cifParser, stationParser, _mapperConfig.CreateMapper(), config, logger);
            return loader;
        }

        [Fact]
        public async Task LoadStationsThrowsExceptionIfNotRdgArchive()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(false);
            config.TimetableArchiveFile.Returns(TestArchive);

            var loader = CreateLoader(config);
  
            var ex = await Assert.ThrowsAnyAsync<InvalidDataException>(() =>  loader.LoadStationMasterListAsync(CancellationToken.None));
            
            Assert.Contains(TestArchive , ex.Message);
        }
        
        [Fact]
        public async Task LoadTimetableSetsLocations()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(true);
            config.TimetableArchiveFile.Returns(TestArchive);

            var loader = CreateLoader(config);

            var data = await loader.LoadAsync(CancellationToken.None);

            var locationData = data.Locations;
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);          
        }
            
        [Fact]
        public async Task LoadTimetableDataSetsNlcs()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(true);
            config.TimetableArchiveFile.Returns(TestArchive);

            var parser = Substitute.For<IParser>();
            parser.Read(Arg.Any<TextReader>()).Returns(new IRecord[]
            {
                Cif.TestCifLocations.Surbiton,
                Cif.TestCifLocations.WaterlooMain,
                Cif.TestCifLocations.WaterlooWindsor
            });
            
            var loader = CreateLoader(config, parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;

            var location = locationData.LocationsByTiploc["SURBITN"];
            
            Assert.Equal("557100", location.Nlc);            
        }
        
        [Fact]
        public async Task LoadSchedules()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(true);
            config.TimetableArchiveFile.Returns(TestArchive);

            var parser = Substitute.For<IParser>();
            parser.Read(Arg.Any<TextReader>()).Returns(new IRecord[]
            {
                Cif.TestSchedules.Test
            });
            
            var loader = CreateLoader(config, parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var services = data.Services;

            var schedule = services.GetSchedule(Cif.TestSchedules.X12345, new DateTime(2019, 8, 1));
            
            Assert.NotNull(schedule);            
        }
    }
}