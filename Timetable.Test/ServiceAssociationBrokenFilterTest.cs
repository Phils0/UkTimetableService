using System.Linq;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ServiceAssociationBrokenFilterTest
    {
        [Fact]
        public void RemovedBrokenAssocation()
        {
            var service = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service.HasAssociations());
            service.Associations[0].AsDynamic().Stop = null;
            
            var notBroken = FilterBroken(new []{service});

            Assert.False(notBroken[0].HasAssociations());
        }

        private static ResolvedService[] FilterBroken(ResolvedService[] services)
        {
            var d = new ServiceAssociationBrokenFilter();
            return d.Filter(services).ToArray();
        }
        
        private static ResolvedServiceStop[] FilterBroken(ResolvedServiceStop[] services)
        {
            var d = new ServiceAssociationBrokenFilter();
            return d.Filter(services).ToArray();
        }
        
        [Fact]
        public void RemovedBrokenAssocationStops()
        {
            var service = TestSchedules.CreateServiceWithAssociation();
            Assert.True(service.HasAssociations());
            service.Associations[0].AsDynamic().Stop = null;
            var stop = TestSchedules.CreateResolvedDepartureStop(service);
            
            var notBroken = FilterBroken(new []{stop});

            Assert.False(notBroken[0].Service.HasAssociations());
        }
    }
}