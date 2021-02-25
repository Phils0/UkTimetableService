using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ServiceCancelledFilterTest
    {
        [Fact]
        public void RemovedCancelledService()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
            };
            
            var notCancelled = FilterCancelled(services);

            Assert.Empty(notCancelled);
        }

        private static ResolvedService[] FilterCancelled(ResolvedService[] services)
        {
            var d = new ServiceCancelledFilter();
            return d.Filter(services).ToArray();
        }
        
        [Fact]
        public void MultipleServicesCancelled()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("Z12345"),
                TestSchedules.CreateService("A12345", isCancelled: true)
            };
            
            var notCancelled = FilterCancelled(services);

            Assert.Single(notCancelled);
            Assert.Equal(services[1], notCancelled[0]);
        }
        
        [Fact]
        public void MultipleArrivalStopsCancelled()
        {
            ResolvedServiceStop[] services = {
                TestSchedules.CreateResolvedArrivalStop("A98765"),
                TestSchedules.CreateResolvedArrivalStop(isCancelled: true),
                TestSchedules.CreateResolvedArrivalStop("Z12345"),
                TestSchedules.CreateResolvedArrivalStop("X98765", isCancelled: true)
            };
            
            var notCancelled = FilterCancelled(services);

            Assert.Equal(2, notCancelled.Length);
            Assert.Contains(services[0], notCancelled);
            Assert.Contains(services[2], notCancelled);
        }
        
        private static ResolvedServiceStop[] FilterCancelled(ResolvedServiceStop[] services)
        {
            var d = new ServiceCancelledFilter();
            return d.Filter(services).ToArray();
        }
        
        [Fact]
        public void MultipleDepartureStopsCancelled()
        {
            ResolvedServiceStop[] services = {
                TestSchedules.CreateResolvedDepartureStop("A98765"),
                TestSchedules.CreateResolvedDepartureStop(isCancelled: true),
                TestSchedules.CreateResolvedDepartureStop("Z12345"),
                TestSchedules.CreateResolvedDepartureStop("X98765", isCancelled: true)
            };
            
            var notCancelled = FilterCancelled(services);

            Assert.Equal(2, notCancelled.Length);
            Assert.Contains(services[0], notCancelled);
            Assert.Contains(services[2], notCancelled);
        }
    }
}