using Xunit;

namespace Timetable.Test
{
    public class ResultWindowTest
    {
        [Theory]
        [InlineData(2, 3, 2, 3)]   // both requested - unchanged
        [InlineData(0, 5, 0, 5)]   // after-only - unchanged
        [InlineData(3, 0, 3, 0)]   // before-only - unchanged (the guard fires only when BOTH are zero)
        [InlineData(1, 1, 1, 1)]
        public void PreservesCountsWhenAtLeastOneIsRequested(int before, int after, int expectedBefore, int expectedAfter)
        {
            var window = new ResultWindow(before, after);

            Assert.Equal(expectedBefore, window.Before);
            Assert.Equal(expectedAfter, window.After);
        }

        [Fact]
        public void PromotesZeroZeroToASingleServiceAfterThePivot()
        {
            var window = new ResultWindow(0, 0);

            Assert.Equal(0, window.Before);
            Assert.Equal(1, window.After);
        }
    }
}
