using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;
using Xunit.Sdk;

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

            var associations = GetAssociations(service);
            Assert.NotEmpty( associations["A98765"]);
        }
        
        [Fact]
        public void WhenIsAssociatedAddMainUid()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var association = TestAssociations.CreateAssociation();

            service.AddAssociation(association, false);

            var associations = GetAssociations(service);
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

            var associations = GetAssociations(service);
            Assert.Single(associations);
            Assert.Equal(2, associations["A98765"].Count);
        }

        private static Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>> GetAssociations(Service service)
        {
            return (Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>) service.AsDynamic()._associations.RealObject;
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

            var associations = GetAssociations(service);
            Assert.Single(associations);
            Assert.Equal(2, associations["A98765"].Count);
        }
        
        [Fact]
        public void CannotAddSameAssociationTwice()
        {
            var service = TestSchedules.CreateScheduleWithService().Service;
            var permanent = TestAssociations.CreateAssociation(indicator: StpIndicator.Permanent);
            
            service.AddAssociation(permanent, true);
            var ex = Assert.Throws<ArgumentException>(() =>  service.AddAssociation(permanent, true));
            
            Assert.StartsWith("Association already added", ex.Message);
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

            var associations = GetAssociations(service);
            Assert.Equal(2, associations.Count);
            Assert.Single(associations["A12345"]);
            Assert.Single(associations["A67890"]);
        }
    }
}