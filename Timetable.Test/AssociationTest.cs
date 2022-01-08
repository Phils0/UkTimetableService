using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Castle.DynamicProxy.Generators.Emitters;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class AssociationTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        private const string MainUid = "X12345";
        private const string AssociatedUid = "A98765";
            
        public static TheoryData<DateTime, string, AssociationDateIndicator, bool> Dates =>
            new TheoryData<DateTime, string, AssociationDateIndicator, bool>()
            {
                { MondayAugust12, MainUid, AssociationDateIndicator.Standard, true }, 
                { MondayAugust12.AddDays(1), MainUid, AssociationDateIndicator.Standard, false },
                { MondayAugust12.AddDays(-1), MainUid, AssociationDateIndicator.Standard, false },
                { MondayAugust12, AssociatedUid, AssociationDateIndicator.Standard, true }, 
                { MondayAugust12.AddDays(1), AssociatedUid, AssociationDateIndicator.Standard, false },
                { MondayAugust12.AddDays(-1), AssociatedUid, AssociationDateIndicator.Standard, false },
                { MondayAugust12, MainUid, AssociationDateIndicator.NextDay, true }, 
                { MondayAugust12.AddDays(1), MainUid, AssociationDateIndicator.NextDay, false },
                { MondayAugust12.AddDays(-1), MainUid, AssociationDateIndicator.NextDay, false },
                { MondayAugust12, AssociatedUid, AssociationDateIndicator.NextDay, false }, 
                { MondayAugust12.AddDays(1), AssociatedUid, AssociationDateIndicator.NextDay, true },
                { MondayAugust12.AddDays(-1), AssociatedUid, AssociationDateIndicator.NextDay, false },                
                { MondayAugust12, MainUid, AssociationDateIndicator.PreviousDay, true }, 
                { MondayAugust12.AddDays(1), MainUid, AssociationDateIndicator.PreviousDay, false },
                { MondayAugust12.AddDays(-1), MainUid, AssociationDateIndicator.PreviousDay, false },
                { MondayAugust12, AssociatedUid, AssociationDateIndicator.PreviousDay, false }, 
                { MondayAugust12.AddDays(1), AssociatedUid, AssociationDateIndicator.PreviousDay, false },
                { MondayAugust12.AddDays(-1), AssociatedUid, AssociationDateIndicator.PreviousDay, true },
            };
        
        [Theory]
        [MemberData(nameof(Dates))]
        public void GetsAssociationAppliesOnDate(DateTime date, string timetableUID, AssociationDateIndicator dateIndicator,  bool expected)
        {
            var association = TestAssociations.CreateAssociation(
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday),
                dateIndicator: dateIndicator);
            
            Assert.Equal(expected, association.AppliesOn(date, timetableUID));
        }

        [Fact]
        public void DoesNotThrowExceptionIsMain()
        {
            var association = TestAssociations.CreateAssociation(
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday),
                dateIndicator: AssociationDateIndicator.None);
            
            Assert.True(association.AppliesOn(MondayAugust12, MainUid));
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
        public void ResolveDate(AssociationDateIndicator indicator, bool isMain, DateTime expected)
        {
            var association = TestAssociations.CreateAssociationWithServices(dateIndicator: indicator);

            var service = isMain ? association.Main.Service : association.Associated.Service;
            Assert.Equal(expected, association.ResolveDate(MondayAugust12, service.TimetableUid));
        }

        [Fact]
        public void ThrowsExceptionWhenDateIndicatorNotSet()
        {
            var association = TestAssociations.CreateAssociation(
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday),
                dateIndicator: AssociationDateIndicator.None);
            
            Assert.Throws<ArgumentException>(() => association.ResolveDate(MondayAugust12, AssociatedUid));
        }
        
        [Theory]
        [InlineData(MainUid)]
        [InlineData(AssociatedUid)]
        public void ResolveDateWhenCancelledWhenNoDateIndicator(string timetableUid)
        {
            var association = TestAssociations.CreateAssociation(
                calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday),
                indicator: StpIndicator.Cancelled,
                dateIndicator: AssociationDateIndicator.None);
            
            Assert.Equal(MondayAugust12, association.ResolveDate(MondayAugust12, timetableUid));
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