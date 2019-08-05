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
    public class TimetableControllerGetServiceByRetailServiceIdTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime April1 = new DateTime(2019, 4, 1);
        
        [Fact]
        public async Task ServiceByRetailServiceIdReturnsService()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByRetailServiceId(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.Success,  new [] {TestSchedules.CreateService()}));

            var controller = new TimetableController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByRetailServiceId("VT1234", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.Service[];
            var service = services[0];
            Assert.Equal("VT123400", service.RetailServiceId);
            Assert.False(service.IsCancelled);
        }
        
        [Fact]
        public async Task ServiceByRetailServiceIdReturnsNotFoundWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByRetailServiceId(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.ServiceNotFound, new ResolvedService[0]));

            var controller = new TimetableController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByRetailServiceId("VT1234", April1) as ObjectResult;;
            
            Assert.Equal(404, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("VT1234", notFound.Id);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("VT1234 not found in timetable", notFound.Reason);
        }
        
        [Fact]
        public async Task ServiceByRetailServiceIdReturnsCancelledWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByRetailServiceId(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.Success, new [] { TestSchedules.CreateService(isCancelled: true)}));

            var controller = new TimetableController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByRetailServiceId("VT1234", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            
            var services = response.Value as Model.Service[];
            var service = services[0];
            Assert.Equal("VT123400", service.RetailServiceId);
            Assert.True(service.IsCancelled);
        }
    }
}