using System;
using System.Diagnostics;
using TraceActivity =  System.Diagnostics.Activity;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace Timetable.Web.Test
{
    public class IntegrationTest
    {
        private const string _host = "http://localhost:8084";
        private string _testDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
        
        [Fact(Skip = "Manual Test")]
        // [Fact]
        public async void AddDistributedTracing()
        {
            // TraceActivity.DefaultIdFormat = ActivityIdFormat.W3C;
            var activity = new TraceActivity("CallToTimetable").Start();

            try
            {
                var url =
                    $"{_host}/api/Timetable/departures/LDS/{_testDate}?to=BHM&fullday=true&includeStops=false&toc=VT&toc=GW&toc=GR&toc=XC";
            
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Add("Origin", _host);

                var response = await client.GetStringAsync(url);
                var departures = JsonConvert.DeserializeObject<Model.FoundSummaryResponse>(response);
            
                Assert.True(departures.Services.Any());
            }
            finally
            {
                activity.Stop();
            }
        }
    }
}