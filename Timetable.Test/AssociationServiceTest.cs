using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class AssociationServiceTest
    {
        [Theory]
        [InlineData("X12345", true)]
        [InlineData("A98765", false)]
        [InlineData("Q11111", false)]
        public void AddToServiceThrowsExceptionWhenNotMatchingId(string uid, bool success)
        {
            var schedule = TestSchedules.CreateScheduleWithService(uid);
            var associationService = TestAssociations.CreateAssociation().Main;
            
            Assert.Equal(success, associationService.TrySetService(schedule.Service));
        }
    }
}