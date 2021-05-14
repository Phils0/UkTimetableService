using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test
{
    public class SchedulePropertiesTest
    {
        [Theory]
        [InlineData("VT123400", "VT1234", true)]
        [InlineData("VT123400", "VT123400", true)]
        [InlineData("VT123400", "VT123401", true)]
        [InlineData("VT123400", "VT999900", false)]
        [InlineData("VT123400", "", false)]
        [InlineData("VT123400", null, false)]
        [InlineData("", "VT999900", false)]
        [InlineData("", "", false)]
        [InlineData("", null, false)]
        [InlineData(null, "VT999900", false)]
        [InlineData(null, "", false)]
        [InlineData(null, null, false)]
        public void HasRetailServiceIdChecksUsingTheShortRetailServiceId(string retailsServiceId, string testId, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            schedule.Properties.RetailServiceId = retailsServiceId;
            Assert.Equal(expected, schedule.Properties.HasRetailServiceId(testId));
        }
        
        [Theory]
        [InlineData("VT123400", "VT1234")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ShortRetailServiceId(string retailServiceId, string expected)
        {
            var properties = TestSchedules.CreateSchedule().Properties;
            properties.RetailServiceId = retailServiceId;
            Assert.Equal(expected, properties.NrsRetailServiceId);
        }
        
        [Theory]
        [InlineData("VT", true)]
        [InlineData("GW", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void OperatedByToc(string toc, bool expected)
        {
            var schedule = TestSchedules.CreateSchedule();
            Assert.Equal(expected, schedule.Properties.IsOperatedBy(toc));
        }
    }
}