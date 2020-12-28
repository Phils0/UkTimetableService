using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Timetable.Web.Mapping;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.Test.Controllers
{
    public class ReferenceControllerTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());
        
        private static Timetable.Data TestData()
        {
            var tocs = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", new Toc("VT")
                    {
                        Name = "Avanti"
                    }}
                });
            
            var data = Substitute.For<ILocationData>();
            data.Locations.Returns(TestLocations());
            
            return new Timetable.Data()
            {
                Tocs = tocs,
                Locations = data,
                Darwin = new RealtimeData()
                {
                    CancelReasons = CreateReasons("Cancel"),
                    LateRunningReasons = CreateReasons("Late"),
                    Sources = new Dictionary<string, string>()
                    {
                        {"AM01", "Southern Metropolitan" },
                        {"at02", "Birmingham" },
                        {"si01", "St Pancras" },
                    }
                }
            };
        }
        
        private static Dictionary<string, Station> TestLocations()
        {
            var surbiton = TestStations.Surbiton;
            AddToc(surbiton, "SW");

            var waterloo = TestStations.Waterloo;
            AddToc(waterloo, "SW");
            AddToc(waterloo, "SE");
            
            var weybridge = TestStations.Weybridge;
            AddToc(weybridge, "TL");
            
            return new Dictionary<string, Station>()
            {
                {"SUR", surbiton},
                {"WAT", waterloo},
                {"WYB", weybridge}
            };
        }
        
        private static void AddToc(Station station, string toc)
        {
            station.TocServices.Add(new Toc(toc));
        }

        private static IReadOnlyDictionary<int, string> CreateReasons(string prefix)
        {
            return new Dictionary<int, string>()
            {
                {1, $"{prefix} Reason 1"},
                {999, $"{prefix} Reason 999"}
            };
        }

        [Fact]
        public async Task ReturnsLocations()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotEmpty(response.Value as Model.Station[]);
        }
        
        public static TheoryData<string[], int> TocFilterData =>
            new TheoryData<string[], int>()
            {
                {new []{"SW"},  2},
                {new []{"SE"},  1},
                {new []{"TL"},  1},
                {new []{"SW", "SE"},  2},
                {new []{"TL", "SE"},  2},
                {new []{"SW", "SE", "TL"},  3},
                {new []{""},  3},
                {new string[]{null},  3},
                {new string[0],  3},
                {null,  3},
            };
        
        [Theory]
        [MemberData(nameof(TocFilterData))]
        public async Task ReturnsFilteredLocations(string[] tocs, int expected)
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync(toc: tocs) as ObjectResult;
            
            var stations = response.Value as Model.Station[];
            Assert.Equal(expected, stations.Length);
        }
        
        [Fact]
        public async Task Returns400WhenInvalidToc()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync(toc: new []{"SWR", "VT"}) as ObjectResult;
            
            Assert.Equal(400, response.StatusCode);
            var error = response.Value as ReferenceError;
            Assert.Equal("Invalid tocs provided in request SWR|VT", error.Reason);
        }    
        
        public static TheoryData<string, int> ServiceOperatorData =>
            new TheoryData<string, int>()
            {
                {"SW",  2},
                {"NR",  1},
                {"",  3},
                {null,  3},
            };
        
        [Theory]
        [MemberData(nameof(ServiceOperatorData))]
        public async Task ReturnsServiceOperatorFilteredLocations(string serviceOperator, int expected)
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync(serviceOperator) as ObjectResult;
            
            var stations = response.Value as Model.Station[];
            Assert.Equal(expected, stations.Length);
        }
        
        [Fact]
        public async Task ReturnsNotFoundWhenTocRunsNoServices()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync("XC") as ObjectResult;
            Assert.IsType<NotFoundObjectResult>(response);
            Assert.IsType<ReferenceError>(response.Value);
        }
        
        [Fact]
        public async Task ReturnsTocs()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.TocsAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotEmpty(response.Value as Model.Toc[]);
        }
        
        [Fact]
        public async Task ReturnsCancellationReasons()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.CancellationReasonsAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            var reasons = (response.Value) as IEnumerable<Reason>;
            Assert.NotEmpty(reasons);
            foreach (var reason in reasons)
            {
                Assert.StartsWith("Cancel", reason.Text);
            }
            
        }
        
        [Fact]
        public async Task ReturnsLateRunningReasons()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LateReasonsAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            var reasons = (response.Value) as IEnumerable<Reason>;
            Assert.NotEmpty(reasons);
            foreach (var reason in reasons)
            {
                Assert.StartsWith("Late", reason.Text);
            }
        }
        
        [Fact]
        public async Task ReturnsDarwinSources()
        {
            var controller = new ReferenceController(TestData(), _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.DarwinSourcesAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            var sources = (response.Value) as IEnumerable<DarwinSource>;
            Assert.NotEmpty(sources);
        }
    }
}