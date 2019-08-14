using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceTest
    {
        private static readonly DateTime TestDate = new DateTime(2019, 8, 12);
        
        [Theory]
        [InlineData(true, "X12345 12/08/2019 CANCELLED")]
        [InlineData(false, "X12345 12/08/2019")]
        public void ToStringReturnsTimetableUidAndDate(bool isCancelled, string expected)
        {
            var service =  new ResolvedService(TestSchedules.CreateSchedule(), TestDate, isCancelled);

            Assert.Equal(expected, service.ToString());
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FindStopIsIndependentOfWhetherCancelled(bool isCancelled)
        {
            var service =  new ResolvedService(TestSchedules.CreateSchedule(), DateTime.Today, isCancelled);

            var find = new StopSpecification(TestStations.Surbiton, TestSchedules.Ten, TestDate);
            Assert.True(service.TryFindStop(find, out var stop));
            Assert.NotNull(stop);
        }
        
        [Fact]
        public void DoNotFindStop()
        {
            var service =  new ResolvedService(TestSchedules.CreateSchedule(), DateTime.Today, false);

            var find = new StopSpecification(TestStations.Surbiton, TestSchedules.TenThirty, TestDate);
            Assert.False(service.TryFindStop(find, out var stop));
            Assert.Null(stop);
        }
    }
}