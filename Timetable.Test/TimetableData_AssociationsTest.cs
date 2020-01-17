using System;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class TimetableData_AssociationsTest
    {
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);

        private static TimetableData CreateTimetable()
        {
            return new TimetableData(Substitute.For<ILogger>());
        }
        
        [Fact]
        public void AddAssociationToServices()
        {
            var timetable =  CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, "X12345");
            TestSchedules.CreateScheduleInTimetable(timetable, "A98765");

            var associations = new [] {
                TestAssociations.CreateAssociation("X12345", "A98765")
            };

            var count = timetable.AddAssociations(associations);

            Assert.Equal(1, count);
            var service = timetable.GetService("X12345");
            Assert.Single(service.GetAssociations());
        }
        
        [Fact]
        public void AddMultipleVersionsOfAnAssociationToServices()
        {
            var timetable =  CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, "X12345");
            TestSchedules.CreateScheduleInTimetable(timetable, "A98765");

            var associations = new [] {
                TestAssociations.CreateAssociation("X12345", "A98765", StpIndicator.Permanent),
                TestAssociations.CreateAssociation("X12345", "A98765", StpIndicator.Override),
                TestAssociations.CreateAssociation("X12345", "A98765", 
                    calendar: TestSchedules.CreateEverydayCalendar(new DateTime(2019, 9, 1), new DateTime(2019, 9 ,30)))
            };

            var count = timetable.AddAssociations(associations);

            Assert.Equal(3, count);
            var service = timetable.GetService("X12345");
            Assert.Single(service.GetAssociations());
        }
        
        [Fact]
        public void AddMultipleAssociationToServices()
        {
            var timetable =  CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, "X12345");
            TestSchedules.CreateScheduleInTimetable(timetable, "A98765");
            TestSchedules.CreateScheduleInTimetable(timetable, "A12345");

            var associations = new [] {
                TestAssociations.CreateAssociation("X12345", "A98765"),
                TestAssociations.CreateAssociation("X12345", "A12345")
            };

            var count = timetable.AddAssociations(associations);

            Assert.Equal(2, count);
            var service = timetable.GetService("X12345");
            var serviceAssociations = service.GetAssociations();
            Assert.Equal(2, serviceAssociations.Count);
        }
        
        [Fact]
        public void OnlyAddAssociationIfBothMainAndAssociatedServiceExist()
        {
            var timetable =  CreateTimetable();           
            TestSchedules.CreateScheduleInTimetable(timetable, "X12345");

            var associations = new [] {
                TestAssociations.CreateAssociation("X12345", "A98765"),
                TestAssociations.CreateAssociation("A98765", "X12345")                  
            };

            var count = timetable.AddAssociations(associations);

            Assert.Equal(0, count);
            var service = timetable.GetService("X12345");
            Assert.Null(service.GetAssociations());
        }


        [Fact]
        public void ResolvedServiceAlsoIncludesAssociatedServices()
        {
            var timetable = CreateTimetable();
            var main = TestSchedules.CreateScheduleInTimetable(timetable, "X12345", retailServiceId: "VT123401").Service;
            var associated = TestSchedules.CreateScheduleInTimetable(timetable, "A98765", retailServiceId: "VT123402").Service;

            var associations = new [] {
                TestAssociations.CreateAssociationWithServices(main, associated)
            };
            
            var found = timetable.GetScheduleByTimetableUid("X12345", MondayAugust12);
            var withAssociations = found.service as ResolvedServiceWithAssociations;
            Assert.NotEmpty(withAssociations.Associations);
        }
    }
}