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
                { new ResolvedAssociation[1], true }, 
                { new ResolvedAssociation[0], false }, 
                { null, false }, 
            };
        
        [Theory]
        [MemberData(nameof(HasAssociationsTestData))]
        public void HasAssociations(ResolvedAssociation[] associations, bool expected)
        {
            var resolved = new ResolvedServiceWithAssociations(TestSchedules.CreateService(), associations);
            Assert.Equal(expected, resolved.HasAssociations());
        }

    }
}