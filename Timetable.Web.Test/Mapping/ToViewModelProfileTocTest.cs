using System.Collections.Generic;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileTocTest
    {
        public static readonly MapperConfiguration ToViewProfileConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<ToViewModelProfile>());

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void TocToString()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Toc, string>(TestSchedules.VirginTrains);
            
            Assert.Equal("VT", output);
        }
        
        [Fact]
        public void TocToToc_Code()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Toc, Model.Toc>(TestSchedules.VirginTrains);
            
            Assert.Equal("VT", output.Code);
        }
        
        [Fact]
        public void TocToToc_Name()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Toc, Model.Toc>(TestSchedules.VirginTrains);
            
            Assert.Equal("Virgin Trains", output.Name);
        }
        
        [Fact]
        public void TocToToc_NoName()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var source = new Timetable.Toc("SW");
            var output = mapper.Map<Timetable.Toc, Model.Toc>(source);
            
            Assert.Null(output.Name);
        }
        
        [Fact]
        public void TocToToc_Url()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Toc, Model.Toc>(TestSchedules.VirginTrains);
            
            Assert.Equal("http://www.nationalrail.co.uk/tocs_maps/tocs/VT.aspx", output.NationalRailUrl);
        }
        
        [Fact]
        public void TocToToc_NoUrl()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var source = new Timetable.Toc("SW");
            var output = mapper.Map<Timetable.Toc, Model.Toc>(source);
            
            Assert.Null(output.NationalRailUrl);
        }
    }
}