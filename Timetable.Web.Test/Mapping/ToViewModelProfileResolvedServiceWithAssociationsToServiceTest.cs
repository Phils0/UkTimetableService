using System;
using AutoMapper;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedServiceWithAssociationsToServiceTest
    {
        private static readonly DateTime TestDate = Cif.TestTime.August1;
        
        private static readonly MapperConfiguration ToViewProfileConfiguration = 
            ToViewModelProfileLocationTest.ToViewProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }
        
        [Fact]
        public void MapTimetableId()
        {
            var output = MapResolvedService();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static Model.Service MapResolvedService(bool isCancelled = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            var resolved = TestSchedules.CreateServiceWithAssociation(on: TestDate);
            
            var service = mapper.Map<Timetable.ResolvedService, Model.Service>(resolved, opts => opts.Items["On"] = resolved.On);
            return service;
        }
        
        [Fact]
        public void MapAssociationIsCancelled()
        {
            var output = MapResolvedService();
            var association = output.Associations[0];
            Assert.False(association.IsCancelled);
        }
        
        [Fact]
        public void MapDate()
        {
            var output = MapResolvedService();
            var association = output.Associations[0];
            Assert.Equal(TestDate, association.Date);
        }
        
        [Fact]
        public void MapAssociationCategory()
        {
            var output = MapResolvedService();
            var association = output.Associations[0];
            Assert.Equal("Join", association.AssociationCategory);
        }
        
        [Fact]
        public void MapOtherService()
        {
            var output = MapResolvedService();
            var association = output.Associations[0].AssociatedService;
            Assert.IsType<Model.Service>(association);
            Assert.Equal("A98765", association.TimetableUid);
        }
        
        [Fact]
        public void MapServiceStop()
        {
            var output = MapResolvedService();
            var association = output.Associations[0].Stop;
            Assert.Equal("CLJ", association.Location.ThreeLetterCode);
            Assert.Equal(TestDate.Add(TestSchedules.TenFifteen.Value), association.Arrival);
        }
        
        public static TimeSpan TenTen => new TimeSpan(10, 10 ,0);
        
        [Fact]
        public void MapAssociationServiceStop()
        {
            var output = MapResolvedService();
            var association = output.Associations[0].AssociatedServiceStop;
            Assert.Equal("CLJ", association.Location.ThreeLetterCode);
            Assert.Equal(TestDate.Add(TenTen), association.Arrival);
        }
    }
}