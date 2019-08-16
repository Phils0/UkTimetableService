using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class GatherFilterTest
    {
        private  static readonly IFilterFactory Factory = new GatherFilterFactory();
        
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
        
        [Theory]
        [InlineData("VT", 2)]
        [InlineData("GR", 1)]
        [InlineData("GR|VT", 3)]
        public void FilterTocs(string tocFilterString, int expected)
        {
            var source = ComesFromSource;
            source[0].Details.Operator.Code = "GR";
            source[1].Details.Operator.Code = "VT";
            source[2].Details.Operator.Code = "VT";
            source[3].Details.Operator.Code = "SW";

            var filter = Factory.ProvidedByToc(tocFilterString, Factory.NoFilter);

            var results = filter(source).ToArray();
            
            Assert.Equal(expected, results.Length);
        }
    }
}