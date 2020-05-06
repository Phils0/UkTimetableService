using Xunit;

namespace Timetable.Web.IntegrationTest
{
    [CollectionDefinition("Service")]
    public class ServiceCollection : ICollectionFixture<WebServiceFixture>
    {
    }
}