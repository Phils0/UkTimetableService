using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Timetable.Web.Mapping;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.Test.Controllers
{
    public class DeparturesControllerTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime Aug12AtTen = new DateTime(2019, 8, 12, 10, 0, 0);
        
        [Fact]
        public async Task DeparturesReturnsServices()
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>())
               .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedStop() }));

            var controller = new DeparturesController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        private void AssertRequestSetInResponse(FoundResponse response)
        {
            Assert.NotNull(response.Request);
            Assert.True(response.GeneratedAt > DateTime.Now.AddMinutes(-1));
        }
        
        private void AssertRequestSetInResponse(NotFoundResponse response)
        {
            Assert.NotNull(response.Request);
            Assert.True(response.GeneratedAt > DateTime.Now.AddMinutes(-1));
        }

        [InlineData(FindStatus.LocationNotFound,  "Did not find location SUR")]
        [InlineData(FindStatus.NoServicesForLocation, "Did not find services for SUR@2019-08-12T10:00:00")]
        [Theory]
        public async Task DepartureReturnsNotFoundWithReason(FindStatus status, string expectedReason)
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns((status, new ResolvedServiceStop[0]));

            var controller = new DeparturesController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;
            
            Assert.Equal(404, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal(expectedReason, notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Fact]
        public async Task DeparturesReturnsError()
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>())
                .Throws(new Exception("Something went wrong"));

            var controller = new DeparturesController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;
            
            Assert.Equal(500, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal("Error while finding services for SUR@2019-08-12T10:00:00", notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
    }
}