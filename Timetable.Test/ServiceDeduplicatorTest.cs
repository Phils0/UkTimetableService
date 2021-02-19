using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ServiceDeduplicatorTest
    {
        [Fact]
        public void TwoServicesCancelledFirstDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("Z12345")
            };
            
            var deduped = Deduplicate(services);

            Assert.Single(deduped);
            Assert.Equal(services[1], deduped[0]);
        }

        private static ResolvedService[] Deduplicate(ResolvedService[] services)
        {
            var d = new ServiceDeduplicator();
            return d.Filter(services).ToArray();
        }

        [Fact]
        public void TwoServicesCancelledSecondDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(),
                TestSchedules.CreateService("Z12345", isCancelled: true),
            };
            
            var deduped = Deduplicate(services);

            Assert.Single(deduped);
            Assert.Equal(services[0], deduped[0]);
        }
        
        [Fact]
        public void TwoServicesDifferentHeadcodeDoNotDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("X98765")
            };
            
            var deduped = Deduplicate(services);

            Assert.Equal(services, deduped);
        }
        
        [Fact]
        public void TwoServicesDifferentRsidDoNotDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(),
                TestSchedules.CreateService("Z12345", retailServiceId: "VT9999", isCancelled: true),
            };
            
            var deduped = Deduplicate(services);

            Assert.Equal(services, deduped);
        }
        
        [Fact]
        public void TwoServicesNotCancelledDoNotDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(),
                TestSchedules.CreateService("Z12345"),
            };
            
            var deduped = Deduplicate(services);

            Assert.Equal(services, deduped);            
        }
        
        [Fact]
        public void ThreeServicesDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("Z12345"),
                TestSchedules.CreateService("A12345", isCancelled: true)
            };
            
            var deduped = Deduplicate(services);

            Assert.Single(deduped);
            Assert.Equal(services[1], deduped[0]);
        }
        
        [Fact]
        public void ThreeServicesOnlyOneDedup()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("Z12345"),
                TestSchedules.CreateService("X98765", isCancelled: true)
            };
            
            var deduped = Deduplicate(services);

            Assert.Equal(2, deduped.Length);
            Assert.Contains(services[1], deduped);
            Assert.Contains(services[2], deduped);
        }
        
        [Fact]
        public void DedupMultipleDifferentServices()
        {
            ResolvedService[] services = {
                TestSchedules.CreateService("A98765"),
                TestSchedules.CreateService(isCancelled: true),
                TestSchedules.CreateService("Z12345"),
                TestSchedules.CreateService("X98765", isCancelled: true)
            };
            
            var deduped = Deduplicate(services);

            Assert.Equal(2, deduped.Length);
            Assert.Contains(services[0], deduped);
            Assert.Contains(services[2], deduped);
        }
    }
}