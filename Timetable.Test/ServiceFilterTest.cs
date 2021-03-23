using System;
using System.Linq;
using NSubstitute;
using ReflectionMagic;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ServiceFilterTest
    {
        private static readonly DateTime Ten = new DateTime(2019, 8, 12, 10, 0, 0);
        private static readonly DateTime Aug12 = Ten.Date;
                
        [Fact]
        public void DoNotFindDeparturesWhenAllCancelled()
        {
            var data = TestData.CreateTimetabledLocations(cancelTimetable: true);
            var found = data.FindDepartures("SUR", Ten, GathererConfig.OneService);

            var filter = CreateFilter();
            var filtered = filter.Filter(found.services, false);

            Assert.Equal(FindStatus.NoServicesForLocation, filtered.status);
            Assert.Empty(filtered.services);
        }

        private ServiceFilters CreateFilter()
        {
            return new ServiceFilters(false,Substitute.For<ILogger>());
        }

        [Fact]
        public void FindDeparturesWhenAllCancelledAndReturningCancelled()
        {
            var data = TestData.CreateTimetabledLocations(cancelTimetable: true);
            var found = data.FindDepartures("SUR", Ten, GathererConfig.OneService);

            var filter = CreateFilter();
            var filtered = filter.Filter(found.services, true);
            
            Assert.Equal(FindStatus.Success, filtered.status);
            Assert.NotEmpty(filtered.services);
        }
        
        [Fact]
        public void DeduplicateReturnsCancelled()
        {
            var data = TestData.CreateTimetabledLocations(cancelTimetable: true);
            var found = data.FindDepartures("SUR", Ten, GathererConfig.OneService);
            var services = found.services.Select(s => s.Service).ToArray();

            var filters = CreateFilter();
            var filtered = filters.Deduplicate(services);
            Assert.NotEmpty(filtered);
        }
        
        [Fact]
        public void BrokenAssociationsRemoved()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service1.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");

            var filters = CreateFilter();
            var filtered = filters.RemoveBrokenServices(new [] { service1, service2 });
            Assert.False(service1.HasAssociations());
        }
        
        
        [Fact]
        public void BrokenAssociationsNotRemovedIfReturningDebugResponses()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service1.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");

            var filters = new ServiceFilters(true,Substitute.For<ILogger>());
            var filtered = filters.RemoveBrokenServices(new [] { service1, service2 });
            Assert.True(service1.HasAssociations());
        }
        
        [Fact]
        public void DeduplicateAlsoRemovesBrokenAssociations()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service1.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");

            var filters = CreateFilter();
            var filtered = filters.Deduplicate(new [] { service1, service2 });
            Assert.False(service1.HasAssociations());
        }
        
        [Fact]
        public void FilterAlsoRemovesBrokenAssociations()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service1.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");

            var filters = CreateFilter();
            var filtered = filters.Filter(new [] { service1, service2 }, true);
            Assert.False(service1.HasAssociations());
        }
        
        [Fact]
        public void FilterStopsAlsoRemovesBrokenAssociations()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            var stop1 = TestSchedules.CreateResolvedDepartureStop(service1);
            Assert.True(stop1.Service.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");
            var stop2 = TestSchedules.CreateResolvedDepartureStop(service2);

            var filters = CreateFilter();
            var filtered = filters.Filter(new [] { stop1, stop2 }, true);
            Assert.False(stop1.Service.HasAssociations());
        }
        
        [Fact]
        public void BrokenAssociationsNotRemovedFromStopsIfReturningDebugResponses()
        {
            var service1 = TestSchedules.CreateServiceWithAssociation();
            var stop1 = TestSchedules.CreateResolvedDepartureStop(service1);
            Assert.True(stop1.Service.HasAssociations());
            service1.Associations[0].AsDynamic().Stop = null;
            var service2 = TestSchedules.CreateService("Z98765");
            var stop2 = TestSchedules.CreateResolvedDepartureStop(service2);

            var filters = new ServiceFilters(true,Substitute.For<ILogger>());
            var filtered = filters.Filter(new [] { stop1, stop2 }, true);
            Assert.True(stop1.Service.HasAssociations());
        }
    }
}