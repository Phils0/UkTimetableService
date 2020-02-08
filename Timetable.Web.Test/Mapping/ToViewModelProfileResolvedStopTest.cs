using System;
using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileResolvedStopTest
    {
        private static readonly DateTime TestDate = TestTime.August1;

        private static readonly MapperConfiguration ToViewProfileConfiguration = 
            ToViewModelProfileLocationTest.ToViewProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }
        
        private static Model.ScheduledStop Map(ResolvedStop stop)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            return (Model.ScheduledStop) mapper.Map(stop, stop.GetType(), typeof(Model.ScheduledStop),
                o => { o.Items["On"] = TestDate; });
        }
        
        [Fact]
        public void MapStop()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var stopDate = TestDate.AddDays(1);
            var input = new ResolvedStop(
                TestScheduleLocations.CreateStop(
                    TestLocations.Surbiton,
                    new Time(TestTime.Ten)),
                stopDate);
            
            
            var output = Map(input);
            
            Assert.Equal("SUR", output.Location.ThreeLetterCode);
            var expected = stopDate.Add(TestTime.Ten);
            Assert.Equal(expected, output.Arrival);
            var expectedDeparture = expected.AddMinutes(1);
            Assert.Equal(expectedDeparture, output.Departure);
            Assert.Equal("10", output.Platform);
            Assert.Equal("T",  output.Activities[0]);
            Assert.Null(output.PassesAt);
        }
    }
}