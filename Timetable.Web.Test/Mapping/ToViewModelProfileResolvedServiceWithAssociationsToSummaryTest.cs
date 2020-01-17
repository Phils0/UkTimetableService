using System;
using AutoMapper;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedServiceWithAssociationsToSummaryTest
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

        private static Model.ServiceSummary MapResolvedService(
            bool isCancelled = false,
            string mainUid = "X12345",
            string associatedUid = "A98765", 
            bool isNextDay = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            var resolved = TestSchedules.CreateServiceWithAssociation(on: TestDate, isCancelled, mainUid, associatedUid, isNextDay);
            
            var service = mapper.Map<Timetable.ResolvedService, Model.ServiceSummary>(resolved, opts => opts.Items["On"] = resolved.On);
            return service;
        }
        
        [Theory]
        [InlineData("X12345", "A98765", true)]
        [InlineData("A98765", "X12345", false)]
        public void MapAssociationIsMain(string mainUid, string associatedUid, bool expected)
        {
            var output = MapResolvedService(false, mainUid, associatedUid);
            var association = output.Associations[0];
            Assert.Equal(expected, association.IsMain);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapAssociationIsCancelled(bool isCancelled)
        {
            var output = MapResolvedService(isCancelled);
            var association = output.Associations[0];
            Assert.Equal(isCancelled, association.IsCancelled);
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
            Assert.IsType<Model.ServiceSummary>(association);
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
        
        [Fact]
        public void MapAssociationIsNextDay()
        {
            var output = MapResolvedService(isNextDay: true);
            
            Assert.Equal(TestDate, output.Date);
            
            var association = output.Associations[0];
            Assert.Equal(TestDate, association.Date);
            Assert.Equal(TestDate.Add(TestSchedules.TenFifteen.Value), association.Stop.Arrival);
            Assert.Equal(TestDate.AddDays(1), association.AssociatedService.Date);
            Assert.Equal(TestDate.AddDays(1).Add(TenTen), association.AssociatedServiceStop.Arrival);
        }
    }
}