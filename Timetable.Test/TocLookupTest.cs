using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Serilog;
using Xunit;

namespace Timetable.Test
{
    public class TocLookupTest
    {
        public ILookup<string, Toc> TestLookup { get; }
        
        public IList<Toc> TestTocs { get; }
        
        public TocLookupTest()
        {
            TestTocs = new List<Toc>();
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());
            TestTocs.Add(lookup.FindOrAdd("VT"));
            TestTocs.Add(lookup.FindOrAdd("GR"));
            TestTocs.Add(lookup.FindOrAdd("GW"));
            TestLookup = lookup;
        }
        
        
        [Fact]
        public void ReturnsExistingToc()
        {
            var vt = new Toc("VT");
            
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", vt}
                });

            var toc = lookup.FindOrAdd("VT");
            Assert.Same(vt, toc);
        }
        
        [Fact]
        public void CreatesNewToc()
        {         
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());

            var toc = lookup.FindOrAdd("VT");
            Assert.Equal("VT", toc.Code);
        }

        [Fact]
        public void ReturnsSameTocOnSubsequentCalls()
        {         
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());

            var toc1 = lookup.FindOrAdd("VT");
            var toc2 = lookup.FindOrAdd("VT");
            Assert.Same(toc2, toc1);
        }
        
        [Fact]
        public void EnumeratesAllTocs()
        {
            var i = 0;
            foreach (var toc in TestTocs)
            {
                i++;
                Assert.Contains<Toc>(toc, TestTocs);
            }
            Assert.Equal(3, i);
        }
        
        [Fact]
        public void Count()
        {
            Assert.Equal(3, TestLookup.Count);
        }
        
        [Fact]
        public void GroupByEnumerator()
        {
            var groups = (IEnumerable<IGrouping<string, Toc>>) TestLookup;
            var i = 0;
            foreach (var group in groups)
            {
                foreach (var toc in group)
                {
                     i++;
                     Assert.Contains<Toc>(toc, TestTocs);                   
                }
            }
            Assert.Equal(3, i);
        }
        
        [Fact]
        public void Contains()
        {
            Assert.True(TestLookup.Contains("VT"));
        }
        
        [Fact]
        public void DoesNotContain()
        {
            Assert.False(TestLookup.Contains("ZZ"));
        }
        
        [Fact]
        public void Item()
        {
            var toc = TestLookup["VT"].Single();
            Assert.Equal("VT", toc.Code);

            var ex = Assert.Throws<KeyNotFoundException>(() => TestLookup["ZZ"]);
        }
        
        [Fact]
        public void AddsNewToc()
        {         
            var avanti = new Toc("VT", "Avanti");
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());

            lookup.Add("VT", avanti);
            var toc = lookup["VT"].Single();
            Assert.Same(avanti, toc);
        }
        
        [Fact]
        public void IgnoresAddingExistingToc()
        {
            var avanti = new Toc("VT", "Avanti");
            
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", avanti}
                });

            var virgin = new Toc("VT", "Virgin");
            lookup.Add("VT", virgin);
            var toc = lookup["VT"].Single();
            Assert.Same(avanti, toc);
        }
    }
}