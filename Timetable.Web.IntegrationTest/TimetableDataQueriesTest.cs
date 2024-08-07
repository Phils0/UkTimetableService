﻿// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Newtonsoft.Json;
// using ReflectionMagic;
// using Xunit;
//
// namespace Timetable.Web.IntegrationTest
// {
//     public record ServiceType
//     {
//         public string TrainClass { get; init; }
//         public string Category { get; init; }
//         public string Route { get; init; }
//     }
//     
//     public record DaysForServiceType : ServiceType
//     {
//         public string Days { get; init; }
//     }
//     
//     public record RetailService
//     {
//         public string RetailServiceId { get; init; }
//         public List<DaysForServiceType> Days { get; init; }
//     }
//     
//     [Collection("Service")]
//     public class TimetableDataQueriesTest : ServiceTestBase
//     {
//         public TimetableDataQueriesTest(WebServiceFixture fixture) : base(fixture)
//         {
//         }
//
//         [Theory]
//         [InlineData("TP")]
//         public async void GetAllRetailServiceIds(string toc)
//         {
//             var data = Host.Services.GetService(typeof(Timetable.Data)) as Timetable.Data;
//             var timetable = data.Timetable.AsDynamic();
//             var rsidLookup = timetable._retailServiceIdMap.RealObject as Dictionary<string, IList<IService>>;
//
//             var tocServices = rsidLookup
//                 .Where(kv => kv.Key.StartsWith(toc))
//                 .Select(ToRetailService)
//                 .OrderBy(r => r.RetailServiceId)
//                 .ToArray();
//             
//             var json = JsonConvert.SerializeObject(tocServices);
//             await File.WriteAllTextAsync($"C:\\tmp\\{toc}.json", json);
//         }
//
//         private RetailService ToRetailService(KeyValuePair<string, IList<IService>> kv)
//         {
//             var serviceTypes = new Dictionary<ServiceType, DaysFlag>();
//             
//             foreach (var s in kv.Value)
//             {
//                 var service = s as CifService;
//                 var analyser = service.CreateAnalyser();
//                 var serviceType = new ServiceType()
//                 {
//                     TrainClass = analyser.TrainClasses,
//                     Category = analyser.Categories,
//                     Route = analyser.GetRoutes()
//                 };
//
//                 if (serviceTypes.ContainsKey(serviceType))
//                 {
//                     serviceTypes[serviceType] |= analyser.Days;
//                 }
//                 else
//                 {
//                     serviceTypes.Add(serviceType, analyser.Days);
//                 }
//             }
//
//             return new RetailService()
//             {
//                 RetailServiceId = kv.Key,
//                 Days = serviceTypes.Select(kv => new DaysForServiceType()
//                 {
//                     TrainClass = kv.Key.TrainClass,
//                     Category = kv.Key.Category,
//                     Route = kv.Key.Route,
//                     Days = kv.Value.ToIsoDays()
//                 }).ToList()
//             };
//         }
//
//         public static TheoryData<string, string[]> Routes =>
//             new TheoryData<string, string[]>()
//             {
//                 {"TP", ["YRK", "ALM"]},
//             };
//         
//         [Theory]
//         [MemberData(nameof(Routes))]
//         public async void GetRouteRetailServiceIds(string toc, string[] includes)
//         {
//             var data = Host.Services.GetService(typeof(Timetable.Data)) as Timetable.Data;
//
//             var locations = data.Locations.Locations;
//             var includeStations = includes.Select(l => locations[l]).ToArray();
//             
//             var timetable = data.Timetable.AsDynamic();
//             var rsidLookup = timetable._retailServiceIdMap.RealObject as Dictionary<string, IList<IService>>;
//             
//             var tocServices = rsidLookup
//                 .Where(kv => kv.Key.StartsWith(toc) && IncludesStops(kv.Value, includeStations))
//                 .Select(ToRetailService)
//                 .OrderBy(r => r.RetailServiceId)
//                 .ToArray();
//             
//             var json = JsonConvert.SerializeObject(tocServices);
//             await File.WriteAllTextAsync($"C:\\tmp\\{toc}{includes.Aggregate("_", (a,s) => $"{a}_{s}")}.json", json);
//         }
//
//         private bool IncludesStops(IList<IService> services, Station[] includes)
//         {
//             
//             foreach (var s in services)
//             {
//                 var service = ((CifService) s).CreateAnalyser();
//
//                 if (includes.All(s => service.StopsAt(s)))
//                 {
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//     }
// }