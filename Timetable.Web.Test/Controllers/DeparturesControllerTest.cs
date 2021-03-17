using System;
using System.Collections.Generic;
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

        private static readonly DateTime Aug12 = new DateTime(2019, 8, 12);
        private static readonly DateTime Aug12AtTen = Aug12.AddHours(10);
        
        [Theory]
        [InlineData("SUR")]
        [InlineData("sur")]
        public async Task DeparturesReturnsServices(string surbiton)
        {
            var data = CreateStubDataWithFindDepartures("SUR", Aug12AtTen);
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures(surbiton, Aug12AtTen) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        private static ILocationData CreateStubDataWithFindDepartures(string atLocation = null, DateTime? atTime = null,
            GatherConfiguration config = null, FindStatus returnedStatus = FindStatus.Success, ResolvedServiceStop[] returnedStops = null)
        {
            atLocation = atLocation ?? Arg.Any<string>();
            atTime = atTime ?? Arg.Any<DateTime>();
            config = config ?? Arg.Any<GatherConfiguration>();
            returnedStops = returnedStops ?? new [] { TestSchedules.CreateResolvedDepartureStop() };

            var data = Substitute.For<ILocationData>();
            data.FindDepartures(atLocation, atTime.Value, config)
                .Returns((returnedStatus, returnedStops));
            return data;
        }

        private static GatherFilterFactory FilterFactory => new GatherFilterFactory(Substitute.For<ILogger>());

        private void AssertRequestSetInResponse(SearchResponse response)
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
            var data = CreateStubDataWithFindDepartures(returnedStatus: status, returnedStops: new ResolvedServiceStop[0]);
            var controller = new DeparturesController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
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
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Throws(new Exception("Something went wrong"));

            var controller = new DeparturesController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen) as ObjectResult;
            
            Assert.Equal(500, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal("Error while finding services for SUR@2019-08-12T10:00:00", notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Theory]
        [InlineData("WAT", true)]
        [InlineData("XXX", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public async Task SetsToFilter(string location, bool hasFilter)
        {
            var data = CreateStubDataWithFindDepartures();
            data.TryGetStation("WAT", out Arg.Any<Station>()).Returns(true);

            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.DeparturesGoTo(Arg.Any<Station>()).Returns(GatherFilterFactory.NoFilter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, location) as ObjectResult;;

            if (hasFilter)
                filterFactory.Received().DeparturesGoTo(Arg.Any<Station>());
            else
                filterFactory.DidNotReceive().DeparturesGoTo(Arg.Any<Station>());
        }
        
        [Fact]
        public async Task SetsTocFilter()
        {
            var data = CreateStubDataWithFindDepartures();
            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, toc: new [] {"VT"}) as ObjectResult;
            
            filterFactory.Received().ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter);
        }
        
        [Fact]
        public async Task DeparturesNowSetsTocFilter()
        {
            var data = CreateStubDataWithFindDepartures();
            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", toc: new [] {"VT"}) as ObjectResult;
            
            filterFactory.Received().ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter);
        }
        
        [Fact]
        public async Task SetsReturnedCancelledFlag()
        {
            var returnedStop = TestSchedules.CreateResolvedDepartureStop(isCancelled: true);
            var data = CreateStubDataWithFindDepartures(returnedStops:  new [] { returnedStop });
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, returnCancelledServices: true) as ObjectResult;
            
            Assert.Equal(200, response.StatusCode);
            var services = response.Value as Model.FoundSummaryResponse;
            Assert.Single(services.Services);
            Assert.True(services.Services[0].Service.IsCancelled);
        }
        
        [Fact]
        public async Task DeparturesNowSetsReturnedCancelledFlag()
        {
            var returnedStop = TestSchedules.CreateResolvedDepartureStop(isCancelled: true);
            var data = CreateStubDataWithFindDepartures(returnedStops:  new [] { returnedStop });
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", returnCancelledServices: true) as ObjectResult;
            
            Assert.Equal(200, response.StatusCode);
            var services = response.Value as Model.FoundSummaryResponse;
            Assert.Single(services.Services);
            Assert.True(services.Services[0].Service.IsCancelled);
        }
        
        [Theory]
        [InlineData("SUR")]
        [InlineData("sur")]
        public async Task AllDeparturesReturnsServices(string surbiton)
        {
            var data = CreateStubDataWithAllDepartures("SUR", Aug12);
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures(surbiton, Aug12, fullDay: true) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }
        
        private static ILocationData CreateStubDataWithAllDepartures(string atLocation = null, DateTime? atTime = null,
            GatherConfiguration.GatherFilter config = null, Time? boundary = null, FindStatus returnedStatus = FindStatus.Success, ResolvedServiceStop[] returnedStops = null)
        {
            atLocation = atLocation ?? Arg.Any<string>();
            atTime = atTime ?? Arg.Any<DateTime>();
            config = config ?? Arg.Any<GatherConfiguration.GatherFilter>();
            returnedStops = returnedStops ?? new [] { TestSchedules.CreateResolvedDepartureStop() };
            boundary = boundary ?? Time.Midnight;

            var data = Substitute.For<ILocationData>();
            data.AllDepartures(atLocation, atTime.Value, config, boundary.Value)
                .Returns((returnedStatus, returnedStops));
            return data;
        }
        
        [Fact]
        public async Task AllRailDayDeparturesReturnsServices()
        {
            var data = CreateStubDataWithAllDepartures("SUR", Aug12, boundary: Time.StartRailDay);
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12, fullDay: true, dayBoundary: "02:30") as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }
        
        [InlineData(FindStatus.LocationNotFound,  "Did not find location SUR")]
        [InlineData(FindStatus.NoServicesForLocation, "Did not find services for Day SUR@2019-08-12T00:00:00")]
        [Theory]
        public async Task DeparturesForDayReturnsNotFoundWithReason(FindStatus status, string expectedReason)
        {
            var data = CreateStubDataWithAllDepartures(returnedStatus: status, returnedStops: new ResolvedServiceStop[0]);
            var controller = new DeparturesController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12, fullDay: true) as ObjectResult;
            
            Assert.Equal(404, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal(expectedReason, notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Fact]
        public async Task DeparturesForDayReturnsError()
        {
            var data = Substitute.For<ILocationData>();
            data.AllDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Throws(new Exception("Something went wrong"));

            var controller = new DeparturesController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, fullDay: true) as ObjectResult;
            
            Assert.Equal(500, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal("Error while finding services for Day SUR@2019-08-12T00:00:00", notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        public static IEnumerable<object[]> ReturnsStops
        {
            get
            {
                yield return new object[] {false, typeof(FoundSummaryResponse)};                
                yield return new object[] {true, typeof(FoundServiceResponse)};
            }
        }
        
        [Theory]
        [MemberData(nameof(ReturnsStops))]
        public async Task ReturnsServicesWithStops(bool includeStops, Type expected)
        {
            var data = CreateStubDataWithFindDepartures();
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
        
        [Theory]
        [MemberData(nameof(ReturnsStops))]
        public async Task DeparturesNowReturnsServicesWithStops(bool includeStops, Type expected)
        {
            var data = CreateStubDataWithFindDepartures();
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
        
        [Fact]
        public async Task DeparturesReturns400WithInvalidTocs()
        {
            var data = CreateStubDataWithFindDepartures();
            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, toc: new [] {"VT", "SWR"}) as ObjectResult;
            
            Assert.Equal(400, response.StatusCode);
            var error = response.Value as BadRequestResponse;
            Assert.Equal("Invalid tocs provided in request VT|SWR", error.Reason);
        }
    }
}