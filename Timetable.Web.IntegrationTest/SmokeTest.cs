using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Timetable.Web.Model;
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

        // [InlineData("SHF", "MAN", "TP")]
        [Theory]
        [InlineData("BHM", "BRI", "XC")]
        [InlineData("EUS", "MAN", "VT")]
        [InlineData("LDS", "KGX", "GR")]
        [InlineData("PAD", "SWI", "GW")]
        [InlineData("INV", "EUS", "CS")]
        public async void MakeDeparturesRequest(string origin, string destination, string toc)
        {
            var client = Host.GetTestClient();
            var url =
                $"/api/Timetable/departures/{origin}/{TestDate}?to={destination}&fullDay=true&includeStops=false&toc={toc}";
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning("{toc} request failed. {0} should be successful: {1} Retrying including cancelled services", toc, url, response.StatusCode);
                (response, url) = await RetryIncludingCancelledServices(client, url);
            }
            
            var responseString = await response.Content.ReadAsStringAsync();
            var departures = JsonConvert.DeserializeObject<Model.FoundSummaryResponse>(responseString);
            departures.Services.Should().NotBeEmpty("{0} should return values", url);
        }
        
        [Theory]
        [InlineData("TP")]
        [InlineData("XC")]
        [InlineData("VT")]
        [InlineData("GR")]
        [InlineData("GW")]
        [InlineData("CS")]
        [InlineData("EM")]
        [InlineData("NT")]
        public async void TocHasRunningService(string toc)
        {
            var services = await MakeTocRequest(toc);
            services.Should().NotBeEmpty("{0} should return values", toc);
        }

        private async Task<ServiceSummary[]> MakeTocRequest(string toc)
        {
            var url = $"/api/Timetable/toc/{toc}/{TestDate}?includeStops=false";
            Logger.Information("{toc}: Making request: {url}", toc, url);
            var client = Host.GetTestClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning("{0} request failed. {1} should be successful: {2} Retrying including cancelled services", toc,
                    url, response.StatusCode);
                (response, _) = await RetryIncludingCancelledServices(client, url);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.ServiceSummary[]>(responseString);
            return services;
        }

        private static async Task<(HttpResponseMessage, string)> RetryIncludingCancelledServices(HttpClient client, string url)
        {
            url = $"{url}&returnCancelledServices=true";
            var response = await client.GetAsync(url);
            response.IsSuccessStatusCode.Should().BeTrue("{0} should be successful: {1}", url, response.StatusCode);
            return (response, url);
        }

        [Theory]
        [InlineData("CS")]
        public async void MakeRetailServiceIdRequest(string toc)
        {
            var tocServices = await MakeTocRequest(toc);
            var rsid = tocServices[0].RetailServiceId;
            
            var client = Host.GetTestClient();
            var url = $"/api/Timetable/retailService/{rsid}/{TestDate}";
            var response = await client.GetAsync(url);
            
            response.IsSuccessStatusCode.Should().BeTrue("{0} should be successful: {1}", url, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.Service[]>(responseString);
            services.Should().NotBeEmpty("{0} should return values", url);
        }
        
        [Theory]
        [InlineData("CS")]
        public async void MakeRetailServiceIdRequestWithNrsRsid(string toc)
        {
            var tocServices = await MakeTocRequest(toc);
            var rsid = tocServices[0].NrsRetailServiceId;
            
            var client = Host.GetTestClient();
            var url = $"/api/Timetable/retailService/{rsid}/{TestDate}";
            var response = await client.GetAsync(url);
            
            response.IsSuccessStatusCode.Should().BeTrue("{0} should be successful: {1}", url, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.Service[]>(responseString);
            services.Should().NotBeEmpty("{0} should return values", url);
        }
        
        [Theory]
        [InlineData("XC", "1S47")]
        public async void TrainIdentityHasRunningService(string toc, string trainIdentity)
        {
            var services = await MakeTrainIdentityRequest(toc, trainIdentity);
            services.Should().NotBeEmpty("{0} should return values", toc);
        }

        private async Task<ServiceSummary[]> MakeTrainIdentityRequest(string toc, string trainIdentity)
        {
            var url = $"/api/Timetable/toc/{toc}/train/{trainIdentity}/{TestDate}?includeStops=false";
            Logger.Information("{toc}: Making request: {url}", toc, url);
            var client = Host.GetTestClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning("{0} request failed. {1} should be successful: {2} Retrying including cancelled services", toc,
                    url, response.StatusCode);
                (response, _) = await RetryIncludingCancelledServices(client, url);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<Model.ServiceSummary[]>(responseString);
            return services;
        }
    }
}