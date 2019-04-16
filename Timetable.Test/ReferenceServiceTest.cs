using System.Threading;
using System.Threading.Tasks;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ReferenceServiceTest
    {
        [Fact]
        public async Task GetLocations()
        {
            var service = new ReferenceService(TestData.Locations);

            var locations = await service.GetLocationsAsync(CancellationToken.None);
            
            Assert.NotEmpty(locations);
        }
    }
}