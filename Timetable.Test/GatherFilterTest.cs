using System.Linq;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class GatherFilterTest
    {
        private  static readonly IFilterFactory Factory = new GatherFilterFactory(Substitute.For<ILogger>());
        
        [Fact]
        public void NoFilterDoesNothing()
        {
            var source = Source;
            var filter = Factory.NoFilter;

            var results = filter(source).ToArray();
            
            Assert.Equal(4, results.Count());
        }
        
        [Fact]
        public void FilterGoesTo()
        {
            var source = Source;
            var filter = Factory.DeparturesGoTo(TestStations.ClaphamJunction);

            var results = filter(source).ToArray();
            
            Assert.Contains(source[0], results);
            Assert.DoesNotContain(source[1], results);
            Assert.Contains(source[3], results);
            Assert.Equal(2, results.Count());
        }

        private ResolvedServiceStop[] Source
        {
            get
            {
                return new[]
                {
                    TestSchedules.CreateResolvedDepartureStop(),
                    TestSchedules.CreateResolvedDepartureStop(
                        stops: TestSchedules.DefaultLocations.Where(s => !s.Station.Equals(TestStations.ClaphamJunction))
                            .ToArray()),
                    TestSchedules.CreateResolvedDepartureStop(
                        stops: TestSchedules.DefaultLocations.Where(s => !s.Station.Equals(TestStations.ClaphamJunction))
                            .ToArray()),
                    TestSchedules.CreateResolvedDepartureStop(),
                };
            }
        }

        [Fact]
        public void FilterAllWhenDoesNotGoesTo()
        {
            var source = Source;
            var filter = Factory.DeparturesGoTo(TestStations.Vauxhall);

            var results = filter(source).ToArray();
            
            Assert.Empty(results);
        }
        
        [Fact]
        public void GoesToFilterHandlesErrors()
        {
            var source = ComesFromSource;
            var filter = Factory.DeparturesGoTo(TestStations.ClaphamJunction);

            var results = filter(source).ToArray();
            
            Assert.NotEmpty(results);
        }
        
        private ResolvedServiceStop[] ComesFromSource
        {
            get
            {
                return new[]
                {
                    TestSchedules.CreateResolvedArrivalStop( atLocation: TestStations.Waterloo, when: TestSchedules.TenThirty),
                    TestSchedules.CreateResolvedArrivalStop( atLocation: TestStations.Waterloo, when: TestSchedules.TenThirty,
                        stops: TestSchedules.DefaultLocations.Where(s => !s.Station.Equals(TestStations.ClaphamJunction))
                            .ToArray()),
                    TestSchedules.CreateResolvedArrivalStop( atLocation: TestStations.Waterloo, when: TestSchedules.TenThirty,
                        stops: TestSchedules.DefaultLocations.Where(s => !s.Station.Equals(TestStations.ClaphamJunction))
                            .ToArray()),
                    TestSchedules.CreateResolvedArrivalStop(atLocation: TestStations.Waterloo, when: TestSchedules.TenThirty),
                };
            }
        }
        
        [Fact]
        public void FilterComesFrom()
        {
            var source = ComesFromSource;
            var filter = Factory.ArrivalsComeFrom(TestStations.ClaphamJunction);

            var results = filter(source).ToArray();
            
            Assert.Contains(source[0], results);
            Assert.DoesNotContain(source[1], results);
            Assert.Contains(source[3], results);
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FilterAllWhenDoesNotComeFrom()
        {
            var source = ComesFromSource;
            var filter = Factory.ArrivalsComeFrom(TestStations.Vauxhall);

            var results = filter(source).ToArray();
            
            Assert.Empty(results);
        }
        
        [Fact]
        public void ComesFromFilterHandlesErrors()
        {
            var source = Source;
            var filter = Factory.ArrivalsComeFrom(TestStations.ClaphamJunction);

            var results = filter(source).ToArray();
            
            Assert.Empty(results);
        }
        
        [Theory]
        [InlineData("VT", 2)]
        [InlineData("GR", 1)]
        [InlineData("GR|VT", 3)]
        [InlineData("GW", 0)]
        public void FilterTocs(string tocFilterString, int expected)
        {
            var source = ComesFromSource;
            source[0].Operator.Code = "GR";
            source[1].Operator.Code = "VT";
            source[2].Operator.Code = "VT";
            source[3].Operator.Code = "SW";

            var filter = Factory.ProvidedByToc(tocFilterString, Factory.NoFilter);

            var results = filter(source).ToArray();
            
            Assert.Equal(expected, results.Length);
        }

        [Fact]
        public void FilterTocsHandlesExceptions()
        {
            var source = ComesFromSource;
            source[0].Operator.Code = "GR";
            source[1].Service.Details.Operator = null;
            source[2].Operator.Code = "VT";
            source[3].Operator.Code = "SW";

            var filter = Factory.ProvidedByToc("VT", Factory.NoFilter);

            var results = filter(source).ToArray();
            
            Assert.Single(results);
        }
        
        [Fact]
        public void HandlesBeingPassedAnEmptyEnumerable()
        {
            var source = Enumerable.Empty<ResolvedServiceStop>();
    
            var filter = Factory.ProvidedByToc("VT", Factory.NoFilter);

            var results = filter(source);
            Assert.Empty(results);
        }
    }
}