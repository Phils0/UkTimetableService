using System.Collections.Generic;
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
        
        public static TheoryData<TocFilter, int> FilterTocData =>
            new TheoryData<TocFilter, int>()
            {
                {new TocFilter(new [] {"VT"}), 2},
                {new TocFilter(new [] {"GR"}), 1},
                {new TocFilter(new [] {"VT", "GR"}), 3},
                {new TocFilter(new [] {"GW"}), 0},
            };
        
        [Theory]
        [MemberData(nameof(FilterTocData))]
        public void FilterTocs(TocFilter tocFilter, int expected)
        {
            var source = ComesFromSource;
            SetToc(source[0],"GR");
            SetToc(source[1],"VT");
            SetToc(source[2],"VT");
            SetToc(source[3],"SW");

            var filter = Factory.ProvidedByToc(tocFilter, Factory.NoFilter);

            var results = filter(source).ToArray();
            
            Assert.Equal(expected, results.Length);
        }
        
        private void SetToc(ResolvedServiceStop stop, string toc)
        {
            var properties = ((CifSchedule) stop.Service.Details).Properties;
            properties.Operator = (toc == null ? null : new Toc(toc));
        }

        [Fact]
        public void FilterTocsHandlesExceptions()
        {
            var source = ComesFromSource;
            SetToc(source[0],"GR");
            SetToc(source[1], null);
            SetToc(source[2],"VT");
            SetToc(source[3],"SW");

            var filter = Factory.ProvidedByToc(TocFilter, Factory.NoFilter);

            var results = filter(source).ToArray();
            
            Assert.Single(results);
        }
        
        private TocFilter TocFilter => new TocFilter(new [] {"VT"});
        
        [Fact]
        public void HandlesBeingPassedAnEmptyEnumerable()
        {
            var source = Enumerable.Empty<ResolvedServiceStop>();
    
            var filter = Factory.ProvidedByToc(TocFilter, Factory.NoFilter);

            var results = filter(source);
            Assert.Empty(results);
        }
        
        public static IEnumerable<object[]> Tocs
        {
            get
            {
                yield return new object[] {new TocFilter(new [] {"VT"}), true};
                yield return new object[] {new TocFilter(null), false};
            }
        }

        [MemberData(nameof(Tocs))]
        [Theory]
        public void SetTocGatherFilter(TocFilter filter, bool addedFilter)
        {
            var noFilter = Factory.NoFilter;
            var gatherFilter = Factory.ProvidedByToc(filter , noFilter);
            if(addedFilter)
                Assert.NotSame(noFilter, gatherFilter);
            else
                Assert.Same(noFilter, gatherFilter);
        }
    }
}