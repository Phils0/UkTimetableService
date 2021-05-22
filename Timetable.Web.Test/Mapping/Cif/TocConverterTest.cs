using System;
using System.Collections.Generic;
using AutoMapper;
using NSubstitute;
using Serilog;
using Timetable.Web.Mapping.Cif;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class TocConverterTest
    {
        private static readonly Toc VT = new Toc("VT")
        {
            Name = "Virgin Trains"
        };
        
        private static TocLookup CreateTocLookup()
        {
            var lookup = new TocLookup(Substitute.For<ILogger>(),
                new Dictionary<string, Toc>()
                {
                    {"VT", VT}
                });
            return lookup;
        }
        
        [Fact]
        public void ConvertUsesExistingToc()
        {
            var converter = new TocConverter();
            Assert.Same(VT, converter.Convert("VT", CreateTocLookup()));
        }
        
        [Fact]
        public void ConvertCreatesNewToc()
        {
            var converter = new TocConverter();
            var toc = converter.Convert("SW", CreateTocLookup());
            
            Assert.Equal("SW", toc.Code);
        }
    }
}