using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class AssociationDictionaryTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Fact]
        public void ResolveAssociationsApplyingOnDate()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var association1 = TestAssociations.CreateAssociationWithServices(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Wednesday), mainService: service);
            var association2 = TestAssociations.CreateAssociationWithServices(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Thursday), mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;

            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12.AddDays(2));

            Assert.NotEmpty(resolved);
            Assert.Equal(association1, resolved[0].Details);
            
            resolved = associations.Resolve(service.TimetableUid,MondayAugust12.AddDays(3));
            Assert.NotEmpty(resolved);
            Assert.Equal(association2, resolved[0].Details);
            
            resolved = associations.Resolve(service.TimetableUid,MondayAugust12);
            Assert.Empty(resolved);
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
            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12);
            Assert.Equal(high, resolved[0].Details);
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
            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12);
            
            Assert.True( resolved[0].IsCancelled);
            Assert.Equal(low, resolved[0].Details);
        }
        
        [Fact]
        public void ResolveAssociationsMultipleAssociationsWithCancelReturnsHighestPriority()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var low = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Permanent, mainService: service);
            var high = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Override, mainService: service);
            var cancelled = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Cancelled, mainService: service);
            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12);

            Assert.True( resolved[0].IsCancelled);
            Assert.Equal(high, resolved[0].Details);
        }
        
        [Fact]
        public void ResolveAssociationsMultipleAssociationsWithDifferentServices()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
  
            var association1 = TestAssociations.CreateAssociationWithServices(mainService: service, associatedUid:"A12345");
            var association2 = TestAssociations.CreateAssociationWithServices(mainService: service, associatedUid:"A67890");
            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;
            
            var resolved =associations.Resolve(service.TimetableUid, MondayAugust12);

            Assert.Equal(2, resolved.Length);
            
            Assert.Equal(association1, resolved[0].Details);
            Assert.Equal(association2, resolved[1].Details);
        }
        
        public static TheoryData<AssociationDateIndicator, bool, DateTime> DataIndicatorScenarios =>
            new TheoryData<AssociationDateIndicator, bool, DateTime>()
            {
                { AssociationDateIndicator.Standard, true, MondayAugust12 }, 
                { AssociationDateIndicator.Standard, false, MondayAugust12 }, 
                { AssociationDateIndicator.PreviousDay, true, MondayAugust12.AddDays(-1) }, 
                { AssociationDateIndicator.PreviousDay, false, MondayAugust12.AddDays(1) },
                { AssociationDateIndicator.NextDay, true, MondayAugust12.AddDays(1) }, 
                { AssociationDateIndicator.NextDay, false, MondayAugust12.AddDays(-1) },                 
            };
        
        [Theory]
        [MemberData(nameof(DataIndicatorScenarios))]
        public void GetsScheduleWithAssociationsApplyingMovingDateBAsedUponDateIndicator(AssociationDateIndicator indicator, bool isMain, DateTime expected)
        {
            
            var association = TestAssociations.CreateAssociationWithServices(dateIndicator: indicator);

            var service = isMain ? association.Main.Service : association.Associated.Service;
            var other = isMain ? association.Associated.Service : association.Main.Service;
            
            var resolved = Associations(other.TimetableUid).Resolve(service.TimetableUid, MondayAugust12);

            Assert.NotEmpty(resolved);
            Assert.Equal(expected, resolved[0].AssociatedService.On);

            AssociationDictionary Associations(string uid)
            {
                var values = new SortedList<(StpIndicator indicator, ICalendar calendar), Association>();
                values.Add((StpIndicator.Permanent, association.Calendar), association);
                return new AssociationDictionary(1)
                {
                    {uid, values}
                };
            }
        }
    }
}