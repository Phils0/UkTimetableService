using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Serilog;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test
{
    public class ConfigurationFinderTest
    {
        public static TheoryData<Type> InternalConfigurations =>
            new TheoryData<Type>()
            {
                {typeof(Singletons)},
                {typeof(SetData)},
                {typeof(Swagger)},
                {typeof(HealthCheck)},
                {typeof(ExceptionHandler)},
            };
        
        [Theory]
        [MemberData(nameof(InternalConfigurations))]
        public void HasInternalServiceConfiguration(Type configType)
        {
            var serviceConfigurations = Find(false);
            serviceConfigurations.Should().ContainSingle(c => configType.Equals(c.GetType()));
        }

        private ServiceConfigurations Find(bool enablePrometheus)
        {
            var config = ConfigurationHelper.GetConfiguration(enablePrometheus: enablePrometheus);
            return ConfigurationFinder.Find(config, Substitute.For<ILogger>());
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PrometheusAdded(bool enabled)
        {
            var prometheus = Find(enabled).OfType<Timetable.Web.ServiceConfiguration.Prometheus>();
            prometheus.Any().Should().Be(enabled);
        }
    }
}