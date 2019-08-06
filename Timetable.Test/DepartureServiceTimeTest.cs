using System;
using System.Linq;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class DepartureServiceTimeTest
    {
        private  ScheduleStop CreateScheduleStop()
        {
            var stop = TestScheduleLocations.CreateStop(TestStations.Surbiton, TestSchedules.Ten);
            TestSchedules.CreateScheduleWithService(locations: new ScheduleLocation[] {stop});
            return stop;
        }
        
        private DepartureServiceTime CreateServiceTime(ScheduleStop stop)
        {
            return new DepartureServiceTime(stop);
        }
        
        [Fact]
        public void TimeIsSetToPublicArrival()
        {
            var stop = CreateScheduleStop();

            var serviceTime = CreateServiceTime(stop);
            Assert.Equal(new Time(new TimeSpan(10, 01,00 )), serviceTime.Time);
        }
        [Fact]
        public void TimeIsSetToWorkingArrivalIfPublicNotSet()
        {
            var stop = CreateScheduleStop();
            stop.Departure = Time.NotValid;
            
            var serviceTime = CreateServiceTime(stop);
            Assert.Equal(new Time(new TimeSpan(10, 00,30 )), serviceTime.Time);
        }
        
        [Fact]
        public void ServiceIsStopsService()
        {
            var schedule = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday));
            var service = schedule.Service;
            
            var serviceTime = new DepartureServiceTime(schedule.Locations.First() as IDeparture);
            Assert.Equal(service, serviceTime.Service);
        }


    }
}