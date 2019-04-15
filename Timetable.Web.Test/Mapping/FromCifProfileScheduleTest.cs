using System;
using System.Collections.Generic;
using AutoMapper;
using CifParser;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Timetable.Web.Test.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping
{
    public class FromCifProfileScheduleTest
    {
        private static readonly MapperConfiguration FromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            FromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void ScheduleMapTimetableUid()
        {
            var output = MapSchedule();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static TocLookup CreateLookup() => new TocLookup(
            Substitute.For<ILogger>(),
            new Dictionary<string, Toc>());
        
        public static Schedule MapSchedule(CifParser.Schedule input = null)
        {
            input = input ?? TestSchedules.Test;
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Schedule, Timetable.Schedule>(input, o =>
            {
                o.Items.Add("Tocs", CreateLookup());
                o.Items.Add("Locations", TestData.Instance);
            });
        }

        [Fact]
        public void ScheduleMapStpIndicator()
        {
            var output = MapSchedule();
            Assert.Equal(Timetable.StpIndicator.Permanent, output.StpIndicator);
        }

        [Fact]
        public void ScheduleMapStatus()
        {
            var output = MapSchedule();
            Assert.Equal(ServiceStatus.PermanentPassenger, output.Status);
        }

        [Fact]
        public void ScheduleMapCategory()
        {
            var output = MapSchedule();
            Assert.Equal(ServiceCategory.ExpressPassenger, output.Category);
        }

        [Fact]
        public void ScheduleMapSeatClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.Both, output.SeatClass);
        }

        [Fact]
        public void ScheduleMapSleeperClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.None, output.SleeperClass);
        }

        [Fact]
        public void ScheduleMapReservationIndicator()
        {
            var output = MapSchedule();
            Assert.Equal(ReservationIndicator.Recommended, output.ReservationIndicator);
        }

        [Fact]
        public void ScheduleMapCalendar()
        {
            var output = MapSchedule();
            var calendar = output.On as Calendar;
            Assert.Equal(new DateTime(2019, 8, 1), calendar.RunsFrom);
            Assert.Equal(new DateTime(2019, 8, 31), calendar.RunsTo);
            Assert.Equal(DaysFlag.Weekdays, calendar.DayMask);
            Assert.Equal(BankHolidayRunning.RunsOnBankHoliday, calendar.BankHolidays);
        }

        [Fact]
        public void ScheduleMapReusesExistingCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output1 = MapSchedule();
            var output2 = MapSchedule();

            Assert.NotSame(output1, output2);
            Assert.Same(output1.On, output2.On);
        }

        [Fact]
        public void ScheduleMapRetailServiceId()
        {
            var output = MapSchedule();
            Assert.Equal("SW123400", output.RetailServiceId);
        }

        [Fact]
        public void ScheduleMapToc()
        {
            var output = MapSchedule();
            var toc = output.Toc;
            Assert.Equal("SW", toc.Code);
            Assert.Equal("", toc.Name);
        }

        [Fact]
        public void ScheduleMapReusesExistingToc()
        {
            var lookup = CreateLookup();
            var mapper = FromCifProfileConfiguration.CreateMapper();
            
            var output1 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test, o =>
                {
                    o.Items.Add("Tocs", lookup);
                    o.Items.Add("Locations", TestData.Instance);
                });
            var output2 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(TestSchedules.Test, o => 
                {
                    o.Items.Add("Tocs", lookup);
                    o.Items.Add("Locations", TestData.Instance);
                });

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Toc, output2.Toc);
        }

        [Fact]
        public void ScheduleMapNoExtraDataRecord()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    TestSchedules.CreateScheduleDetails(),
                    TestSchedules.CreateOriginLocation(),
                    TestSchedules.CreateIntermediateLocation(),
                    TestSchedules.CreateIntermediateLocation(),
                    TestSchedules.CreateTerminalLocation()
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal(Toc.Unknown, output.Toc);
            Assert.Equal("", output.RetailServiceId);
        }
        
        [Fact]
        public void ScheduleMapNoRetailServiceIdSet()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    TestSchedules.CreateScheduleDetails(),
                    TestSchedules.CreateScheduleExtraDetails(retailServieId: ""),
                    TestSchedules.CreateOriginLocation(),
                    TestSchedules.CreateIntermediateLocation(),
                    TestSchedules.CreateIntermediateLocation(),
                    TestSchedules.CreateTerminalLocation()
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal("", output.RetailServiceId);
        }
    }
}