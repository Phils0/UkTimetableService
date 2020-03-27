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
            association.Main.Service.TryFindScheduleOn(Aug10, out var resolvedService);
            var resolved = new ResolvedAssociation(
                association,
                DateTime.Today, 
                false,
                null);

            var stop = resolved.GetStop(resolvedService);
            Assert.NotNull(stop);
            Assert.Equal(TestLocations.CLPHMJN, stop.Stop.Stop.Location);
            Assert.Equal("X12345", stop.Service.TimetableUid);        
        }
        
        [Fact]
        public void GetStopOnAssociated()
        {
            var association = TestAssociations.CreateAssociationWithServices();
            association.Associated.Service.TryFindScheduleOn(Aug10, out var resolvedService);
            var resolved = new ResolvedAssociation(
                association,
                DateTime.Today, 
                false,
                null);
            ;

            var stop = resolved.GetStop(resolvedService);
            Assert.NotNull(stop);
            Assert.Equal(TestLocations.CLPHMJN, stop.Stop.Stop.Location);
            Assert.Equal("A98765", stop.Service.TimetableUid);        
        }

        [Theory]
        [InlineData(AssociationCategory.Join, true)]
        [InlineData(AssociationCategory.Split, false)]
        [InlineData(AssociationCategory.None, false)]
        [InlineData(AssociationCategory.NextPrevious, false)]
        public void IsJoin(AssociationCategory category, bool expected)
        {
            var association = TestAssociations.CreateAssociationWithServices(category: category);
            var resolved = new ResolvedAssociation(
                association,
                DateTime.Today, 
                false,
                null);
            ;
            
            Assert.Equal(expected, resolved.IsJoin);
        }
        
        [Theory]
        [InlineData(AssociationCategory.Join, false)]
        [InlineData(AssociationCategory.Split, true)]
        [InlineData(AssociationCategory.None, false)]
        [InlineData(AssociationCategory.NextPrevious, false)]
        public void IsSplit(AssociationCategory category, bool expected)
        {
            var association = TestAssociations.CreateAssociationWithServices(category: category);
            var resolved = new ResolvedAssociation(
                association,
                DateTime.Today, 
                false,
                null);
            ;
            
            Assert.Equal(expected, resolved.IsSplit);
        }
    }
}