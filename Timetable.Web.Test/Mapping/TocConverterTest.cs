using System;
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
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", VT}
                });
            var context = new ResolutionContext(new MappingOperationOptions<string, Toc>(null), Substitute.For<IRuntimeMapper>());
            context.Items.Add("Tocs", lookup);
            
            var converter = new TocConverter();

            var output = converter.Convert("VT", context);

            Assert.Same(VT, output);
        }
        
        [Fact]
        public void ConvertCreatesNewToc()
        {
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", new Toc() {Code = "VT"}}
                });
            var context = new ResolutionContext(new MappingOperationOptions<string, Toc>(null), Substitute.For<IRuntimeMapper>());
            context.Items.Add("Tocs", lookup);

            var converter = new TocConverter();
            var output = converter.Convert("SW", context);

            Assert.NotSame(VT, output);
            Assert.Equal("SW", output.Code);
        }
        
        [Fact]
        public void ThrowsExceptionIfDoNotPassTocs()
        {
            var context = new ResolutionContext(new MappingOperationOptions<string, Toc>(null), Substitute.For<IRuntimeMapper>());
            
            var converter = new TocConverter();

            Assert.Throws<ArgumentException>(() => converter.Convert("VT", context)) ;
        }
    }
}