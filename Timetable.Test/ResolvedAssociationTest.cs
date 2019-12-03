using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedAssociationTest
    {
        [Theory]
        [InlineData("X12345", true)]
        [InlineData("A98765", false)]
        public void IsMain(string timetableId, bool expected)
        {
            var association = new ResolvedAssociation(
                TestAssociations.CreateAssociation(),
                DateTime.Today, 
                false,
                null);
                ;
            
            Assert.Equal(expected, association.IsMain(timetableId));
        }
        
        private static DateTime Aug10 = new DateTime(2019, 8, 12);
        
        [Fact]
        public void GetStopOnMain()
        {
            var association = TestAssociations.CreateAssociationWithServices();
            var resolvedService = association.Main.Service.GetScheduleOn(Aug10);
            var resolved = new ResolvedAssociation(
                TestAssociations.CreateAssociation(),
                DateTime.Today, 
                false,
                null);
            ;

            var stop = resolved.GetStop(resolvedService);
            Assert.NotNull(stop);
            Assert.Equal(TestLocations.CLPHMJN, stop.Location);
            Assert.Equal("X12345", stop.Schedule.TimetableUid);        
        }
        
        [Fact]
        public void GetStopOnAssociated()
        {
            var association = TestAssociations.CreateAssociationWithServices();
            var resolvedService = association.Associated.Service.GetScheduleOn(Aug10);
            var resolved = new ResolvedAssociation(
                TestAssociations.CreateAssociation(),
                DateTime.Today, 
                false,
                null);
            ;

            var stop = resolved.GetStop(resolvedService);
            Assert.NotNull(stop);
            Assert.Equal(stop.Location, TestLocations.CLPHMJN);
            Assert.Equal("A98765", stop.Schedule.TimetableUid);        
        }
    }
}