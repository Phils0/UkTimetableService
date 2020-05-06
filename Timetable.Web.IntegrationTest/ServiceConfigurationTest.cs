using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Timetable.Web.IntegrationTest
{
    [Collection("Service")]
    public class ServiceConfigurationTest : ServiceTestBase
    {
        public ServiceConfigurationTest(WebServiceFixture fixture) : base(fixture)
        {
        }
        
        [Fact]
        public async void MakeServiceConfigurationRequest()
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/ServiceConfiguration/";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var serviceConfig = JsonConvert.DeserializeObject<Model.Configuration>(responseString);
            
            var fullTimetableRegex = new Regex(@"RJTTF\d{3}\.zip");
            Assert.Matches(fullTimetableRegex, serviceConfig.Data);
        }
    }
}