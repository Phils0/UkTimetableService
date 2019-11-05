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
            
            association.AddToService(schedule.Service, true);
            Assert.Equal(schedule.Service, association.MainService);
            Assert.Null(association.AssociatedService);
        }
        
        [Fact]
        public void AddToServiceAddAssociatedService()
        {
            var schedule = TestSchedules.CreateScheduleWithService("A98765");
            var association = TestAssociations.CreateAssociation();
            
            association.AddToService(schedule.Service, false);
            Assert.Equal(schedule.Service, association.AssociatedService);
            Assert.Null(association.MainService);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddToServiceThrowsExceptionWhenNotMatchingId(bool isMain)
        {
            var schedule = TestSchedules.CreateScheduleWithService("Q11111");
            var association = TestAssociations.CreateAssociation();
            
            var ex = Assert.Throws<ArgumentException>(() => association.AddToService(schedule.Service, isMain));
        }
    }
}