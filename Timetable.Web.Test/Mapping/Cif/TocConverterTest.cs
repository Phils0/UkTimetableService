using System;
using System.Collections.Generic;
using AutoMapper;
using NSubstitute;
using Serilog;
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
        
        private readonly MapperConfiguration _fromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        private CifSchedule MapSchedule(CifParser.Records.ScheduleExtraData input)
        {
            var mapper = _fromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.ScheduleExtraData, Timetable.CifSchedule>(input, new CifSchedule(), o =>
            {
                o.Items.Add("Tocs", CreateTocLookup());
            });
        }
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
            var schedule = TestSchedules.CreateScheduleExtraDetails(toc: "VT");
            
            var output = MapSchedule(schedule);

            Assert.Same(VT, output.Operator);
        }
        
        [Fact]
        public void ConvertCreatesNewToc()
        {
            var schedule = TestSchedules.CreateScheduleExtraDetails(toc: "SW");
            
            var output = MapSchedule(schedule);
            
            Assert.Equal("SW", output.Operator.Code);
        }
        
        [Fact]
        public void ThrowsExceptionIfDoNotPassTocs()
        {
            var schedule = TestSchedules.CreateScheduleExtraDetails();
            var mapper = _fromCifProfileConfiguration.CreateMapper();

            var  ex = Assert.Throws<AutoMapperMappingException>(() => mapper.Map<CifParser.Records.ScheduleExtraData, Timetable.CifSchedule>(schedule, new CifSchedule()));
            Assert.IsType<ArgumentException>(ex.InnerException);
        }
    }
}