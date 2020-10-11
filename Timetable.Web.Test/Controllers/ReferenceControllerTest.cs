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
        
        [Fact]
        public async Task ReturnsLocations()
        {
            var data = Substitute.For<ILocationData>();
            data.Locations.Returns(TestLocations());

            var controller = new ReferenceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotEmpty(response.Value as Model.Station[]);
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
        
        
        public static TheoryData<string[], int> FilterData =>
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
        [MemberData(nameof(FilterData))]
        public async Task ReturnsFilteredLocations(string[] tocs, int expected)
        {
            var data = Substitute.For<ILocationData>();
            data.Locations.Returns(TestLocations());

            var controller = new ReferenceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync(tocs) as ObjectResult;
            
            var stations = response.Value as Model.Station[];
            Assert.Equal(expected, stations.Length);
        }
        
        [Fact]
        public async Task Returns400WhenInvalidToc()
        {
            var data = Substitute.For<ILocationData>();
            data.Locations.Returns(TestLocations());

            var controller = new ReferenceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.LocationAsync(new []{"SWR", "VT"}) as ObjectResult;
            
            Assert.Equal(400, response.StatusCode);
            var error = response.Value as ReferenceError;
            Assert.Equal("Invalid tocs provided in request SWR|VT", error.Reason);
        }        
    }
}