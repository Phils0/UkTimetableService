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
    public class ServiceControllerGetTocServicesTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime April1 = new DateTime(2019, 4, 1);
        
        [Fact]
        public async Task ServicesByTocReturnsServices()
        {
            var data = CreateTimetableMock(Time.Midnight, LookupStatus.Success);
            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetTocServices("VT", April1) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.ServiceSummary[];
            Assert.NotEmpty(services);
        }
        
        private ITimetableLookup CreateTimetableMock(Time dayBoundary, LookupStatus status)
        {
            var services = status == LookupStatus.Success
                ? new[] {TestSchedules.CreateService()}
                : new ResolvedService[0];
            
            var data = Substitute.For<ITimetableLookup>();
            data.GetSchedulesByToc(Arg.Any<string>(), Arg.Any<DateTime>(), dayBoundary)
                .Returns((LookupStatus.Success, services));
            data.CreateFilter()
                .Returns(new FilterServicesDecorator(data, Timetable.Test.Data.Filters.Instance));
            return data;
        }
        
        [Fact]
        public async Task ServicesByTocReturnsFullSchedulewsWhenSetFullScheduleParameter()
        {
            var data = CreateTimetableMock(Time.Midnight, LookupStatus.Success);
            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetTocServices("VT", April1, includeStops: true) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.Service[];
            Assert.NotEmpty(services);
        }
        
        [Fact]
        public async Task RailDayServicesByTocReturnsServices()
        {
            var data = CreateTimetableMock(Time.StartRailDay, LookupStatus.Success);
            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetTocServices("VT", April1, "02:30") as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.ServiceSummary[];
            Assert.NotEmpty(services);
        }
        
        [Fact]
        public async Task ServicesByTocReturnsNotFoundWithReason()
        {
            var data = CreateTimetableMock(Time.Midnight, LookupStatus.ServiceNotFound);
            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetTocServices("VT", April1) as ObjectResult;;
            
            Assert.Equal(404, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("VT", notFound.Id);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("VT not found in timetable", notFound.Reason);
        }
        
        [Fact]
        public async Task ServicesByTocReturnsError()
        {
            var data = Substitute.For<ITimetableLookup>();
            data.GetSchedulesByToc(Arg.Any<string>(), Arg.Any<DateTime>(), Time.Midnight)
                .Throws(new Exception("Test"));

            var controller = new ServiceController(data, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.GetTocServices("VT", April1) as ObjectResult;;
            
            Assert.Equal(500, response.StatusCode);
            var notFound = response.Value as ServiceNotFound;
            Assert.Equal("VT", notFound.Id);
            Assert.Equal(April1, notFound.Date);
            Assert.Equal("Error looking for Toc VT on 2019-04-01", notFound.Reason);
        }
    }
}