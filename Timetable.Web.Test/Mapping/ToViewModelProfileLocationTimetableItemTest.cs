using System;
using AutoMapper;
using Timetable.Test.Data;
using Timetable.Web.Test.Cif;
using Xunit;
using TestSchedules = Timetable.Test.Data.TestSchedules;

namespace Timetable.Web.Test.Mapping
{
    public class ToViewModelProfileLocationTimetableItemTest
    {
        private static readonly DateTime TestDate = TestTime.August1;
        
        private static readonly MapperConfiguration ToViewProfileConfiguration = 
            ToViewModelProfileLocationTest.ToViewProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            ToViewProfileConfiguration.AssertConfigurationIsValid();
        }
    }
}