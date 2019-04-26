using System;
using System.Collections.Generic;
using AutoMapper;
using CifParser;
using CifParser.Records;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Timetable.Web.Mapping;
using Cif = Timetable.Web.Test.Cif;
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
        public void MapTimetableUid()
        {
            var output = MapSchedule();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static TocLookup CreateLookup() => new TocLookup(
            Substitute.For<ILogger>(),
            new Dictionary<string, Toc>());
        
        private static TimetableData CreateTimetable() => new TimetableData();
        
        public static Schedule MapSchedule(CifParser.Schedule input = null)
        {
            input = input ?? Cif.TestSchedules.Test;
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Schedule, Timetable.Schedule>(input, o =>
            {
                o.Items.Add("Tocs", CreateLookup());
                o.Items.Add("Locations", TestData.Locations);
                o.Items.Add("Timetable", CreateTimetable());
            });
        }
       
        [Theory]
        [InlineData(CifParser.Records.StpIndicator.P, StpIndicator.Permanent)]
        [InlineData(CifParser.Records.StpIndicator.O, StpIndicator.Override)]
        [InlineData(CifParser.Records.StpIndicator.N, StpIndicator.New)]
        [InlineData(CifParser.Records.StpIndicator.C, StpIndicator.Cancelled)]
        public void MapStpIndicator(CifParser.Records.StpIndicator input, StpIndicator expected)
        {
            var schedule = Cif.TestSchedules.Test;
            var details = schedule.GetScheduleDetails();
            details.StpIndicator = input;

            var output = MapSchedule(schedule);

            Assert.Equal(expected, output.StpIndicator);
        }

        [Fact]
        public void MapStatus()
        {
            var output = MapSchedule();
            Assert.Equal(ServiceStatus.PermanentPassenger, output.Status);
        }

        [Fact]
        public void MapCategory()
        {
            var output = MapSchedule();
            Assert.Equal(ServiceCategory.ExpressPassenger, output.Category);
        }

        [Fact]
        public void MapSeatClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.Both, output.SeatClass);
        }

        [Fact]
        public void MapSleeperClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.None, output.SleeperClass);
        }

        [Fact]
        public void MapReservationIndicator()
        {
            var output = MapSchedule();
            Assert.Equal(ReservationIndicator.Recommended, output.ReservationIndicator);
        }

        [Fact]
        public void MapCalendar()
        {
            var output = MapSchedule();
            var calendar = output.Calendar as Calendar;
            Assert.Equal(new DateTime(2019, 8, 1), calendar.RunsFrom);
            Assert.Equal(new DateTime(2019, 8, 31), calendar.RunsTo);
            Assert.Equal(DaysFlag.Weekdays, calendar.DayMask);
            Assert.Equal(BankHolidayRunning.RunsOnBankHoliday, calendar.BankHolidays);
        }

        [Fact]
        public void MapReusesExistingCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output1 = MapSchedule();
            var output2 = MapSchedule();

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Calendar, output2.Calendar);
        }

        [Fact]
        public void MapRetailServiceId()
        {
            var output = MapSchedule();
            Assert.Equal("SW123400", output.RetailServiceId);
        }

        [Fact]
        public void MapToc()
        {
            var output = MapSchedule();
            var toc = output.Operator;
            Assert.Equal("SW", toc.Code);
            Assert.Equal("", toc.Name);
        }

        [Fact]
        public void MapReusesExistingToc()
        {
            var lookup = CreateLookup();
            var mapper = FromCifProfileConfiguration.CreateMapper();
            
            var output1 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(Cif.TestSchedules.Test, o =>
                {
                    o.Items.Add("Tocs", lookup);
                    o.Items.Add("Locations", TestData.Locations);
                    o.Items.Add("Timetable", CreateTimetable());
                });
            var output2 = mapper.Map<CifParser.Schedule, Timetable.Schedule>(Cif.TestSchedules.Test, o => 
                {
                    o.Items.Add("Tocs", lookup);
                    o.Items.Add("Locations", TestData.Locations);
                    o.Items.Add("Timetable", CreateTimetable());
                });

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Operator, output2.Operator);
        }

        [Fact]
        public void MapNoExtraDataRecord()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    Cif.TestSchedules.CreateScheduleDetails()
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal(Toc.Unknown, output.Operator);
            Assert.Equal("", output.RetailServiceId);
        }
        
        [Fact]
        public void MapNoRetailServiceIdSet()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    Cif.TestSchedules.CreateScheduleDetails(),
                    Cif.TestSchedules.CreateScheduleExtraDetails(retailServieId: "")
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal("", output.RetailServiceId);
        }
        
        [Fact]
        public void MapLocations()
        {
            var output = MapSchedule();
            Assert.NotEmpty(output.Locations);
        }
        
        [Fact]
        public void DoNotMapUnknownLocations()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    Cif.TestSchedules.CreateScheduleDetails(),
                    Cif.TestSchedules.CreateScheduleExtraDetails(),
                    Cif.TestSchedules.CreateOriginLocation(tiploc: "UNKNOWN1"),
                    Cif.TestSchedules.CreateIntermediateLocation(tiploc: "UNKNOWN"),
                    Cif.TestSchedules.CreatePassLocation(tiploc: "UNKNOWN2"),
                    Cif.TestSchedules.CreateIntermediateLocation(tiploc: "UNKNOWN", sequence: 2),
                    Cif.TestSchedules.CreateTerminalLocation(tiploc: "UNKNOWN3")
                })
            };
            
            var output = MapSchedule(schedule);
            Assert.Empty(output.Locations);
        }
        
        [Fact]
        public void AddADayToTimesWhenGoIntoNextDay()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    Cif.TestSchedules.CreateScheduleDetails(),
                    Cif.TestSchedules.CreateScheduleExtraDetails(),
                    Cif.TestSchedules.CreateOriginLocation(departure: new TimeSpan(23, 30, 0)),
                    Cif.TestSchedules.CreateIntermediateLocation(departure: new TimeSpan(23, 45, 0)),
                    Cif.TestSchedules.CreatePassLocation(pass: new TimeSpan(0, 15, 0)),
                    Cif.TestSchedules.CreateIntermediateLocation(departure: new TimeSpan(0, 30, 0), sequence: 2),
                    Cif.TestSchedules.CreateTerminalLocation(arrival: new TimeSpan(0, 45, 0))
                })
            };
            
            var output = MapSchedule(schedule);

            var origin = output.Locations[0] as ScheduleOrigin;
            Assert.False(origin.Departure.IsNextDay);

            var stop = output.Locations[1] as ScheduleStop;
            Assert.False(stop.Arrival.IsNextDay);
            
            var pass = output.Locations[2] as SchedulePass;
            Assert.True(pass.PassesAt.IsNextDay);
            
            var stop2 = output.Locations[3] as ScheduleStop;
            Assert.True(stop2.Arrival.IsNextDay);

            var destination = output.Locations[4] as ScheduleDestination;
            Assert.True(destination.Arrival.IsNextDay);
        }
        
        [Fact]
        public void SetUniqueIds()
        {
            var schedule1 = MapSchedule();
            var schedule2 = MapSchedule();
            
            Assert.NotEqual(schedule1.Id, schedule2.Id);
        }
    }
}