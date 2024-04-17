// using System;
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
//     public record Schedule(
//         string RetailServiceId,
//         string Origin,
//         string Destination
//     );
//     
//     public record ScheduleWithCalandar(
//         string RetailServiceId,
//         string TimetableId,
//         string TrainIdentity,
//         string Origin,
//         string Destination,
//         string Calandar
//     ): Schedule(RetailServiceId, Origin, Destination);
//     
//     public record ScheduleWithCalandars(
//         string RetailServiceId,
//         string Origin,
//         string Destination,
//         string[] Calandars
//     ): Schedule(RetailServiceId, Origin, Destination);
//     
//     [Collection("Service")]
//     public class TimetableDataQueriesTest : ServiceTestBase
//     {
//         public TimetableDataQueriesTest(WebServiceFixture fixture) : base(fixture)
//         {
//         }
//
//         [Theory]
//         [InlineData("GW")]
//         [InlineData("LE")]
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
//                     TrainClass = analyser.SeatClasses,
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
//         [Theory]
//         [InlineData("LE")]
//         public async void GetAllExpressServices(string toc)
//         {
//             var data = Host.Services.GetService(typeof(Timetable.Data)) as Timetable.Data;
//             var timetable = data.Timetable.AsDynamic();
//             var rsidLookup = timetable._retailServiceIdMap.RealObject as Dictionary<string, IList<IService>>;
//
//             var tocServices = rsidLookup
//                 .Where(kv => kv.Key.StartsWith(toc))
//                 .SelectMany(kv => kv.Value.Select(s => s as CifService))
//                 .SelectMany(s => s.CreateAnalyser().ActiveSchedules)
//                 .Where(s => IsFirstExpress(s) && !(OriginOrDestinationIs(s, "NRW") || OriginOrDestinationIs(s, "IPS")))
//                 .Select(s => new ScheduleWithCalandar(s.RetailServiceId, s.TimetableUid, s.TrainIdentity,
//                     s.Origin.Location.ThreeLetterCode, s.Destination.Location.ThreeLetterCode, s.Calendar.ToString()))
//                 .GroupBy(s => new Schedule(s.RetailServiceId, s.Origin, s.Destination))
//                 .Select(g => new ScheduleWithCalandars(
//                     g.Key.RetailServiceId,
//                     g.Key.Origin,
//                     g.Key.Destination,
//                     g.Select(s => s.Calandar).ToArray()
//                 ))
//                 .OrderBy(s => s.RetailServiceId)
//                 .ToArray();
//             
//             var json = JsonConvert.SerializeObject(tocServices);
//             await File.WriteAllTextAsync($"C:\\tmp\\{toc}_express_services.json", json);
//         }
//
//         private bool OriginOrDestinationIs(CifSchedule schedule, string location)
//         {
//             return schedule.Origin.Location.ThreeLetterCode == location || schedule.Destination.Location.ThreeLetterCode == location;
//         }
//
//         private bool IsFirstExpress(CifSchedule schedule)
//         {
//             return schedule is { Category: "XX", SeatClass: AccomodationClass.Both};
//         }
//     }
// }