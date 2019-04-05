using System.Collections.Generic;
using NSubstitute;
using Serilog;
using Xunit;

namespace Timetable.Test
{
    public class TocLookupTest
    {
        [Fact]
        public void ReturnsExistingToc()
        {
            var vt = new Toc() {Code = "VT"};
            
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", vt}
                });

            var toc = lookup.Find("VT");
            Assert.Same(vt, toc);
        }
        
        [Fact]
        public void CreatesNewToc()
        {         
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());

            var toc = lookup.Find("VT");
            Assert.Equal("VT", toc.Code);
        }

        [Fact]
        public void ReturnsSameTocOnSebsequentCalls()
        {         
            var lookup = new TocLookup(Substitute.For<ILogger>(), new Dictionary<string, Toc>());

            var toc1 = lookup.Find("VT");
            var toc2 = lookup.Find("VT");
            Assert.Same(toc2, toc1);
        }
    }
}