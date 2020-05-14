using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class AssociationTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        
        [Fact]
        public void GetsAssociationAppliesOnDate()
        {
            var association = TestAssociations.CreateAssociation(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            Assert.True(association.AppliesOn(MondayAugust12));
            Assert.False(association.AppliesOn(MondayAugust12.AddDays(1)));
        }
        
        [Theory]
        [InlineData(StpIndicator.Cancelled, true)]
        [InlineData(StpIndicator.New, false)]
        [InlineData(StpIndicator.Override, false)]
        [InlineData(StpIndicator.Permanent, false)]
        public void IsCancelled(StpIndicator indicator, bool expected)
        {
            var association = TestAssociations.CreateAssociation(indicator: indicator);
            Assert.Equal(expected, association.IsCancelled());
        }
        
        [Fact]
        public void AddToServiceAddMainService()
        {
            var schedule = TestSchedules.CreateScheduleWithService();
            var association = TestAssociations.CreateAssociation();
            
            association.SetService(schedule.Service, true);
            Assert.Equal(schedule.Service, association.Main.Service);
            Assert.Null(association.Associated.Service);
        }
        
        [Fact]
        public void AddToServiceAddAssociatedService()
        {
            var schedule = TestSchedules.CreateScheduleWithService("A98765");
            var association = TestAssociations.CreateAssociation();
            
            association.SetService(schedule.Service, false);
            Assert.Equal(schedule.Service, association.Associated.Service);
            Assert.Null(association.Main.Service);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddToServiceThrowsExceptionWhenNotMatchingId(bool isMain)
        {
            var schedule = TestSchedules.CreateScheduleWithService("Q11111");
            var association = TestAssociations.CreateAssociation();
            
            var ex = Assert.Throws<ArgumentException>(() => association.SetService(schedule.Service, isMain));
        }
        
        [Theory]
        [InlineData("T-U", AssociationCategory.Join, true)]
        [InlineData("T", AssociationCategory.Join, false)]
        [InlineData("TB", AssociationCategory.Join, false)]
        [InlineData("TF", AssociationCategory.Join, false)]
        [InlineData("T-D", AssociationCategory.Split, true)]
        [InlineData("T", AssociationCategory.Split, false)]
        [InlineData("TB", AssociationCategory.Split, false)]
        [InlineData("TF", AssociationCategory.Split, false)]
        public void HasConsistentLocationOnMain(string activities, AssociationCategory joinSplit, bool expected)
        {
            var stops = AssociationDictionaryTest.CreateStopsSettingClaphamActivities(activities);
            var schedule = TestSchedules.CreateScheduleWithService(stops: stops);
            var association = TestAssociations.CreateAssociation(category: joinSplit);
            
            Assert.Equal(expected, association.HasConsistentLocation(schedule, true));
        }
        
        [Theory]
        [InlineData("T-U", AssociationCategory.Join, false)]
        [InlineData("T", AssociationCategory.Join, false)]
        [InlineData("TB", AssociationCategory.Join, false)]
        [InlineData("TF", AssociationCategory.Join, true)]
        [InlineData("T-D", AssociationCategory.Split, false)]
        [InlineData("T", AssociationCategory.Split, false)]
        [InlineData("TB", AssociationCategory.Split, true)]
        [InlineData("TF", AssociationCategory.Split, false)]
        public void HasConsistentLocationOnAssociated(string activities, AssociationCategory joinSplit, bool expected)
        {
            var stops = AssociationDictionaryTest.CreateAssociateStopsSettingClaphamActivities(joinSplit, activities);
            var schedule = TestSchedules.CreateScheduleWithService(stops: stops);
            var association = TestAssociations.CreateAssociation(category: joinSplit);
            
            Assert.Equal(expected, association.HasConsistentLocation(schedule, false));
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NotConsistentLocationWhenCannotFindTheStop(bool isMain)
        {
            var association = TestAssociations.CreateAssociation(location: TestLocations.Weybridge);
            var schedule = TestSchedules.CreateScheduleWithService();
            
            Assert.False(association.HasConsistentLocation(schedule, isMain));
        }
    }
}