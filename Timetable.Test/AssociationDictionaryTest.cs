using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using ReflectionMagic;
using Serilog;
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

            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12.AddDays(2), schedule.NrsRetailServiceId);

            Assert.NotEmpty(resolved);
            Assert.Equal(association1, resolved[0].Details);
            
            resolved = associations.Resolve(service.TimetableUid,MondayAugust12.AddDays(3), schedule.NrsRetailServiceId);
            Assert.NotEmpty(resolved);
            Assert.Equal(association2, resolved[0].Details);
            
            resolved = associations.Resolve(service.TimetableUid,MondayAugust12, schedule.NrsRetailServiceId);
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
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12, schedule.NrsRetailServiceId);
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
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12, schedule.NrsRetailServiceId);
            
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
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12, schedule.NrsRetailServiceId);

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
            
            var resolved =associations.Resolve(service.TimetableUid, MondayAugust12, schedule.NrsRetailServiceId);

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
        public void GetsScheduleWithAssociationsApplyingMovingDateBasedUponDateIndicator(AssociationDateIndicator indicator, bool isMain, DateTime expected)
        {
            
            var association = TestAssociations.CreateAssociationWithServices(dateIndicator: indicator);

            var service = isMain ? association.Main.Service : association.Associated.Service;
            var other = isMain ? association.Associated.Service : association.Main.Service;
            
            var resolved = Associations(other.TimetableUid).Resolve(service.TimetableUid, MondayAugust12, "VT123401");

            Assert.NotEmpty(resolved);
            Assert.Equal(expected, resolved[0].AssociatedService.On);

            AssociationDictionary Associations(string uid)
            {
                var values = new SortedList<(StpIndicator indicator, ICalendar calendar), Association>();
                values.Add((StpIndicator.Permanent, association.Calendar), association);
                return new AssociationDictionary(1, Substitute.For<ILogger>())
                {
                    {uid, values}
                };
            }
        }
        
        [Theory]
        [InlineData("VT123402", true)]
        [InlineData("VT567802", true)]    // Allow associations with different Retail Service Ids example LIV-EUS Saturdays 20:04 LNWR
        public void ResolveAssociationsWithDifferentRetailServiceId(string associatedRetailServiceId, bool hasAssociation)
        {
            var schedule = TestSchedules.CreateScheduleWithService(retailServiceId: "VT123401");
            var service = schedule.Service;
  
            var association = TestAssociations.CreateAssociationWithServices(indicator: StpIndicator.Permanent, mainService: service, retailServiceId: associatedRetailServiceId);

            var associations = service.AsDynamic()._associations.RealObject as AssociationDictionary;
            
            var resolved = associations.Resolve(service.TimetableUid, MondayAugust12, schedule.NrsRetailServiceId);
            
            Assert.Equal(hasAssociation, resolved.Any());
        }
        
        [Fact]
        public void HandleBadAssociationWhereOtherServiceDoesNotResolve()
        {
            var main = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var other = TestSchedules.CreateScheduleWithService("Z98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var association = TestAssociations.CreateAssociationWithServices(
                mainService: main.Service,
                associatedService: other.Service,    
                dateIndicator: AssociationDateIndicator.NextDay);    // Next day means other schedule will not resolve

            var associations = main.Service.AsDynamic()._associations.RealObject as AssociationDictionary;

            var resolved = associations.Resolve(main.TimetableUid, MondayAugust12, main.Properties.RetailServiceId);
            Assert.Empty(resolved);
        }
        
        [Fact]
        public void HandleWhenAssociationHasNoServiceDoesNotResolve()
        {
            var main = TestSchedules.CreateScheduleWithService(calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            var other = TestSchedules.CreateScheduleWithService("Z98765", calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday));
            
            var association = TestAssociations.CreateAssociationWithServices(
                mainService: main.Service,
                associatedService: other.Service);    // Next day means other schedule will not resolve
                        
            association.Associated.AsDynamic().Service = null;  // Force the service to be null which should nt happen but can do
            
            var associations = main.Service.AsDynamic()._associations.RealObject as AssociationDictionary;

            var resolved = associations.Resolve(main.TimetableUid, MondayAugust12, main.Properties.RetailServiceId);
            Assert.Empty(resolved);
        }
        
        [Fact]
        public void AddSameAssociationTwiceResultsInNeitherBeingAdded()
        {
            var service = TestSchedules.CreateScheduleWithService("A12345").Service;
            var association1 = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890");
            var association2 = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890");
           
            service.AddAssociation(association1, true);
            service.AddAssociation(association2, true);
            
            Assert.False(service.HasAssociations());
            var associations = service.GetAssociations();
            Assert.Empty(associations);
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
        public void AddSameAssociationWithDifferentLocationsResolveToCorrectForMain(string activities, AssociationCategory joinSplit, bool expectedHasAssociation)
        {
            Test(TestLocations.CLPHMJN, TestLocations.WaterlooMain);
            Test(TestLocations.WaterlooMain, TestLocations.CLPHMJN);

            void Test(Location location1, Location location2)
            {
                var stops = CreateStopsSettingClaphamActivities(activities);
                var service = TestSchedules.CreateScheduleWithService("A12345", stops: stops).Service;
                var association1 = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890",
                    category: joinSplit, location: location1);
                var association2 = TestAssociations.CreateAssociation(mainUid: "A12345", associatedUid: "A67890",
                    category: joinSplit, location: location2);

                service.AddAssociation(association1, true);
                service.AddAssociation(association2, true);

                Assert.Equal(expectedHasAssociation, service.HasAssociations());
                if (expectedHasAssociation)
                {
                    var associations = service.GetAssociations()["A67890"];
                    Assert.Single(associations);
                    Assert.Equal(TestLocations.CLPHMJN, associations.Single().Value.AtLocation);
                }
            }
        }

        public static ScheduleLocation[] CreateStopsSettingClaphamActivities(string activities)
        {
            var stops = TestSchedules.CreateFourStopSchedule(TestSchedules.Ten);
            stops[2].Activities = new Activities(activities);
            return stops;
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
        public void AddSameAssociationWithDifferentLocationsResolveToCorrectForAssociated(string activities, AssociationCategory joinSplit, bool expectedHasAssociation)
        {
            Test(TestLocations.CLPHMJN, TestLocations.Woking);
            Test(TestLocations.Woking, TestLocations.CLPHMJN);

            void Test(Location location1, Location location2)
            {
                var stops = CreateAssociateStopsSettingClaphamActivities(joinSplit, activities);
                var service = TestSchedules.CreateScheduleWithService("A12345", stops: stops).Service;
                var association1 = TestAssociations.CreateAssociation(mainUid: "A67890",associatedUid:"A12345",  
                    category: joinSplit, location: location1);
                var association2 = TestAssociations.CreateAssociation(mainUid: "A67890",associatedUid:"A12345",
                    category: joinSplit, location: location2);

                service.AddAssociation(association1, false);
                service.AddAssociation(association2, false);

                Assert.Equal(expectedHasAssociation, service.HasAssociations());
                if (expectedHasAssociation)
                {
                    var associations = service.GetAssociations()["A67890"];
                    Assert.Single(associations);
                    Assert.Equal(TestLocations.CLPHMJN, associations.Single().Value.AtLocation);
                }
            }
        }
        
        public static ScheduleLocation[] CreateAssociateStopsSettingClaphamActivities(AssociationCategory joinSplit,string activities)
        {
            ScheduleLocation[] stops;
            
            if (joinSplit.IsJoin())
            {
                stops = TestSchedules.CreateWokingClaphamSchedule(TestSchedules.NineForty);
                stops[2].Activities = new Activities(activities);              
            }
            else
            {
                stops = TestSchedules.CreateClaphamWokingSchedule(TestSchedules.TenTwentyFive);
                stops[0].Activities = new Activities(activities);                   
            }
            
            return stops;
        }
    }
}