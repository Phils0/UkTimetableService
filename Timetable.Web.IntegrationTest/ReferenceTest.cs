using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;

namespace Timetable.Web.IntegrationTest
{
    [Collection("Service")]
    public class ReferenceTest : ServiceTestBase
    {
        public ReferenceTest(WebServiceFixture fixture) : base(fixture)
        {
        }
        
        [Fact]
        public async void MakeLocationRequest()
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/reference/location/";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<Model.Station[]>(responseString);
            Assert.NotEmpty(locations);
        }
    }
}