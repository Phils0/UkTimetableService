using System;
using System.Collections.Generic;
using AutoMapper;
using Xunit;
using Timetable.Test.Data;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileScheduleToFoundServiceItemTest
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
        public void MapService()
        {
            var output = MapResolvedStop();
            var service = output.Service;
            Assert.Equal("X12345", service.TimetableUid);
            Assert.Equal(TestDate, service.Date);
        }

        private static Model.FoundServiceItem MapResolvedStop(Timetable.Schedule schedule = null, 
            Timetable.Station at = null, Time? time = null,
            Timetable.Station from = null, Timetable.Station to = null)
        {
            schedule = schedule ?? TestSchedules.CreateScheduleWithService();
            at = at ?? TestStations.Surbiton;
            time = time ?? TestSchedules.Ten;
            var find = new StopSpecification(at, time.Value, TestDate, TimesToUse.Departures);
            
            var resolved = new ResolvedService(schedule, TestDate, false);
            resolved.TryFindStop(find, out var stop);
            if(to != null)
                stop.GoesTo(to);
            if(from != null)
                stop.ComesFrom(from);
            
            var mapper = ToViewProfileConfiguration.CreateMapper();
            return mapper.Map<Timetable.ResolvedServiceStop, Model.FoundServiceItem>(stop, opts => opts.Items["On"] = stop.On);
        }

        [Fact]
        public void MapAssociation()
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();
            var resolved = TestSchedules.CreateServiceWithAssociation();
            var find = new StopSpecification(TestStations.Surbiton, TestSchedules.Ten, TestDate, TimesToUse.Departures);
            resolved.TryFindStop(find, out var stop);
            
            var item = mapper.Map<Timetable.ResolvedServiceStop, Model.FoundServiceItem>(stop, opts => opts.Items["On"] = stop.On);
            
            Assert.NotEmpty(item.Service.Associations);
            Assert.False(item.Association.IsIncluded);
        }

        public static IEnumerable<object[]> JoinToStations
        {
            get
            {
                yield return new object[] {TestStations.Weybridge, false};   
                yield return new object[] {TestStations.ClaphamJunction, false};
                yield return new object[] {TestStations.Waterloo, true};
            }
        }
        
        [Theory]
        [MemberData(nameof(JoinToStations))]
        public void MapIncludeAssociation(Station station, bool expected)
        {
            var mapper = ToViewProfileConfiguration.CreateMapper();

            var association = CreateJoinServices();
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, TestDate, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(woking, out var stop);
            stop.GoesTo(station);
            
            var item = mapper.Map<Timetable.ResolvedServiceStop, Model.FoundServiceItem>(stop, opts => opts.Items["On"] = stop.On);
            
            Assert.NotEmpty(item.Service.Associations);
            Assert.Equal(expected, item.Association.IsIncluded);
            if(expected)
                Assert.Equal("X12345", item.Association.TimetableUid);
        }
        
        private Association CreateJoinServices()
        {
            var main = TestSchedules.CreateScheduleWithService("X12345", retailServiceId: "VT123401").Service;
            var associated = TestSchedules.CreateScheduleWithService("A98765", retailServiceId: "VT123402",
                stops: TestSchedules.CreateWokingClaphamSchedule(TestSchedules.NineForty)).Service;
            var association = TestAssociations.CreateAssociationWithServices(main, associated);
            return association;
        }
        
        [Fact]
        public void MapStop()
        {
            var output = MapResolvedStop();            
            var stop = output.At;
            Assert.Equal("SUR", stop.Location.ThreeLetterCode);
            Assert.Equal(TestDate, stop.Departure.Value.Date);
        }
        
        [Fact]
        public void MapToStop()
        {
            var output = MapResolvedStop(to: TestStations.Waterloo);            
            var stop = output.To;
            Assert.Equal("WAT", stop.Location.ThreeLetterCode);
            Assert.Equal(TestDate, stop.Arrival.Value.Date);
        }
        
        [Fact]
        public void MapFromStop()
        {
            var output = MapResolvedStop(at: TestStations.ClaphamJunction, time: TestSchedules.TenSixteen,  from: TestStations.Surbiton);            
            var stop = output.From;
            Assert.Equal("SUR", stop.Location.ThreeLetterCode);
            Assert.Equal(TestDate, stop.Departure.Value.Date);
        }
    }
}