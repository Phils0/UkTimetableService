using System;
using System.Linq;
using AutoMapper;
using Timetable.Test.Data;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedServiceWithAssociationsToServiceTest
    {
        private static readonly DateTime TestDate = TestTime.August1;
        
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

        private static Model.Service MapResolvedService(
            bool isCancelled = false,
            string mainUid = "X12345",
            string associatedUid = "A98765", 
            bool isNextDay = false)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            var resolved = TestSchedules.CreateServiceWithAssociation(on: TestDate, isCancelled, mainUid, associatedUid, isNextDay);
            
            var service = mapper.Map<Timetable.ResolvedService, Model.Service>(resolved, opts => opts.Items["On"] = resolved.On);
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
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapAssociationWhereMainServiceDoesNotHaveLocation(bool isCancelled)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var newStops = TestSchedules.DefaultLocations
                .Where(l => !l.Station.Equals(TestStations.ClaphamJunction))
                .ToArray();
            
            var mainService = TestSchedules.CreateService(stops: newStops);
            var resolved = TestSchedules.CreateServiceWithAssociation(mainService, isCancelled);
            
            var output = 
                mapper.Map<Timetable.ResolvedService, Model.Service>(resolved, opts => opts.Items["On"] = resolved.On);
            
            Assert.True(output.Associations[0].IsBroken);
            Assert.Null(output.Associations[0].Stop);
            Assert.Equal(isCancelled, output.Associations[0].IsCancelled);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapAssociationWhereAssociatedServiceDoesNotHaveLocation(bool isCancelled)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            var resolved = TestSchedules.CreateServiceWithAssociation(@on: TestDate, isCancelled, "X12345", "A98765", false);

            var association = resolved.Associations[0];

            var newStops = TestSchedules.CreateWokingClaphamSchedule(TestSchedules.NineForty)
                    .Where(l => !l.Station.Equals(TestStations.ClaphamJunction))
                    .ToArray();
            var newService = TestSchedules.CreateScheduleWithService("A98765", stops: newStops);
            var newAssociation = new ResolvedAssociation(
                    association.Details, 
                    association.On, 
                    association.IsCancelled, 
                    new ResolvedService(newService, association.On, false));
            resolved = new ResolvedServiceWithAssociations(resolved, new [] { newAssociation });
            
            var output = 
                    mapper.Map<Timetable.ResolvedService, Model.Service>(resolved, opts => opts.Items["On"] = resolved.On);
            
            Assert.True(output.Associations[0].IsBroken);
            Assert.Null(output.Associations[0].AssociatedServiceStop);
            Assert.Equal(isCancelled, output.Associations[0].IsCancelled);
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