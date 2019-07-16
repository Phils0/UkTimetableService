using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timetable.Test.Data;
using Timetable.Web.Controllers;
using Timetable.Web.Mapping;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.Test.Controllers
{
    public class TimetableControllerTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime April1 = new DateTime(2019, 4, 1);

        [Fact]
        public async Task ServiceByTimetableUidReturnsService()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((TestSchedules.CreateSchedule(), ""));

            var controller = new TimetableController(data, _config.CreateMapper());
            var response = await controller.GetServiceByTimetableId("X12345", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Value as Model.Service);
        }
        
        [Fact]
        public async Task ServiceByTimetableUidReturnsNotFoundWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((null, "Not found"));

            var controller = new TimetableController(data, _config.CreateMapper());
            var response = await controller.GetServiceByTimetableId("X12345", April1) as ObjectResult;;
            
            Assert.Equal(404, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("X12345", notFound.TimetableUid);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("Not found", notFound.Reason);
        }
        
        [Fact]
        public async Task ServiceByRetailServiceIdReturnsService()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((TestSchedules.CreateSchedule(), ""));

            var controller = new TimetableController(data, _config.CreateMapper());
            var response = await controller.GetServiceByRetailServiceId("VT1234", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.Service[];
            Assert.NotEmpty(services);
        }
        
        [Fact]
        public async Task ServiceByRetailServiceIdReturnsNotFoundWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((null, "Not found"));

            var controller = new TimetableController(data, _config.CreateMapper());
            var response = await controller.GetServiceByRetailServiceId("VT1234", April1) as ObjectResult;;
            
            Assert.Equal(404, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("VT1234", notFound.RetailServiceid);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("Not found", notFound.Reason);
        }
    }
}