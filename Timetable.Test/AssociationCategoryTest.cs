using Xunit;

namespace Timetable.Test
{
    public class AssociationCategoryTest
    {
        [Theory]
        [InlineData(AssociationCategory.Join, true)]
        [InlineData(AssociationCategory.Split, false)]
        [InlineData(AssociationCategory.None, false)]
        [InlineData(AssociationCategory.NextPrevious, false)]
        public void IsJoin(AssociationCategory category, bool expected)
        {
            Assert.Equal(expected, category.IsJoin());
        }
        
        [Theory]
        [InlineData(AssociationCategory.Join, false)]
        [InlineData(AssociationCategory.Split, true)]
        [InlineData(AssociationCategory.None, false)]
        [InlineData(AssociationCategory.NextPrevious, false)]
        public void IsSplit(AssociationCategory category, bool expected)
        {
            Assert.Equal(expected, category.IsSplit());
        }
    }
}