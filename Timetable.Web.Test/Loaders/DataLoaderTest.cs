using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CifParser;
using CifParser.Archives;
using NreKnowledgebase;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Web.Loaders;
using Timetable.Web.Mapping.Cif;
using Xunit;

namespace Timetable.Web.Test
{
    public class DataLoaderTest
    {
        private static readonly MapperConfiguration _mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<FromCifProfile>());
        
        private IArchive MockRdgArchive
        {
            get
            {
                var archive = Substitute.For<IArchive>();
                archive.IsRdgZip.Returns(true);
                return archive;
            }
        }

        private Web.Loaders.DataLoader CreateLoader(IArchive archive = null, 
            ICifParser cifParser = null, 
            IDataEnricher knowledgebase = null,
            IDataEnricher darwin = null)
        {
            archive = CreateMockArchive(archive, cifParser);
            darwin ??= new NopLoader();
            knowledgebase ??= new KnowledgebaseLoader(Substitute.For<IKnowledgebaseAsync>(), Substitute.For<ILogger>());
            
            return Factory.CreateLoader(archive, darwin, knowledgebase, Substitute.For<ILogger>()) as Web.Loaders.DataLoader;
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
        public async Task LoadTimetableSetsLocations()
        {
            var loader = CreateLoader();
            var data = await loader.LoadAsync(CancellationToken.None);

            var locationData = data.Locations;
            Assert.NotEmpty(locationData.Locations);
            Assert.NotEmpty(locationData.LocationsByTiploc);          
        }
            
        [Fact]
        public async Task LoadTimetableDataSetsNlcs()
        {
            var parser = Substitute.For<ICifParser>();
            parser.Read().Returns(new IRecord[]
            {
                Cif.TestCifLocations.Surbiton,
                Cif.TestCifLocations.WaterlooMain,
                Cif.TestCifLocations.WaterlooWindsor
            });
            
            var loader = CreateLoader(cifParser: parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var locationData = data.Locations;

            var location = locationData.LocationsByTiploc["SURBITN"];
            
            Assert.Equal("557100", location.Nlc);            
        }
        
        [Fact]
        public async Task LoadSchedulesSetsTimetableUidMap()
        {
            var parser = Substitute.For<ICifParser>();
            parser.Read().Returns(new IRecord[]
            {
                Cif.TestSchedules.Test
            });
            
            var loader = CreateLoader(cifParser: parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var services = data.Timetable;

            var schedule = services.GetScheduleByTimetableUid(Cif.TestSchedules.X12345, new DateTime(2019, 8, 1));
            
            Assert.NotNull(schedule.service);            
        }
        
        [Fact]
        public async Task OnlyLoadPassengerVisibleAssociations()
        {
            var parser = Substitute.For<ICifParser>();
            parser.Read().Returns(new IRecord[]
            {
                Cif.TestAssociations.CreateAssociation("X12345", "A12345", type: "P"),
                Cif.TestAssociations.CreateAssociation("X12345", "A67890", type: "O"),
                Cif.TestSchedules.CreateSchedule("X12345"),
                Cif.TestSchedules.CreateSchedule("A12345"),
                Cif.TestSchedules.CreateSchedule("A67890"),
            });
            
            var loader = CreateLoader(cifParser: parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var timetable = (TimetableData) data.Timetable;

            var x12345 = GetService(timetable, "X12345");
            Assert.Single(GetAssociations(x12345));            
            var a12345 = GetService(timetable, "A12345");
            Assert.Single(GetAssociations(a12345));            
            var a67890 = GetService(timetable, "A67890");
            Assert.Null(GetAssociations(a67890));            
        }
        
        [Fact]
        public async Task LoadCancelledPassengerAssociations()
        {
            var parser = Substitute.For<ICifParser>();
            parser.Read().Returns(new IRecord[]
            {
                Cif.TestAssociations.CreateAssociation("X12345", "A12345", type: "P"),
                Cif.TestAssociations.CreateAssociation("X12345", "A67890", CifParser.Records.StpIndicator.C, type: null),
                Cif.TestSchedules.CreateSchedule("X12345"),
                Cif.TestSchedules.CreateSchedule("A12345"),
                Cif.TestSchedules.CreateSchedule("A67890"),
            });
            
            var loader = CreateLoader(cifParser: parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var timetable = (TimetableData) data.Timetable;

            var x12345 = GetService(timetable, "X12345");
            Assert.Equal(2, GetAssociations(x12345).Count);            
            var a12345 = GetService(timetable, "A12345");
            Assert.Single(GetAssociations(a12345));            
            var a67890 = GetService(timetable, "A67890");
            Assert.Single(GetAssociations(a12345));            
        }
        
        private static IService GetService(TimetableData timetable, string timetableUid)
        {
            var services = (Dictionary<string, IService>) timetable.AsDynamic()._timetableUidMap.RealObject;
            return services[timetableUid];
        }
        
        private static Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> GetAssociations(IService service)
        {
            var associations = service.AsDynamic()._associations;
            return associations == null ? null : (Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>) associations.RealObject;
        }
        
        [Fact]
        public async Task LoadSchedulesSetsRetailServiceIdMap()
        {
            var parser = Substitute.For<ICifParser>();
            parser.Read().Returns(new IRecord[]
            {
                Cif.TestSchedules.Test
            });
            
            var loader = CreateLoader(cifParser: parser);

            var data = await loader.LoadAsync(CancellationToken.None);
            var services = data.Timetable;

            var schedule = services.GetScheduleByRetailServiceId(Cif.TestSchedules.SW1234, new DateTime(2019, 8, 1));
            
            Assert.NotEmpty(schedule.services);            
        }
        
        [Fact]
        public async Task SetArchiveName()
        {
            var loader = CreateLoader();
            var data = await loader.LoadAsync(CancellationToken.None);
            
            Assert.Equal("TestData.zip", data.Archive);
        }
                
        [Fact]
        public async Task LoadTimetableSetsTocs()
        {
            var loader = CreateLoader();
            var data = await loader.LoadAsync(CancellationToken.None);
            
            Assert.NotNull(data.Tocs);
        }
        
        [Fact]
        public async Task LoadKnowledgebaseTocsDoesNotOverwriteDarwin()
        {
            var loader = CreateLoader();
            var data = await loader.LoadAsync(CancellationToken.None);
            
            Assert.NotNull(data.Tocs);
        }
    }
}