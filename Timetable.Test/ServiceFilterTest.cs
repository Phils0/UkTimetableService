using System;
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

            var filter = new ServiceFilter();
            var filtered = filter.Filter(found.services, false);

            Assert.Equal(FindStatus.NoServicesForLocation, filtered.status);
            Assert.Empty(filtered.services);
        }
        
        [Fact]
        public void FindDeparturesWhenAllCancelledAndReturningCancelled()
        {
            var data = TestData.CreateTimetabledLocations(cancelTimetable: true);
            var found = data.FindDepartures("SUR", Ten, GathererConfig.OneService);

            var filter = new ServiceFilter();
            var filtered = filter.Filter(found.services, true);
            
            Assert.Equal(FindStatus.Success, filtered.status);
            Assert.NotEmpty(filtered.services);
        }
    }
}