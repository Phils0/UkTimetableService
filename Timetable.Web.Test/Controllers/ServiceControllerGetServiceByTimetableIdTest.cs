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
    public class ServiceControllerGetServiceByTimetableIdTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime April1 = new DateTime(2019, 4, 1);

        [Fact]
        public async Task ServiceByTimetableUidReturnsService()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.Success, TestSchedules.CreateService()));

            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByTimetableId("X12345", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);
            var service = GetService(response);
            Assert.Equal("X12345", service.TimetableUid);
            Assert.False(service.IsCancelled);
        }

        private Model.Service GetService(ObjectResult response)
        {
            return response.Value as Model.Service;
        }
        
        
        [Fact]
        public async Task ServiceByTimetableUidReturnsNotFoundWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.ServiceNotFound, null));

            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByTimetableId("X12345", April1) as ObjectResult;;
            
            Assert.Equal(404, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("X12345", notFound.Id);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("X12345 not found in timetable", notFound.Reason);
        }
        
        [Fact]
        public async Task ServiceByTimetableUidReturnsCancelledWithReason()
        {
            var data = Substitute.For<ITimetable>();
            data.GetScheduleByTimetableUid(Arg.Any<string>(), Arg.Any<DateTime>())
                .Returns((LookupStatus.Success, TestSchedules.CreateService(isCancelled: true)));

            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetServiceByTimetableId("X12345", April1) as ObjectResult;
            
            Assert.Equal(200, response.StatusCode);
            var service = GetService(response);;
            Assert.Equal("X12345", service.TimetableUid);
            Assert.True(service.IsCancelled);
        }
    }
}