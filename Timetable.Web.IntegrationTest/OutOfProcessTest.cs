// using System;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using Newtonsoft.Json;
// using Timetable.Web.Model;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace Timetable.Web.IntegrationTest
// {
//     public class OutOfProcessTest
//     {
//         private readonly ITestOutputHelper output;
//     
//         public OutOfProcessTest(ITestOutputHelper output)
//         {
//             this.output = output;
//         }
//         
//         [Fact]
//         public async void MakeLocationRequest()
//         {
//             var client = new HttpClient();
//             var url = @"http://localhost:8484/api/reference/location?toc=GR";
//             var response = await client.GetAsync(url);
//             
//             response.EnsureSuccessStatusCode();
//             var responseString = await response.Content.ReadAsStringAsync();
//             var locations = JsonConvert.DeserializeObject<Model.Station[]>(responseString);
//             foreach (var station in locations)
//             {
//                 output.WriteLine($"{station.ThreeLetterCode}   {station.Locations.First().Tiploc}  {station.Name}");
//             }
//         }
//
//         private static readonly DateTime Start = new DateTime(2023, 10, 23);
//         
//         private static readonly string[] Days = Enumerable.Range(0, 14)
//             .Select(offset => Start.AddDays(offset).ToString("yyyy-MM-dd"))
//             .ToArray(); 
//         
//         [Fact]
//         public async void GetTocServiceCount()
//         {
//             var client = new HttpClient();
//             var url = @"http://localhost:8484/api/reference/toc";
//             var response = await client.GetAsync(url);
//             
//             response.EnsureSuccessStatusCode();
//             var responseString = await response.Content.ReadAsStringAsync();
//             var tocs = JsonConvert.DeserializeObject<Model.Toc[]>(responseString);
//
//             output.WriteLine($"toc,name,date,services,withFirst");
//             foreach (var toc in tocs)
//             {
//                 foreach (var day in Days)
//                 {
//                     await MakeTocServicesRequest(toc, day);
//                 }
//             }
//         }
//         
//         public async Task MakeTocServicesRequest(Model.Toc toc, string date)
//         {
//             try
//             {
//                 var client = new HttpClient();
//                 var url = $"http://localhost:8484/api/timetable/toc/{toc.Code}/{date}?includeStops=false";
//                 var response = await client.GetAsync(url);
//                 
//                 response.EnsureSuccessStatusCode();
//                 var responseString = await response.Content.ReadAsStringAsync();
//                 var services = JsonConvert.DeserializeObject<Model.ServiceSummary[]>(responseString);
//                 var message = $"{toc.Code},{toc.Name},{date},{services.Length},{services.Count(s => !s.IsCancelled && (HasFirstClassSeat(s) || HasFirstClassBerth(s)))}";
//                 output.WriteLine(message);
//             }
//             catch (Exception e)
//             {
//                 output.WriteLine($"{toc.Code},{toc.Name},{date},0,0");
//             }
//
//             bool HasFirstClassSeat(ServiceSummary s)
//             {
//                 return s.SeatClass == "Both" || s.SeatClass == "First";
//             }
//             
//             bool HasFirstClassBerth(ServiceSummary s)
//             {
//                 return s.SleeperClass == "Both" || s.SleeperClass == "First";
//             }
//         }
//         
//         [Fact]
//         public async void GetServiceCountForSpecificToc()
//         {
//             var client = new HttpClient();
//             var url = @"http://localhost:8484/api/reference/toc";
//             var response = await client.GetAsync(url);
//             
//             response.EnsureSuccessStatusCode();
//             // var responseString = await response.Content.ReadAsStringAsync();
//             // var tocs = JsonConvert.DeserializeObject<Model.Toc[]>(responseString);
//             var tocs = new[]
//             {
//                 new Model.Toc()
//                 {
//                     Code = "XC",
//                     Name = "Cross Country"
//                 }
//             };
//
//             output.WriteLine($"toc,name,date,services,withFirst");
//             foreach (var toc in tocs)
//             {
//                 foreach (var day in Days)
//                 {
//                     await MakeTocServicesRequest(toc, day);
//                 }
//             }
//         }
//     }
// }