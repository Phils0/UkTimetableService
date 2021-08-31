using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class Service_AssociationsTest
    {
        [Fact]
        public void WhenIsMainAddsAssociatedUid()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var association = TestAssociations.CreateAssociation();

            Assert.True(service.AddAssociation(association, true));
            Assert.True(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.NotEmpty( associations["A98765"]);
        }
        
        [Fact]
        public void WhenIsAssociatedAddMainUid()
        {
            var service = TestSchedules.CreateScheduleWithService("A98765").Service;
            var association = TestAssociations.CreateAssociation();

            Assert.True(service.AddAssociation(association, false));
            Assert.True(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.NotEmpty( associations["X12345"]);
        }
        
        [Fact]
        public void CanAddAssociationWithDifferentStpIndicator()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var permanent = TestAssociations.CreateAssociation(indicator: StpIndicator.Permanent);
            var overlay =  TestAssociations.CreateAssociation(indicator: StpIndicator.Override);
            
            service.AddAssociation(permanent, true);
            service.AddAssociation(overlay, true);

            Assert.True(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Single(associations);
            Assert.Equal(2, associations["A98765"].Count);
        }

        // It happens we order by the calendar as need a unique order for the SortedList but could be anything that creates uniqueness
        [Fact]
        public void CanAddAssociationsWithSameStpIndicator()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var permanent = TestAssociations.CreateAssociation(indicator: StpIndicator.Permanent);
            var permanent2 = TestAssociations.CreateAssociation(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            service.AddAssociation(permanent, true);
            service.AddAssociation(permanent2, true);

            Assert.True(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Single(associations);
            Assert.Equal(2, associations["A98765"].Count);
        }
        
        // This is very dubious currently occurs once in the industry data 
        // AANW80860W838501905201912131111100JJSRAMSGTE  TP                               P
        // AANW83850W808601905201912131111100VVSRAMSGTE  TP                               P
        // Remove both
        [Fact]
        public void AddReversedAssociationTwiceResultsInNeitherBeingAdded()
        {
            var service = TestSchedules.CreateScheduleWithService("A12345").Service;
            var association = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890");
            var reversed = TestAssociations.CreateAssociation(mainUid: "A67890", associatedUid: "A12345");
           
            service.AddAssociation(association, true);
            service.AddAssociation(reversed, false);
            
            Assert.False(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Empty(associations);
        }
        
        // Implication of the above
        [Fact]
        public void AddSameAssociationTwiceResultsInNeitherBeingAdded()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var permanent = TestAssociations.CreateAssociation();
            
            service.AddAssociation(permanent, true);
            service.AddAssociation(permanent, true);
            
            Assert.False(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Empty(associations);
        }
        
        [Fact]
        public void FailToAddAssociation()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var association = TestAssociations.CreateAssociation();

            service.AddAssociation(association, true);
            // Adding same association can result in failing to add
            Assert.False(service.AddAssociation(association, true));
            Assert.False(service.HasAssociations());
        }
        
        [Fact(Skip="Not currently implemented")]
        public void DoNotHaveOverlappingAssociationsWithTheSameStpIndicator()
        {
            var everydayIn2019 =
                TestSchedules.CreateEverydayCalendar(new DateTime(2019, 1, 1), new DateTime(2019, 12, 31));
            var service = TestSchedules.CreateScheduleWithService("A12345").Service;
            var association = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890");
            var overlapping = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890", calendar: everydayIn2019);
           
            service.AddAssociation(association, true);
            service.AddAssociation(overlapping, true);
            
            Assert.False(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Empty(associations);
        }
        
        [Fact]
        public void CanAddAssociationWithDifferentTimetableUids()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var uid1 = TestAssociations.CreateAssociation(associatedUid: "A12345");
            var uid2 =  TestAssociations.CreateAssociation(associatedUid: "A67890");
            
            service.AddAssociation(uid1, true);
            service.AddAssociation(uid2, true);

            Assert.True(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Equal(2, associations.Count);
            Assert.Single(associations["A12345"]);
            Assert.Single(associations["A67890"]);
        }
        
        [Fact]
        public void HasAssociationsFalseWhenNoAssociations()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            
            Assert.False(service.HasAssociations());
        }
        
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        private static readonly DateTime TuesdayAugust13 = new DateTime(2019, 8, 13);
        
        [Fact]
        public void NoAssociationsReturnsResolvedService()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
            
            service.TryResolveOn(MondayAugust12, out var found);
            Assert.IsType<ResolvedService>(found);
        }
        
        [Fact]
        public void AssociationsReturnsResolvedServiceWithAssociations()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var service = schedule.Service;
            
            var association = TestAssociations.CreateAssociationWithServices(mainService: service);

            service.TryResolveOn(MondayAugust12, out var found);
            Assert.IsType<ResolvedServiceWithAssociations>(found);
        }
        
        [Fact]
        public void AssociationsResolvedWithAssociationOnNextDay()
        {
            var mainCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday);
            var mainSchedule = TestSchedules.CreateScheduleWithService(calendar: mainCalendar);
            var mainService = mainSchedule.Service;
            
            var associationCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday);
            var associationSchedule = TestSchedules.CreateScheduleWithService("Z98765", calendar: associationCalendar);
            var associationService = associationSchedule.Service;
            
            var association = TestAssociations.CreateAssociationWithServices(
                mainService: mainService, 
                associatedService: associationService, 
                dateIndicator: AssociationDateIndicator.NextDay);

            mainService.TryResolveOn(MondayAugust12, out var found);
            Assert.IsType<ResolvedServiceWithAssociations>(found);
        }
        
        [Fact]
        public void AssociationsResolvedWithAssociationOnPreviousDayWhenOnAssociatedService()
        {
            var mainCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday);
            var mainSchedule = TestSchedules.CreateScheduleWithService(calendar: mainCalendar);
            var mainService = mainSchedule.Service;
            
            var associationCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday);
            var associationSchedule = TestSchedules.CreateScheduleWithService("Z98765", calendar: associationCalendar);
            var associationService = associationSchedule.Service;
            
            var association = TestAssociations.CreateAssociationWithServices(
                mainService: mainService, 
                associatedService: associationService, 
                dateIndicator: AssociationDateIndicator.NextDay);

            associationService.TryResolveOn(TuesdayAugust13, out var found);
            Assert.IsType<ResolvedServiceWithAssociations>(found);
        }
        
        [Fact]
        public void AssociationsResolvedWithAssociationOnPreviousDayWhenOnAssociatedServiceAndCalendarIsMainService()
        {
            var mainCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday);
            var mainSchedule = TestSchedules.CreateScheduleWithService(calendar: mainCalendar);
            var mainService = mainSchedule.Service;
            
            var associationCalendar = TestSchedules.CreateAugust2019Calendar(DaysFlag.Tuesday);
            var associationSchedule = TestSchedules.CreateScheduleWithService("Z98765", calendar: associationCalendar);
            var associationService = associationSchedule.Service;
            
            var association = TestAssociations.CreateAssociationWithServices(
                mainService: mainService, 
                associatedService: associationService, 
                calendar: mainCalendar,
                dateIndicator: AssociationDateIndicator.NextDay);

            associationService.TryResolveOn(TuesdayAugust13, out var found);
            Assert.IsType<ResolvedServiceWithAssociations>(found);
        }
    }
}