using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;

namespace Timetable.Web.IntegrationTest
{
    [Collection("Service")]
    public class SmokeTest : ServiceTestBase
    {
        private string TestDate => DateTime.Today.NextWednesday().ToString("yyyy-MM-dd");

        public SmokeTest(WebServiceFixture fixture) : base(fixture)
        {
        }

        [Theory]
        [InlineData("SHF", "MAN", "TP")]
        [InlineData("LDS", "EDB", "XC")]
        [InlineData("EUS", "MAN", "VT")]
        [InlineData("LDS", "KGX", "GR")]
        [InlineData("PAD", "BRI", "GW")]
        [InlineData("GLC", "EUS", "CS")]
        public async void MakeDeparturesRequest(string origin, string destination, string toc)
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/Timetable/departures/{origin}/{TestDate}?to={destination}&fullDay=true&includeStops=false&toc={toc}";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var departures = JsonConvert.DeserializeObject<Model.FoundSummaryResponse>(responseString);
            Assert.NotEmpty(departures.Services);
        }
        
        [Theory]
        [InlineData("TP")]
        [InlineData("XC")]
        [InlineData("VT")]
        [InlineData("GR")]
        [InlineData("GW")]
        [InlineData("CS")]
        public async void MakeTocRequest(string toc)
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/Timetable/toc/{toc}/{TestDate}?includeStops=false&toc=";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.ServiceSummary[]>(responseString);
            Assert.NotEmpty(services);
        }
        
        [Theory]
        [InlineData("CS1002")]
        public async void MakeRetailServiceIdRequest(string rsid)
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/Timetable/retailService/{rsid}/{TestDate}";
            var response = await client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.Service[]>(responseString);
            Assert.NotEmpty(services);
        }
    }
}