using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceWithAssociationsTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void GetsScheduleWithAssociationsApplyingOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var association1 = TestAssociations.CreateAssociationWithServices(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday), mainService: service);
            var association2 = TestAssociations.CreateAssociationWithServices(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as
                    Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>;
            
            var resolved = CreateResolved(MondayAugust12.AddDays(2));

            Assert.True(resolved.HasAssociations());
            Assert.Equal(association1, resolved.Associations[0].Details);
            
            resolved = CreateResolved(MondayAugust12.AddDays(3));
            Assert.True(resolved.HasAssociations());
            Assert.Equal(association2, resolved.Associations[0].Details);
            
            resolved = CreateResolved(MondayAugust12);
            Assert.False(resolved.HasAssociations());

            ResolvedServiceWithAssociations CreateResolved(DateTime onDate)
            {
                return new ResolvedServiceWithAssociations(schedule, onDate, false, associations);
            }
        }
        
        [Theory]
        [InlineData(StpIndicator.Permanent, StpIndicator.New)]
        [InlineData(StpIndicator.Override, StpIndicator.New)]
        [InlineData(StpIndicator.Permanent, StpIndicator.Override)]
        public void ResolveAssociationsHighIndicatorsTakePriorityOverLow(StpIndicator lowIndicator, StpIndicator highIndicator)
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var low = TestAssociations.CreateAssociationWithServices(indicator: lowIndicator, mainService: service);
            var high = TestAssociations.CreateAssociationWithServices(indicator: highIndicator, mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as
                    Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>;
            
            var resolved = new ResolvedServiceWithAssociations(schedule, MondayAugust12, false, associations);
            
            Assert.Equal(high, resolved.Associations[0].Details);
        }
        
        [Theory]
        [InlineData(StpIndicator.Permanent)]
        [InlineData(StpIndicator.Override)]
        [InlineData(StpIndicator.New)]
        public void ResolveAssociationsCancelledAssociationsReturned(StpIndicator lowIndicator)
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var low = TestAssociations.CreateAssociationWithServices(indicator: lowIndicator, mainService: service);
            var cancelled = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Cancelled, mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as 
                    Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>;
            
            var resolved = new ResolvedServiceWithAssociations(schedule, MondayAugust12, false, associations);
            
            Assert.True( resolved.Associations[0].IsCancelled);
            Assert.Equal(low, resolved.Associations[0].Details);
        }
        
        [Fact]
        public void ResolveAssociationsMultipleAssociationsWithCancelReturnsHighestPriority()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var low = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Permanent, mainService: service);
            var high = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Override, mainService: service);
            var cancelled = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Cancelled, mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as 
                    Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>;
            
            var resolved = new ResolvedServiceWithAssociations(schedule, MondayAugust12, false, associations);

            Assert.True( resolved.Associations[0].IsCancelled);
            Assert.Equal(high, resolved.Associations[0].Details);
        }
        
        [Fact]
        public void ResolveAssociationsMultipleAssociationsWithDifferentServices()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var association1 = TestAssociations.CreateAssociationWithServices(mainService: service, associatedUid:"A12345");
            var association2 = TestAssociations.CreateAssociationWithServices(mainService: service, associatedUid:"A67890");
            var associations = service.AsDynamic()._associations.RealObject as 
                Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>;
            
            var resolved = new ResolvedServiceWithAssociations(schedule, MondayAugust12, false, associations);

            Assert.Equal(2, resolved.Associations.Length);
            
            Assert.Equal(association1, resolved.Associations[0].Details);
            Assert.Equal(association2, resolved.Associations[1].Details);
        }
    }
}