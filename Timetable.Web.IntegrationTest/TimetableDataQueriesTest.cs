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
//         [InlineData("GW")]
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
//                 var serviceType = new ServiceType()
//                 {
//                     TrainClass = service.GetTrainClass(),
//                     Category = service.GetCategory()
//                 };
//
//                 if (serviceTypes.ContainsKey(serviceType))
//                 {
//                     serviceTypes[serviceType] |= service.GetDays();
//                 }
//                 else
//                 {
//                     serviceTypes.Add(serviceType, service.GetDays());
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
//                     Days = kv.Value.ToIsoDays()
//                 }).ToList()
//             };
//         }
//     }
// }