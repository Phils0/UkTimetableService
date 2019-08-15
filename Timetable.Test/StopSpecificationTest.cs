using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class StopSpecificationTest
    {
        [Theory]
        [InlineData(TimesToUse.Arrivals, true)]
        [InlineData(TimesToUse.Departures, false)]
        public void UseArrivalBasedUponTimesToUse(TimesToUse use, bool expected)
        {
            var spec = Create(use);
            Assert.Equal(expected, spec.UseArrival);
        }

        private StopSpecification Create(TimesToUse use)
        {
            return new StopSpecification(TestStations.Surbiton, Time.NotValid, DateTime.Today, use);
        }
        
        [Theory]
        [InlineData(TimesToUse.Arrivals, false)]
        [InlineData(TimesToUse.Departures, true)]
        public void UseDepartureBasedUponTimesToUse(TimesToUse use, bool expected)
        {
            var spec = Create(use);
            Assert.Equal(expected, spec.UseDeparture);
        }
        
        [Fact]
        public void MoveToPreviousDayOnlyChangesServiceDate()
        {
            var origSpec = Create(TimesToUse.Arrivals);

            var expectedDate = DateTime.Today.AddDays(-1);

            var spec = origSpec.MoveToPreviousDay();
            
            Assert.Equal(expectedDate, spec.OnDate);
            Assert.Equal(origSpec.Location, spec.Location);
            Assert.Equal(origSpec.Time, spec.Time);
            Assert.Equal(origSpec.UseDeparture, spec.UseDeparture);
        }
    }
}