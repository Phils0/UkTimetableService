using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ArrivalServiceTimeTest
    {
        private  ScheduleStop CreateScheduleStop()
        {
            var stop = TestScheduleLocations.CreateStop(TestStations.Surbiton, TestSchedules.Ten);
            TestSchedules.CreateScheduleWithService(locations: new ScheduleLocation[] {stop});
            return stop;
        }
        
        private ArrivalServiceTime CreateServiceTime(ScheduleStop stop)
        {
            return new ArrivalServiceTime(stop);
        }
       
        [Fact]
        public void TimeIsSetToPublicArrival()
        {
            var stop = CreateScheduleStop();

            var serviceTime = CreateServiceTime(stop);
            Assert.Equal(TestSchedules.Ten, serviceTime.Time);
        }
        [Fact]
        public void TimeIsSetToWorkingArrivalIfPublicNotSet()
        {
            var stop = CreateScheduleStop();
            stop.Arrival = Time.NotValid;
            
            var serviceTime = CreateServiceTime(stop);
            Assert.Equal(new Time(new TimeSpan(9, 59,30 )), serviceTime.Time);
        }
        
        [Fact]
        public void ServiceIsStopsService()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            var serviceTime = new ArrivalServiceTime(schedule.Locations.Last() as IArrival);
            Assert.Equal(service, serviceTime.Service);
        }
    }
}