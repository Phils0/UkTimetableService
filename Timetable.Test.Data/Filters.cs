using NSubstitute;
using Serilog;

namespace Timetable.Test.Data
{
    public static class Filters
    {
        public static ServiceFilters Instance => new ServiceFilters(false, Substitute.For<ILogger>());
    }
}