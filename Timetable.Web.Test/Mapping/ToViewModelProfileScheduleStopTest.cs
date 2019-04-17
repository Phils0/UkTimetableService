using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleStopTest
    {
        private static readonly DateTime TestDate = TestTime.August1;

        private static readonly MapperConfiguration ToViewProfileConfiguration = 
            ToViewModelProfileLocationTest.ToViewProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapScheduleLocation()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var output = mapper.Map<Timetable.Location, Model.ScheduleLocation>(TestLocations.Surbiton);
            
            Assert.Equal("SURBITN", output.Tiploc);
            Assert.Equal("SUR", output.ThreeLetterCode);
        }
        
        [Fact]
        public void MapScheduleOrigin()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var input = TestScheduleLocations.CreateOrigin(
                TestLocations.Surbiton,
                new Time(TestTime.Ten));
            
            var output = Map(input);
            
            Assert.Equal("SUR", output.Location.ThreeLetterCode);
            var expected = TestDate.Add(TestTime.Ten);
            Assert.Equal(expected, output.Departure);
            Assert.Equal("1", output.Platform);
            Assert.Equal("TB",  output.Activities[0]);
            Assert.Null(output.PassesAt);
            Assert.Null(output.Arrival);
        }
        
        private static Model.ScheduledStop Map(IScheduleLocation stop)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            return (Model.ScheduledStop) mapper.Map(stop, stop.GetType(), typeof(Model.ScheduledStop),
                o => { o.Items["On"] = TestDate; });
        }
        
        [Fact]
        public void MapScheduleDestination()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var input = TestScheduleLocations.CreateDestination(
                TestLocations.WaterlooMain,
                new Time(TestTime.Ten));
            
            var output = Map(input);
            
            Assert.Equal("WAT", output.Location.ThreeLetterCode);
            var expected = TestDate.Add(TestTime.Ten);
            Assert.Equal(expected, output.Arrival);
            Assert.Equal("2", output.Platform);
            Assert.Equal("TF",  output.Activities[0]);
            Assert.Null(output.PassesAt);
            Assert.Null(output.Departure);
        }
        
        [Fact]
        public void MapScheduleStop()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var input = TestScheduleLocations.CreateStop(
                TestLocations.Surbiton,
                new Time(TestTime.Ten));
            
            var output = Map(input);
            
            Assert.Equal("SUR", output.Location.ThreeLetterCode);
            var expected = TestDate.Add(TestTime.Ten);
            Assert.Equal(expected, output.Arrival);
            var expectedDeparture = expected.AddMinutes(1);
            Assert.Equal(expectedDeparture, output.Departure);
            Assert.Equal("10", output.Platform);
            Assert.Equal("T",  output.Activities[0]);
            Assert.Null(output.PassesAt);
        }
        
        [Fact]
        public void MapSchedulePass()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var input = TestScheduleLocations.CreatePass(
                TestLocations.CLPHMJN,
                new Time(TestTime.Ten));
            
            var output = Map(input);
            
            Assert.Equal("CLJ", output.Location.ThreeLetterCode);
            var expected = TestDate.Add(TestTime.Ten);
            Assert.Equal(expected, output.PassesAt);
            Assert.Equal("", output.Platform);
            Assert.Empty(output.Activities);
            Assert.Null(output.Arrival);
            Assert.Null(output.Departure);
        }
        
        [Fact]
        public void ReturnNullWhenInvalidTime()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var input = TestScheduleLocations.CreateStop(
                TestLocations.Surbiton,
                Time.NotValid);
            
            var output = Map(input);
            
            Assert.Null(output.Arrival);
        }
    }
}