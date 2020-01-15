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
            var data = Substitute.For<ILocationData>();
            data.FindDepartures("SUR", Aug12AtTen, Arg.Any<GatherConfiguration>())
               .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures(surbiton, Aug12AtTen) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
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
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((status, new ResolvedServiceStop[0]));

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
            var data = Substitute.For<ILocationData>();
            data.TryGetLocation("WAT", out Arg.Any<Station>()).Returns(true);
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

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
        
        public static IEnumerable<object[]> Tocs
        {
            get
            {
                yield return new object[] {new [] {"VT"}, "VT", true};
                yield return new object[] {new [] {"vt"}, "VT", true};
                yield return new object[] {new [] {"VT", "GR"}, "VT|GR", true};
                yield return new object[] {new [] {"VT", "GR", "GW"}, "VT|GR|GW", true};
                yield return new object[] {new string[0], "", false};
                yield return new object[] {null, "", false};
            }
        }
        
        [Theory]
        [MemberData(nameof(Tocs))]
        public async Task SetsTocFilter(string[] tocs, string expectedTocFilter, bool expectedToHaveFilter)
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, toc: tocs) as ObjectResult;

            if (expectedToHaveFilter)
                filterFactory.Received().ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter);
            else
                filterFactory.DidNotReceive().ProvidedByToc(Arg.Any<string>(), Arg.Any<GatherConfiguration.GatherFilter>());
        }
        
        [Theory]
        [MemberData(nameof(Tocs))]
        public async Task DeparturesNowSetsTocFilter(string[] tocs, string expectedTocFilter, bool expectedToHaveFilter)
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new DeparturesController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", toc: tocs) as ObjectResult;

            if (expectedToHaveFilter)
                filterFactory.Received().ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter);
            else
                filterFactory.DidNotReceive().ProvidedByToc(Arg.Any<string>(), Arg.Any<GatherConfiguration.GatherFilter>());
        }
        
        [Theory]
        [InlineData("SUR")]
        [InlineData("sur")]
        public async Task AllDeparturesReturnsServices(string surbiton)
        {
            var data = Substitute.For<ILocationData>();
            data.AllDepartures("SUR", Aug12, Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures(surbiton, Aug12, fullDay: true) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }
        
        [Fact]
        public async Task AllRailDayDeparturesReturnsServices()
        {
            var data = Substitute.For<ILocationData>();
            data.AllDepartures("SUR", Aug12, Arg.Any<GatherConfiguration.GatherFilter>(), Time.StartRailDay)
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

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
            var data = Substitute.For<ILocationData>();
            data.AllDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((status, new ResolvedServiceStop[0]));

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
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));
            
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", Aug12AtTen, includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
        
        [Theory]
        [MemberData(nameof(ReturnsStops))]
        public async Task DeparturesNowReturnsServicesWithStops(bool includeStops, Type expected)
        {
            var data = Substitute.For<ILocationData>();
            data.FindDepartures(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));
            
            var controller = new DeparturesController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Departures("SUR", includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
    }
}