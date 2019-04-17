using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Timetable.Web.Mapping;
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
            data.Locations.Returns(
                new Dictionary<string, Station>()
                {
                    {"SUR", TestStations.Surbiton},
                    {"WAT", TestStations.Waterloo}
                });

            var controller = new ReferenceController(data, _config.CreateMapper());
            var response = await controller.LocationAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotEmpty(response.Value as Model.Station[]);
        }
    }
}