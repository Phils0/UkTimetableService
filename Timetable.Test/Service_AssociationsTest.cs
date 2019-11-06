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

            service.AddAssociation(association, true);

            var associations = service.GetAssociations();
            Assert.NotEmpty( associations["A98765"]);
        }
        
        [Fact]
        public void WhenIsAssociatedAddMainUid()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var association = TestAssociations.CreateAssociation();

            service.AddAssociation(association, false);

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
            
            var associations = service.GetAssociations();
            Assert.Empty(associations["A67890"]);
        }
        
        // Implication of the above
        [Fact]
        public void AddSameAssociationTwiceResultsInNeitherBeingAdded()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var permanent = TestAssociations.CreateAssociation();
            
            service.AddAssociation(permanent, true);
            service.AddAssociation(permanent, true);
            
            var associations = service.GetAssociations();
            Assert.Empty(associations["A98765"]);
        }
        
        [Fact]
        public void DoNotHaveOverlappingAssociationsWithTheSameStpIndicator()
        {
            //TODO
        }
        
        [Fact]
        public void CanAddAssociationWithDifferentTimetableUids()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var uid1 = TestAssociations.CreateAssociation(associatedUid: "A12345");
            var uid2 =  TestAssociations.CreateAssociation(associatedUid: "A67890");
            
            service.AddAssociation(uid1, true);
            service.AddAssociation(uid2, true);

            var associations = service.GetAssociations();
            Assert.Equal(2, associations.Count);
            Assert.Single(associations["A12345"]);
            Assert.Single(associations["A67890"]);
        }
    }
}