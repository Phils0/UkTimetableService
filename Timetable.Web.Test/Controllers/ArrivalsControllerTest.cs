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

            var services = response.Value as Model.FoundResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        private ResolvedServiceStop CreateClaphamResolvedStop()
        {
            return TestSchedules.CreateResolvedDepartureStop(atLocation: TestStations.ClaphamJunction, when: TestSchedules.TenSixteen);
        }

        private static GatherFilterFactory FilterFactory => new GatherFilterFactory();

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
            data.TryGetLocation("SUR", out Arg.Any<Station>()).Returns(true);
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
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new ArrivalsController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ", Aug12AtTenFifteen, toc: tocs) as ObjectResult;

            if (expectedToHaveFilter)
                filterFactory.Received().ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter);
            else
                filterFactory.DidNotReceive().ProvidedByToc(Arg.Any<string>(), Arg.Any<GatherConfiguration.GatherFilter>());
        }
        
        [Theory]
        [MemberData(nameof(Tocs))]
        public async Task ArrivalsNowSetsTocFilter(string[] tocs, string expectedTocFilter, bool expectedToHaveFilter)
        {
            var data = Substitute.For<ILocationData>();
            data.FindArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration>())
                .Returns((FindStatus.Success,  new [] { TestSchedules.CreateResolvedDepartureStop() }));

            var filter = Substitute.For<GatherConfiguration.GatherFilter>();
            var filterFactory = Substitute.For<IFilterFactory>();
            filterFactory.NoFilter.Returns(GatherFilterFactory.NoFilter);
            filterFactory.ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter).Returns(filter);
            
            var controller = new ArrivalsController(data,  filterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals("CLJ",  toc: tocs) as ObjectResult;

            if (expectedToHaveFilter)
                filterFactory.Received().ProvidedByToc(expectedTocFilter, GatherFilterFactory.NoFilter);
            else
                filterFactory.DidNotReceive().ProvidedByToc(Arg.Any<string>(), Arg.Any<GatherConfiguration.GatherFilter>());
        }
        
        [Theory]
        [InlineData("CLJ")]
        [InlineData("clj")]
        public async Task FullDayArrivalsReturnsServices(string clapham)
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals("CLJ", Aug12, Arg.Any<GatherConfiguration.GatherFilter>())
                .Returns((FindStatus.Success,  new [] { CreateClaphamResolvedStop() }));

            var controller = new ArrivalsController(data,  FilterFactory,  _config.CreateMapper(), Substitute.For<ILogger>());
            var response = await controller.Arrivals(clapham, Aug12, fullDay: true) as ObjectResult;;
            
            Assert.Equal(200, response.StatusCode);

            var services = response.Value as Model.FoundResponse;
            AssertRequestSetInResponse(services);
            Assert.NotEmpty(services.Services);
        }

        [InlineData(FindStatus.LocationNotFound,  "Did not find location CLJ")]
        [InlineData(FindStatus.NoServicesForLocation, "Did not find services for Day CLJ@2019-08-12T00:00:00")]
        [Theory]
        public async Task ArrivalsForDayReturnsNotFoundWithReason(FindStatus status, string expectedReason)
        {
            var data = Substitute.For<ILocationData>();
            data.AllArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>())
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
            data.AllArrivals(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<GatherConfiguration.GatherFilter>())
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
                yield return new object[] {false, typeof(FoundSummaryItem)};                
                yield return new object[] {true, typeof(FoundServiceItem)};
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
            var found = response.Value as Model.FoundResponse;

            Assert.IsType(expected, found.Services[0]);
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
            var found = response.Value as Model.FoundResponse;

            Assert.IsType(expected, found.Services[0]);
        }
    }
}