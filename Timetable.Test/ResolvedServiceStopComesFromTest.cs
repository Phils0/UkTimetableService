using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopComesFromTest
    {
        public static IEnumerable<object[]> FromStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, true};   
                yield return new object[] {TestStations.ClaphamJunction, false};    // Destination is our stop.  Effectively assumes we do not have the same location twice in the stops list 
                yield return new object[] {TestStations.Vauxhall, false};
                yield return new object[] {TestStations.Waterloo, false};
                yield return new object[] {TestStations.Woking, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(FromStations))]
        public void ComesFrom(Station station, bool isFrom)
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            if (isFrom)
            {
                Assert.True(stop.ComesFrom(station));
                Assert.NotNull(stop.FoundFromStop);
            }
            else
            {
                Assert.False(stop.ComesFrom(station));
                Assert.Null(stop.FoundFromStop);
            }
        }
        
        [Theory]
        [InlineData(Activities.StopNotAdvertised, false)]
        [InlineData(Activities.PassengerStop, true)]
        [InlineData(Activities.RequestStop, true)]
        [InlineData(Activities.PickUpOnlyStop, true)]
        [InlineData(Activities.SetDownOnlyStop, false)]
        [InlineData(Activities.TrainBegins, true)]
        [InlineData(Activities.TrainFinishes, false)]
        public void ComesFromIsFalseIfNotPublicDeparture(string activity, bool expected)
        {
            var service =  TestSchedules.CreateService();
            var waterloo = service.Details.Locations[3];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            clapham.Activities = new Activities(activity);
            
            var stop = new ResolvedServiceStop(service, waterloo);
            
            Assert.Equal(expected, stop.ComesFrom(clapham.Station));
            Assert.Equal(expected, stop.FoundFromStop != null);
        }
        
        [Fact]
        public void ComesFromWithNoAssociation()
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            Assert.True(stop.ComesFrom(TestStations.Surbiton));
            Assert.False(stop.Association.IsIncluded);
            Assert.Equal(IncludedAssociation.NoAssociation, stop.Association);
        }
        
        public static IEnumerable<object[]> SplitComesFromStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, true, true};   
                yield return new object[] {TestStations.ClaphamJunction, true, false};
                yield return new object[] {TestStations.Waterloo, false, false};
            }
        }
        
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Theory]
        [MemberData(nameof(SplitComesFromStations))]
        public void ComesFromWorksWithSplits(Station station, bool isFrom, bool involvesAssociation)
        {
            var association = CreateSplitServices();
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.TenFiftyFive, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Associated.Service.TryFindScheduledStop(woking, out var stop);

            if (isFrom)
            {
                Assert.True(stop.ComesFrom(station));
                Assert.NotNull(stop.FoundFromStop);
            }
            else
            {
                Assert.False(stop.ComesFrom(station));
                Assert.Null(stop.FoundFromStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }

        private Association CreateSplitServices(ScheduleLocation[] mainStops = null)
        {
            return CreateAssociation(AssociationCategory.Split, mainStops);
        }

        private Association CreateAssociation(AssociationCategory category, ScheduleLocation[] mainStops = null)
        {
            mainStops = mainStops ?? TestSchedules.CreateThreeStopSchedule(TestSchedules.Ten);
            var main = TestSchedules.CreateScheduleWithService("X12345", retailServiceId: "VT123401", stops: mainStops).Service;
            var associated = TestSchedules.CreateScheduleWithService("A98765", retailServiceId: "VT123402",
                stops: TestSchedules.CreateClaphamWokingSchedule(TestSchedules.TenTwentyFive)).Service;
            var association = TestAssociations.CreateAssociationWithServices(main, associated, category: category);
            return association;
        }
        
        [Fact]
        public void MainComesFromAtSplitIsMainStop()
        {
            var association = CreateSplitServices();
            var waterloo = new StopSpecification(TestStations.Waterloo, TestSchedules.TenThirty, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Main.Service.TryFindScheduledStop(waterloo, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.ClaphamJunction));
            
            Assert.Equal(TestSchedules.TenFifteen, (stop.FoundFromStop.Stop as ScheduleStop).Arrival);
        }
        
        [Fact]
        public void AssociatedComesFromAtSplitIsAssociatedStop()
        {
            var association = CreateSplitServices();
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.TenFiftyFive, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Associated.Service.TryFindScheduledStop(woking, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.ClaphamJunction));
            
            Assert.Equal(TestSchedules.TenTwentyFive, (stop.FoundFromStop.Stop as ScheduleStop).Departure);
        }
        
        [Fact]
        public void ComesFromWorksWithSplitsWhereMainTerminatesAtSplitPoint()
        {
            //  Ends at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                mainStops[0],
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, TestSchedules.TenFifteen)
            };
            var association = CreateSplitServices(mainStops);
            
            var at = new StopSpecification(TestStations.Woking, TestSchedules.TenFiftyFive, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.Surbiton));
            Assert.NotNull(stop.FoundFromStop);
            Assert.True(stop.Association.IsIncluded);
        }
        
        [Fact]
        public void ComesFromWorksWithSplitsWhereMainStartsAtSplitPoint()
        {
            //  Start at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                TestScheduleLocations.CreateOrigin(TestStations.ClaphamJunction, TestSchedules.TenTen),
                mainStops[3]
            };
            var association = CreateSplitServices(mainStops);
            
            var at = new StopSpecification(TestStations.Woking, TestSchedules.TenFiftyFive, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.ClaphamJunction));
            Assert.NotNull(stop.FoundFromStop);
            Assert.False(stop.Association.IsIncluded);
        }
        
        public static IEnumerable<object[]> MainJoinFromStations
        {
            get
            {
                yield return new object[] {TestStations.Waterloo, TestSchedules.TenThirty, TestStations.ClaphamJunction, true, false};
                yield return new object[] {TestStations.Waterloo, TestSchedules.TenThirty, TestStations.Surbiton, true, false};
                yield return new object[] {TestStations.Waterloo, TestSchedules.TenThirty, TestStations.Weybridge, true, true};
                yield return new object[] {TestStations.Waterloo, TestSchedules.TenThirty, TestStations.Woking, true, true};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenFifteen, TestStations.Surbiton, true, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenFifteen, TestStations.Wimbledon, true, false};
                yield return new object[] {TestStations.Wimbledon, TestSchedules.TenTen, TestStations.Surbiton, true, false};
                yield return new object[] {TestStations.Wimbledon, TestSchedules.TenTen, TestStations.Weybridge, false, false};
                yield return new object[] {TestStations.Wimbledon, TestSchedules.TenTen, TestStations.Woking, false, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(MainJoinFromStations))]
        public void ComesFromWorksWithJoinsToMain(Station atStation, Time departs, Station @from, bool isFrom, bool involvesAssociation)
        {
            var association = CreateJoinServices();
            var at = new StopSpecification(atStation, departs, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Main.Service.TryFindScheduledStop(at, out var stop);

            if (isFrom)
            {
                Assert.True(stop.ComesFrom(@from));
                Assert.NotNull(stop.FoundFromStop);
            }
            else
            {
                Assert.False(stop.ComesFrom(@from));
                Assert.Null(stop.FoundFromStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }
        
        [Fact] 
        public void ComesFromWorksWithJoinsWhereMainStartsAtJoinPoint()
        {
            //  Starts at Clapham
            var mainStops = CreateMainStops();
            
            mainStops = new [] {
                TestScheduleLocations.CreateOrigin(TestStations.ClaphamJunction, TestSchedules.TenFifteen),
                mainStops[3],
                mainStops[4]
            };
            var association = CreateJoinServices(mainStops);
            
            var at = new StopSpecification(TestStations.Waterloo, TestSchedules.TenThirty, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Main.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.Woking));
            Assert.NotNull(stop.FoundFromStop);
            Assert.True(stop.Association.IsIncluded);
        }
        
        [Fact] 
        public void ComesFromWorksWithJoinsWhereMainEndsAtJoinPoint()
        {
            //  Ends at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                mainStops[0],
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, TestSchedules.TenFifteen)
            };
            var association = CreateJoinServices(mainStops);
            
            var at = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.TenFifteen, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Main.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.ComesFrom(TestStations.Woking));
            Assert.NotNull(stop.FoundFromStop);
            Assert.True(stop.Association.IsIncluded);
        }
        
        private Association CreateJoinServices(ScheduleLocation[] mainStops = null)
        {
            return CreateJoinServices(TestSchedules.NineForty, mainStops);
        }
        
        private Association CreateJoinServices(Time associationDeparts, ScheduleLocation[] mainStops = null)
        {
            mainStops ??= CreateMainStops();
            var main = TestSchedules.CreateScheduleWithService("X12345", retailServiceId: "VT123401", stops: mainStops).Service;
            var associated = TestSchedules.CreateScheduleWithService("A98765", retailServiceId: "VT123402",
                stops: TestSchedules.CreateWokingClaphamSchedule(associationDeparts)).Service;
            var association = TestAssociations.CreateAssociationWithServices(main, associated);
            return association;
        }

        private ScheduleLocation[] CreateMainStops()
        {
            return CreateMainStops(TestSchedules.Ten);
        }
        private ScheduleLocation[] CreateMainStops(Time departs)
        {
            var mainStops = TestSchedules.CreateFourStopSchedule(departs);
            mainStops[3] = TestScheduleLocations.CreateStop(TestStations.Vauxhall, TestSchedules.TenTwenty);    // Make Vauxhall a stop
            return mainStops;
        }
        
        public static IEnumerable<object[]> AssociatedSplitToStations
        {
            get
            {
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTen, TestStations.Waterloo, false, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTen, TestStations.Weybridge, true, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTen, TestStations.Woking, true, false};
                yield return new object[] {TestStations.Weybridge, TestSchedules.NineFiftyFive, TestStations.Waterloo, false, false};
                yield return new object[] {TestStations.Weybridge, TestSchedules.NineFiftyFive, TestStations.Woking, true, false};            
            }
        }
        
        [Theory]
        [MemberData(nameof(AssociatedSplitToStations))]
        public void ComesFromWorksWithJoinFromAssociated(Station atStation, Time departs, Station @from, bool isFrom, bool involvesAssociation)
        {
            var association = CreateJoinServices();
            var at = new StopSpecification(atStation, departs, MondayAugust12, TimesToUse.Arrivals);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);

            if (isFrom)
            {
                Assert.True(stop.ComesFrom(@from));
                Assert.NotNull(stop.FoundFromStop);
            }
            else
            {
                Assert.False(stop.ComesFrom(@from));
                Assert.Null(stop.FoundFromStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }
        
        [Fact]
        public void DoNotUseCancelledAssociations()
        {
            var association = CreateJoinServices();
            var cancelledAssociation  = TestAssociations.CreateAssociationWithServices(
                    association.Main.Service as CifService, 
                    association.Associated.Service as CifService, 
                    StpIndicator.Cancelled);
            
            var waterloo = new StopSpecification(TestStations.Waterloo, TestSchedules.TenThirty, MondayAugust12, TimesToUse.Arrivals);
            var found = cancelledAssociation.Main.Service.TryFindScheduledStop(waterloo, out var stop);
            
            Assert.False(stop.ComesFrom(TestStations.Woking));
            Assert.Null(stop.FoundFromStop);
        }
    }
}