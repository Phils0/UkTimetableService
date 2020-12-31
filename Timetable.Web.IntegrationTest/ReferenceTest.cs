using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.IntegrationTest
{
    [Collection("Service")]
    public class ReferenceTest : ServiceTestBase
    {
        public ReferenceTest(WebServiceFixture fixture) : base(fixture)
        {
        }
        
        private bool HasDarwinReferenceFile => Directory.EnumerateFiles( "./Data", "*_ref_v3.*").Any();
        
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
            var tocs = JsonConvert.DeserializeObject<Model.Toc[]>(responseString);
            tocs.Should().NotBeEmpty("{url} should return values", url);
        }
        
        [Fact]
        public async void MakeCancelReasonsRequest()
        {
            var client = Host.GetTestClient();
            var url = $"/api/reference/reasons/cancellation/";
            var response = await client.GetAsync(url);

            if (HasDarwinReferenceFile)
            {
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var reasons = JsonConvert.DeserializeObject<Reason[]>(responseString);
                reasons.Should().NotBeEmpty("{url} should return values", url);                
            }
            else
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
        
        [Fact]
        public async void MakeLateReasonsRequest()
        {
            var client = Host.GetTestClient();
            var url = $"/api/reference/reasons/late/";
            var response = await client.GetAsync(url);
            
            if (HasDarwinReferenceFile)
            {            
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var reasons = JsonConvert.DeserializeObject<Reason[]>(responseString);
                reasons.Should().NotBeEmpty("{url} should return values", url);
            }
            else
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
        
        [Fact]
        public async void MakeDarwinSourcesRequest()
        {
            var client = Host.GetTestClient();
            var url = $"/api/reference/darwin/sources/";
            var response = await client.GetAsync(url);
            
            if (HasDarwinReferenceFile)
            {            
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var reasons = JsonConvert.DeserializeObject<DarwinSource[]>(responseString);
                reasons.Should().NotBeEmpty("{url} should return values", url);
            }
            else
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }
}