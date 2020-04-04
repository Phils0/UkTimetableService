using System;
using System.Collections.Generic;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceStopGoesToTest
    {
        public static IEnumerable<object[]> ToStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, false};   
                yield return new object[] {TestStations.ClaphamJunction, false};    // Destination is our stop.  Effectively assumes we do not have the same location twice in the stops list 
                yield return new object[] {TestStations.Vauxhall, false};
                yield return new object[] {TestStations.Waterloo, true};
                yield return new object[] {TestStations.Woking, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(ToStations))]
        public void GoesTo(Station station, bool isTo)
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            if (isTo)
            {
                Assert.True(stop.GoesTo(station));
                Assert.NotNull(stop.FoundToStop);
            }
            else
            {
                Assert.False(stop.GoesTo(station));
                Assert.Null(stop.FoundToStop);
            }
        }
        
        [Fact]
        public void GoesToWithNoAssociation()
        {
            var service =  TestSchedules.CreateService();
            var clapham = service.Details.Locations[1];
            var stop = new ResolvedServiceStop(service, clapham);

            Assert.True(stop.GoesTo(TestStations.Waterloo));
            Assert.False(stop.Association.IsIncluded);
            Assert.Equal(IncludedAssociation.NoAssociation, stop.Association);
        }
        
        [Theory]
        [InlineData(Activities.StopNotAdvertised, false)]
        [InlineData(Activities.PassengerStop, true)]
        [InlineData(Activities.RequestStop, true)]
        [InlineData(Activities.PickUpOnlyStop, false)]
        [InlineData(Activities.SetDownOnlyStop, true)]
        [InlineData(Activities.TrainBegins, false)]
        [InlineData(Activities.TrainFinishes, true)]
        public void GoesToOnlyIfHasPublicArrival(string activity, bool expected)
        {
            var service =  TestSchedules.CreateService();
            var surbiton = service.Details.Locations[0];
            var clapham = service.Details.Locations[1] as ScheduleStop;
            clapham.Activities = new Activities(activity);
          
            var stop = new ResolvedServiceStop(service, surbiton);
            
            Assert.Equal(expected, stop.GoesTo(clapham.Station));
            Assert.Equal(expected, stop.FoundToStop != null);
        }
        
        public static IEnumerable<object[]> JoinToStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, false, false};   
                yield return new object[] {TestStations.ClaphamJunction, true, false};
                yield return new object[] {TestStations.Vauxhall, false, false};    // Passing
                yield return new object[] {TestStations.Waterloo, true, true};
            }
        }
        
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        [Theory]
        [MemberData(nameof(JoinToStations))]
        public void GoesToWorksWithJoins(Station station, bool isTo, bool involvesAssociation)
        {
            var association = CreateJoinServices();
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, MondayAugust12, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(woking, out var stop);

            if (isTo)
            {
                Assert.True(stop.GoesTo(station));
                Assert.NotNull(stop.FoundToStop);
            }
            else
            {
                Assert.False(stop.GoesTo(station));
                Assert.Null(stop.FoundToStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }

        private Association CreateJoinServices(ScheduleLocation[] mainStops = null)
        {
            mainStops = mainStops ?? TestSchedules.CreateThreeStopSchedule(TestSchedules.Ten);
            var main = TestSchedules.CreateScheduleWithService("X12345", retailServiceId: "VT123401", stops: mainStops).Service;
            var associated = TestSchedules.CreateScheduleWithService("A98765", retailServiceId: "VT123402",
                stops: TestSchedules.CreateWokingClaphamSchedule(TestSchedules.NineForty)).Service;
            var association = TestAssociations.CreateAssociationWithServices(main, associated);
            return association;
        }

        [Fact]
        public void MainGoesToAtJoinIsMainStop()
        {
            var association = CreateJoinServices();
            var surbiton = new StopSpecification(TestStations.Surbiton, TestSchedules.Ten, MondayAugust12, TimesToUse.Departures);
            var found = association.Main.Service.TryFindScheduledStop(surbiton, out var stop);
            
            Assert.True(stop.GoesTo(TestStations.ClaphamJunction));
            
            Assert.Equal(TestSchedules.TenFifteen, (stop.FoundToStop.Stop as ScheduleStop).Arrival);
        }
        
        [Fact]
        public void AssociatedGoesToAtJoinIsAssociatedStop()
        {
            var association = CreateJoinServices();
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, MondayAugust12, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(woking, out var stop);
            
            Assert.True(stop.GoesTo(TestStations.ClaphamJunction));
            
            Assert.Equal(TestSchedules.TenTen, (stop.FoundToStop.Stop as ScheduleStop).Arrival);
        }
        
        public static IEnumerable<object[]> MainSplitToStations
        {
            get
            {
                yield return new object[] {TestStations.Surbiton, TestSchedules.Ten, TestStations.ClaphamJunction, true, false};
                yield return new object[] {TestStations.Surbiton, TestSchedules.Ten, TestStations.Waterloo, true, false};
                yield return new object[] {TestStations.Surbiton, TestSchedules.Ten, TestStations.Weybridge, true, true};
                yield return new object[] {TestStations.Surbiton, TestSchedules.Ten, TestStations.Woking, true, true};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenSixteen, TestStations.Waterloo, true, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenSixteen, TestStations.Weybridge, true, true};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenSixteen, TestStations.Woking, true, true};
                yield return new object[] {TestStations.Vauxhall, TestSchedules.TenTwentyOne, TestStations.Waterloo, true, false};
                yield return new object[] {TestStations.Vauxhall, TestSchedules.TenTwentyOne, TestStations.Weybridge, false, false};
                yield return new object[] {TestStations.Vauxhall, TestSchedules.TenTwentyOne, TestStations.Woking, false, false};
            }
        }
        
        [Theory]
        [MemberData(nameof(MainSplitToStations))]
        public void GoesToWorksWithSplitsFromMain(Station from, Time departs, Station to, bool isTo, bool involvesAssociation)
        {
            var association = CreateSplitServices();
            var at = new StopSpecification(from, departs, MondayAugust12, TimesToUse.Departures);
            var found = association.Main.Service.TryFindScheduledStop(at, out var stop);

            if (isTo)
            {
                Assert.True(stop.GoesTo(to));
                Assert.NotNull(stop.FoundToStop);
            }
            else
            {
                Assert.False(stop.GoesTo(to));
                Assert.Null(stop.FoundToStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }
        
        [Fact]
        public void GoesToWorksWithJoinsWhereMainStartsAtJoinPoint()
        {
            //  Starts at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                    TestScheduleLocations.CreateOrigin(TestStations.ClaphamJunction, TestSchedules.TenFifteen),
                    mainStops[2],
                    mainStops[3]
                };
            var association = CreateJoinServices(mainStops);
            
            var at = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, MondayAugust12, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.GoesTo(TestStations.Waterloo));
            Assert.NotNull(stop.FoundToStop);
            Assert.True(stop.Association.IsIncluded);
        }
        
        [Fact]
        public void GoesToWorksWithJoinsWhereMainStopsAtJoinPoint()
        {
            //  Ends at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                mainStops[0],
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, TestSchedules.TenFifteen)
            };
            var association = CreateJoinServices(mainStops);
            
            var at = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, MondayAugust12, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);
            
            // Neither train goes beyond Clapham so actually whole journey on join
            Assert.True(stop.GoesTo(TestStations.ClaphamJunction));    
            Assert.NotNull(stop.FoundToStop);
            Assert.False(stop.Association.IsIncluded);
        }

        [Fact]
        public void GoesToWorksWithSplitsWhereMainStartsAtSplitPoint()
        {
            //  Start at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                TestScheduleLocations.CreateOrigin(TestStations.ClaphamJunction, TestSchedules.TenTen),
                mainStops[2],
                mainStops[3]
            };
            var association = CreateSplitServices(mainStops);
            
            var at = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.TenTen, MondayAugust12, TimesToUse.Departures);
            var found = association.Main.Service.TryFindScheduledStop(at, out var mainStop);
            
            // Both trains start at Clapham so actually whole journey on split
            // Using main stop fails
            Assert.True(mainStop.GoesTo(TestStations.Woking));
            Assert.NotNull(mainStop.FoundToStop);
            Assert.True(mainStop.Association.IsIncluded);
            
            at = new StopSpecification(TestStations.ClaphamJunction, TestSchedules.TenTwentyFive, MondayAugust12, TimesToUse.Departures);
            found = association.Associated.Service.TryFindScheduledStop(at, out var associationStop);
            
            // Using Association works
            Assert.True(associationStop.GoesTo(TestStations.Woking));
            Assert.NotNull(associationStop.FoundToStop);
            Assert.False(associationStop.Association.IsIncluded);
        }
        
        [Fact]
        public void GoesToWorksWithSplitsWhereMainTerminatesAtSplitPoint()
        {
            //  Stop at Clapham
            var mainStops = CreateMainStops();
            mainStops = new [] {
                mainStops[0],
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, TestSchedules.TenFifteen)
            };
            var association = CreateSplitServices(mainStops);
            
            var at = new StopSpecification(TestStations.Surbiton, TestSchedules.Ten, MondayAugust12, TimesToUse.Departures);
            var found = association.Main.Service.TryFindScheduledStop(at, out var stop);
            
            Assert.True(stop.GoesTo(TestStations.Woking));
            Assert.NotNull(stop.FoundToStop);
            Assert.True(stop.Association.IsIncluded);
        }
        
        private Association CreateSplitServices(ScheduleLocation[] mainStops = null)
        {
            mainStops ??= CreateMainStops();
            var main = TestSchedules.CreateScheduleWithService("X12345", retailServiceId: "VT123401", stops: mainStops).Service;
            var associated = TestSchedules.CreateScheduleWithService("A98765", retailServiceId: "VT123402",
                stops: TestSchedules.CreateClaphamWokingSchedule(TestSchedules.TenTwentyFive)).Service;
            var association = TestAssociations.CreateAssociationWithServices(main, associated, category: AssociationCategory.Split);
            return association;
        }

        private ScheduleLocation[] CreateMainStops()
        {
            var mainStops = TestSchedules.CreateThreeStopSchedule(TestSchedules.Ten);
            mainStops[2] = TestScheduleLocations.CreateStop(TestStations.Vauxhall, TestSchedules.TenTwenty);    // Make Vauxhall a stop
            return mainStops;
        }
        
        public static IEnumerable<object[]> AssociatedSplitToStations
        {
            get
            {
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTwentyFive, TestStations.Waterloo, false, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTwentyFive, TestStations.Weybridge, true, false};
                yield return new object[] {TestStations.ClaphamJunction, TestSchedules.TenTwentyFive, TestStations.Woking, true, false};
                yield return new object[] {TestStations.Weybridge, TestSchedules.TenFortyOne, TestStations.Waterloo, false, false};
                yield return new object[] {TestStations.Weybridge, TestSchedules.TenFortyOne, TestStations.Woking, true, false};            
            }
        }
        
        [Theory]
        [MemberData(nameof(AssociatedSplitToStations))]
        public void GoesToWorksWithSplitsFromAssociated(Station from, Time departs, Station to, bool isTo, bool involvesAssociation)
        {
            var association = CreateSplitServices();
            var at = new StopSpecification(from, departs, MondayAugust12, TimesToUse.Departures);
            var found = association.Associated.Service.TryFindScheduledStop(at, out var stop);

            if (isTo)
            {
                Assert.True(stop.GoesTo(to));
                Assert.NotNull(stop.FoundToStop);
            }
            else
            {
                Assert.False(stop.GoesTo(to));
                Assert.Null(stop.FoundToStop);
            }
            Assert.Equal(involvesAssociation, stop.Association.IsIncluded);
        }
        
        [Fact]
        public void DoNotUseCancelledAssociations()
        {
            var association = CreateJoinServices();
            var cancelledAssociation  = TestAssociations.CreateAssociationWithServices(association.Main.Service, association.Associated.Service, StpIndicator.Cancelled);
            
            var woking = new StopSpecification(TestStations.Woking, TestSchedules.NineForty, MondayAugust12, TimesToUse.Departures);
            var found = cancelledAssociation.Associated.Service.TryFindScheduledStop(woking, out var stop);
            
            Assert.False(stop.GoesTo(TestStations.Waterloo));
            Assert.Null(stop.FoundToStop);
        }
    }
}