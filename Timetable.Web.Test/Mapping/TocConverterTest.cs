using System.Collections.Generic;
using AutoMapper;
using NSubstitute;
using Serilog;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class TocConverterTest
    {
        private static readonly Toc VT = new Toc()
        {
            Code = "VT",
            Name = "Virgin Trains"
        };
        

        [Fact]
        public void ConvertUsesExistingToc()
        {
            var logger = Substitute.For<ILogger>();
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", VT}
                });
            var context = new ResolutionContext(null, null);

            var converter = new TocConverter(lookup);

            var output = converter.Convert("VT", context);

            Assert.Same(VT, output);
        }
        
        [Fact]
        public void ConvertCreatesNewToc()
        {
            var logger = Substitute.For<ILogger>();
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", new Toc() {Code = "VT"}}
                });
            var context = new ResolutionContext(null, null);

            var converter = new TocConverter(lookup);

            var output = converter.Convert("SW", context);

            Assert.NotSame(VT, output);
            Assert.Equal("SW", output.Code);
        }
    }
}