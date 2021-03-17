using System;
using System.Collections.Generic;
using System.Net;
using ReflectionMagic;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class ResolvedServiceWithAssociationsTest
    {
        public static TheoryData<ResolvedAssociation[], bool> HasAssociationsTestData =>
            new TheoryData<ResolvedAssociation[], bool>()
            {
                { TestSchedules.CreateServiceWithAssociation().Associations, true }, 
                { TestSchedules.NoAssociations, false }, 
                { null, false }, 
            };
        
        [Theory]
        [MemberData(nameof(HasAssociationsTestData))]
        public void HasAssociations(ResolvedAssociation[] associations, bool expected)
        {
            var resolved = new ResolvedServiceWithAssociations(TestSchedules.CreateService(), associations);
            Assert.Equal(expected, resolved.HasAssociations());
        }

        [Fact]
        public void SetsAssociationStopWhenConstructed()
        {
            var main = TestSchedules.CreateService();
            var association = TestSchedules.CreateAssociation(main, "X98765");
            Assert.Null(association.Stop);
            
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association});
            Assert.NotNull(association.Stop);
        }
        
        [Fact]
        public void SetsAssociationWhenMultipleAssociations()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765");
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            Assert.Null(association1.Stop);
            Assert.Null(association2.Stop);
            
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            Assert.NotNull(association1.Stop);
            Assert.NotNull(association2.Stop);
        }
    }
}