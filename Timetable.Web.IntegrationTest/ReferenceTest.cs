using System;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
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
            var url = $"/api/reference/location/";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<Model.Station[]>(responseString);
            locations.Should().NotBeEmpty("{url} should return values", url);
        }
        
        [Fact]
        public async void MakeTocRequest()
        {
            var client = Host.GetTestClient();
            var url = $"/api/reference/toc/";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var locations = JsonConvert.DeserializeObject<Model.Toc[]>(responseString);
            locations.Should().NotBeEmpty("{url} should return values", url);
        }
    }
}