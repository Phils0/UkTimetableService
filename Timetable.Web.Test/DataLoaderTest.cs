using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifExtractor;
using CifParser;
using NSubstitute;
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

            var reader = Substitute.For<TextReader>();
            var extractor = Substitute.For<IArchiveFileExtractor>();
            extractor.ExtractFile(TestArchive, RdgZipExtractor.StationExtension).Returns(reader);
            
            var parser = Substitute.For<IParser>();
            parser.Read(reader)
                .Returns(new []
                {
                    Cif.TestStations.Surbiton,
                    Cif.TestStations.WaterlooMain,
                    Cif.TestStations.WaterlooWindsor
                }); 
            
            var loader = new DataLoader(extractor, parser, _mapperConfig.CreateMapper(), config);

            var locations = await loader.GetStationMasterListAsync(CancellationToken.None);
            
            Assert.Equal(3, locations.Count());
        }
        
        [Fact]
        public async Task LoadStationsThrowsExceptionIfNotRdgArchive()
        {
            var config = Substitute.For<ILoaderConfig>();
            config.IsRdgZip.Returns(false);
            config.TimetableArchiveFile.Returns(TestArchive);

            var reader = Substitute.For<TextReader>();
            var extractor = Substitute.For<IArchiveFileExtractor>();
            extractor.ExtractFile(TestArchive, RdgZipExtractor.StationExtension).Returns(reader);
            
            var parser = Substitute.For<IParser>();
            parser.Read(reader)
                .Returns(new []
                {
                    Cif.TestStations.Surbiton,
                    Cif.TestStations.WaterlooMain,
                    Cif.TestStations.WaterlooWindsor
                }); 
            
            var loader = new DataLoader(extractor, parser, _mapperConfig.CreateMapper(), config);
  
            var ex = await Assert.ThrowsAnyAsync<InvalidDataException>(() =>  loader.GetStationMasterListAsync(CancellationToken.None));
            
            Assert.Contains(TestArchive , ex.Message);
        }
    }
}