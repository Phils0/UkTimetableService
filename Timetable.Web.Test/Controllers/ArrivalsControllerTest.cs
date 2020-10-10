using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ArrivalsControllerTest
    {
        private static readonly MapperConfiguration _config = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        private static readonly DateTime Aug12 = new DateTime(2019, 8, 12, 0, 0, 0);
        private static readonly DateTime Aug12AtTenFifteen = new DateTime(2019, 8, 12, 10, 15, 0);
        
        [Theory]
        [InlineData("CLJ")]
        [InlineData("clj")]
        public async Task ArrivalsReturnsServices(string clapham)
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals("CLJ", Aug12AtTenFifteen, Arg.Any<GatherConfiguration>())
               .Returns((FindStatus.Success,  new [] { CreateClaphamResolvedStop() }));

            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals(clapham, Aug12AtTenFifteen) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        private ResolvedServiceStop CreateClaphamResolvedStop()
        {
            return TestSchedules.CreateResolvedDepartureStop(atLocation: TestStations.ClaphamJunction, when: TestSchedules.TenSixteen);
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

        [InlineData(FindStatus.LocationNotFound,  "Did not find location CLJ")]
        [InlineData(FindStatus.NoServicesForLocation, "Did not find services for CLJ@2019-08-12T10:15:00")]
        [Theory]
        public async Task ArrivalsReturnsNotFoundWithReason(FindStatus status, string expectedReason)
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((status, new ResolvedServiceStop[0]));

            var controller = new ArrivalsController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen) as ObjectResult;
            
            Assert.Equal(404, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal(expectedReason, notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Fact]
        public async Task ArrivalsReturnsError()
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Throws(new Exception("Something went wrong"));

            var controller = new ArrivalsController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen) as ObjectResult;
            
            Assert.Equal(500, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal("Error while finding services for CLJ@2019-08-12T10:15:00", notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Theory]
        [InlineData("SUR", true)]
        [InlineData("XXX", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public async Task SetsFromFilter(string location, bool hasFilter)
        {
            var data = Substitute.For<ILocationData>();
            data.TryGetStation("SUR", out Arg.Any<Station>()).Returns(true);
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { CreateClaphamResolvedStop() }));

            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ArrivalsComeFrom(Arg.Any<Station>()).Returns(GatherFilterFactory.NoFilter);
            
            var controller = new ArrivalsController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen, location) as ObjectResult;;

            if (hasFilter)
                filterFactory.Received().ArrivalsComeFrom(Arg.Any<Station>());
            else
                filterFactory.DidNotReceive().ArrivalsComeFrom(Arg.Any<Station>());
        }
        
        [Fact]
        public async Task SetsTocFilter()
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new ArrivalsController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen, toc: new [] {"VT"}) as ObjectResult;
            
            filterFactory.Received().ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter);
        }
        
        [Fact]
        public async Task ArrivalsNowSetsTocFilter()
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new ArrivalsController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ",  toc: new [] {"VT"}) as ObjectResult;
            
            filterFactory.Received().ProvidedByToc(Arg.Any<TocFilter>(), GatherFilterFactory.NoFilter);
        }
        
        [Theory]
        [InlineData("CLJ")]
        [InlineData("clj")]
        public async Task FullDayArrivalsReturnsServices(string clapham)
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals("CLJ", Aug12, Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((FindStatus.Success,  new [] { CreateClaphamResolvedStop() }));

            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals(clapham, Aug12, fullDay: true) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }
        
        [Fact]
        public async Task FullRailDayArrivalsReturnsServices()
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals("CLJ", Aug12, Arg.Any<GatherConfiguration.GatherFilter>(), Time.StartRailDay)
                .Returns((FindStatus.Success,  new [] { CreateClaphamResolvedStop() }));

            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12, fullDay: true, dayBoundary:"02:30") as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundSummaryResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        [InlineData(FindStatus.LocationNotFound,  "Did not find location CLJ")]
        [InlineData(FindStatus.NoServicesForLocation, "Did not find services for Day CLJ@2019-08-12T00:00:00")]
        [Theory]
        public async Task ArrivalsForDayReturnsNotFoundWithReason(FindStatus status, string expectedReason)
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Returns((status, new ResolvedServiceStop[0]));

            var controller = new ArrivalsController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12, fullDay: true) as ObjectResult;
            
            Assert.Equal(404, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal(expectedReason, notFound.Reason);
            AssertRequestSetInResponse(notFound);
        }
        
        [Fact]
        public async Task ArrivalsForDayReturnsError()
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>(), Arg.Any<Time>())
                .Throws(new Exception("Something went wrong"));

            var controller = new ArrivalsController(data, FilterFactory, _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12, fullDay: true) as ObjectResult;
            
            Assert.Equal(500, response.StatusCode);

            var notFound = response.Value as Model.NotFoundResponse;
            Assert.Equal("Error while finding services for Day CLJ@2019-08-12T00:00:00", notFound.Reason);
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
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));
            
            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen, includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
        
        [Theory]
        [MemberData(nameof(ReturnsStops))]
        public async Task ArrivalsNowReturnsServicesWithStops(bool includeStops, Type expected)
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));
            
            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", includeStops: includeStops) as ObjectResult;
            Assert.IsType(expected, response.Value);
        }
    }
}