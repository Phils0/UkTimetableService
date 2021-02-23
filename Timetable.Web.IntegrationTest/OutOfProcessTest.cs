using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Timetable.Web.IntegrationTest
{
    // public class OutOfProcessTest
    // {
    //     private readonly ITestOutputHelper output;
    //
    //     public OutOfProcessTest(ITestOutputHelper output)
    //     {
    //         this.output = output;
    //     }
    //     
    //     [Fact]
    //     public async void MakeLocationRequest()
    //     {
    //         var client = new HttpClient();
    //         var url = @"http://localhost:8484/api/reference/location?toc=TP";
    //         var response = await client.GetAsync(url);
    //         
    //         response.EnsureSuccessStatusCode();
    //         var responseString = await response.Content.ReadAsStringAsync();
    //         var locations = JsonConvert.DeserializeObject<Model.Station[]>(responseString);
    //         foreach (var station in locations)
    //         {
    //             output.WriteLine($"{station.ThreeLetterCode}   {station.Locations.First().Tiploc}  {station.Name}");
    //         }
    //     }
    //     
    //     [Fact]
    //     public async void MakeTocServicesRequest()
    //     {
    //         var client = new HttpClient();
    //         var url = @"http://localhost:8484/api/timetable/toc/TP/2021-02-16?includeStops=false";
    //         var response = await client.GetAsync(url);
    //         
    //         response.EnsureSuccessStatusCode();
    //         var responseString = await response.Content.ReadAsStringAsync();
    //         var services = JsonConvert.DeserializeObject<Model.ServiceSummary[]>(responseString);
    //         foreach (var service in services.OrderBy(s => s.NrsRetailServiceId).ThenBy(s => s.RetailServiceId).ThenBy(s => s.TimetableUid))
    //         {
    //             output.WriteLine($"{service.RetailServiceId} | {service.Origin.Location.ThreeLetterCode}-{service.Destination.Location.ThreeLetterCode} | {service.TimetableUid} | {service.IsCancelled}");
    //         }
    //     }
    // }
}