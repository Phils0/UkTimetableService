using System;
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
    public class TimetableControllerGetDeparturesTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime Aug12AtTen = new DateTime(2019, 4, 1, 10, 0, 0);
        
        [Fact]
        public async Task DeparturesReturnsServices()
        {
            var data = Substitute.For<ITimetable>();
//            data.GetSchedulesByToc(Arg.Any<string>(), Arg.Any<DateTime>())
//                .Returns((LookupStatus.Success,  new [] {TestSchedules.CreateService()}));

            var controller = new TimetableController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.LocationTimetable;
            Assert.NotNull(services.Request);
            Assert.True(services.GeneratedAt > DateTime.Now.AddMinutes(-1));
            // Assert.NotEmpty(services.Services);
        }
    }
}