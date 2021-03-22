using System;
using System.Collections.Generic;
using System.Dynamic;
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
        public void ResolvedServiceDoesNotHaveAssociations()
        {
            var resolved = TestSchedules.CreateService();
            Assert.False(resolved.HasAssociations());
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
        
        [Fact]
        public void RemoveCancelledAssociation()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765", true);
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            withAssociation.RemoveCancelledAssociations();
            
            Assert.Single(withAssociation.Associations);
            Assert.Equal(association2, withAssociation.Associations[0]);
        }
        
        [Fact]
        public void RemoveAssociationWithCancelledService()
        {
            var main = TestSchedules.CreateService();
            var associated = TestSchedules.CreateService("X98765", stops: TestSchedules.CreateWokingClaphamSchedule(TestSchedules.NineForty), isCancelled: true);
            var association1 = TestSchedules.CreateAssociation(main, associated);
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            withAssociation.RemoveCancelledAssociations();
            
            Assert.Single(withAssociation.Associations);
            Assert.Equal(association2, withAssociation.Associations[0]);
        }
        
        [Fact]
        public void AllAssociationsAreCancelled()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765", true);
            var association2 = TestSchedules.CreateAssociation(main, "X56789", true);
            
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            withAssociation.RemoveCancelledAssociations();
            
            Assert.False(withAssociation.HasAssociations());
            Assert.Empty(withAssociation.Associations);
        }
        
        [Fact]
        public void RemoveBrokenAssociation()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765");
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});

            association2.AsDynamic().Stop = new ResolvedAssociationStop(association2.Stop.Stop, null);        
            
            withAssociation.RemoveBrokenAssociations();
            
            Assert.Single(withAssociation.Associations);
            Assert.Equal(association1, withAssociation.Associations[0]);
        }
        
        [Fact]
        public void RemoveAssociationWithNoStop()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765");
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            
            association2.AsDynamic().Stop = null;
            
            withAssociation.RemoveBrokenAssociations();
            
            Assert.Single(withAssociation.Associations);
            Assert.Equal(association1, withAssociation.Associations[0]);
        }
        
        [Fact]
        public void RemoveBrokenWhenCancelledAssociation()
        {
            var main = TestSchedules.CreateService();
            var association1 = TestSchedules.CreateAssociation(main, "X98765", true);
            var association2 = TestSchedules.CreateAssociation(main, "X56789");
            var withAssociation = new ResolvedServiceWithAssociations(main, new [] {association1, association2});
            
            association2.AsDynamic().Stop = new ResolvedAssociationStop(association2.Stop.Stop, null);        
            
            withAssociation.RemoveBrokenAssociations();
            
            Assert.Single(withAssociation.Associations);
            Assert.Equal(association1, withAssociation.Associations[0]);
        }
    }
}