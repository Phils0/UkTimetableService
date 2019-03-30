using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
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
            var service = Substitute.For<IReference>();
            service.GetLocationsAsync(Arg.Any<CancellationToken>()).Returns(
                new[]
                {
                    TestStations.Surbiton,
                    TestStations.Waterloo
                });

            var controller = new ReferenceController(service, _config.CreateMapper());
            var response = await controller.LocationAsync() as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotEmpty(response.Value as Model.Station[]);
        }
    }
}