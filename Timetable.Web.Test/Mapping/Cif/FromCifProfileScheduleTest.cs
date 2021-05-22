using System;
using System.Collections.Generic;
using AutoMapper;
using CifParser;
using NSubstitute;
using Serilog;
using Timetable.Test.Data;
using Xunit;
using TestSchedules = Timetable.Web.Test.Cif.TestSchedules;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class FromCifProfileScheduleTest
    {
        private readonly MapperConfiguration _fromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            _fromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapTimetableUid()
        {
            var output = MapSchedule();
            Assert.Equal("X12345", output.TimetableUid);
        }

        private static TocLookup CreateLookup() => new TocLookup(Substitute.For<ILogger>());
        
        private static TimetableData CreateTimetable() => new TimetableData(
            Timetable.Test.Data.Filters.Instance,
            Substitute.For<ILogger>());
        
        public CifSchedule MapSchedule(CifParser.Schedule input = null, IMapper mapper = null)
        {
            input = input ?? Test.Cif.TestSchedules.Test;
            mapper = mapper ?? _fromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Schedule, Timetable.CifSchedule>(input, o =>
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
            var schedule = Test.Cif.TestSchedules.Test;
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
            Assert.Equal(ServiceCategory.ExpressPassenger, output.Properties.Category);
        }

        [Fact]
        public void MapSeatClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.Both, output.Properties.SeatClass);
        }

        [Fact]
        public void MapSleeperClass()
        {
            var output = MapSchedule();
            Assert.Equal(AccomodationClass.None, output.Properties.SleeperClass);
        }

        [Fact]
        public void MapCatering()
        {
            var schedule = Test.Cif.TestSchedules.CreateScheduleDetails(Test.Cif.TestSchedules.X12345);
            schedule.Catering = "MT";
            var input = Test.Cif.TestSchedules.CreateSchedule(schedule);
            var output = MapSchedule(input);
            Assert.Equal(Catering.FirstClass | Catering.Trolley, output.Properties.Catering);
        }
        
        [Fact]
        public void MapNoCatering()
        {            
            var output = MapSchedule();
            Assert.Equal(Catering.None, output.Properties.Catering);
        }
        
        [Fact]
        public void MapReservationIndicator()
        {
            var output = MapSchedule();
            Assert.Equal(ReservationIndicator.Recommended, output.Properties.ReservationIndicator);
        }

        [Fact]
        public void MapCalendar()
        {
            var output = MapSchedule();
            var calendar = output.Calendar as CifCalendar;
            Assert.Equal(new DateTime(2019, 8, 1), calendar.RunsFrom);
            Assert.Equal(new DateTime(2019, 8, 31), calendar.RunsTo);
            Assert.Equal(DaysFlag.Weekdays, calendar.DayMask);
            Assert.Equal(BankHolidayRunning.RunsOnBankHoliday, calendar.BankHolidays);
        }

        [Fact]
        public void MapReusesExistingCalendar()
        {
            var mapper = _fromCifProfileConfiguration.CreateMapper();

            var output1 = MapSchedule();
            var output2 = MapSchedule();

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Calendar, output2.Calendar);
        }

        [Fact]
        public void MapRetailServiceId()
        {
            var output = MapSchedule();
            Assert.Equal("SW123400", output.Properties.RetailServiceId);
        }

        [Fact]
        public void MapTrainIdentity()
        {
            var output = MapSchedule();
            Assert.Equal("9X12", output.Properties.TrainIdentity);
        }
        
        [Fact]
        public void MapToc()
        {
            var output = MapSchedule();
            var toc = output.Operator;
            Assert.Equal("SW", toc.Code);
            Assert.Null(toc.Name);
        }

        [Fact]
        public void MapReusesExistingToc()
        {
            var lookup = CreateLookup();
            var mapper = _fromCifProfileConfiguration.CreateMapper();
            
            var output1 = mapper.Map<CifParser.Schedule, Timetable.CifSchedule>(Test.Cif.TestSchedules.Test, o =>
                {
                    o.Items.Add("Tocs", lookup);
                    o.Items.Add("Locations", TestData.Locations);
                    o.Items.Add("Timetable", CreateTimetable());
                });
            var output2 = mapper.Map<CifParser.Schedule, Timetable.CifSchedule>(Test.Cif.TestSchedules.Test, o => 
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
                    Test.Cif.TestSchedules.CreateScheduleDetails()
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal(Toc.Unknown, output.Operator);
            Assert.Equal("", output.Properties.RetailServiceId);
        }
        
        [Fact]
        public void MapNoRetailServiceIdSet()
        {
            var schedule = new CifParser.Schedule()
            {
                Records = new List<IRecord>(new IRecord[]
                {
                    Test.Cif.TestSchedules.CreateScheduleDetails(),
                    Test.Cif.TestSchedules.CreateScheduleExtraDetails(retailServiceId: "")
                })
            };

            var output = MapSchedule(schedule);

            Assert.Equal("", output.Properties.RetailServiceId);
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
                    Test.Cif.TestSchedules.CreateScheduleDetails(),
                    Test.Cif.TestSchedules.CreateScheduleExtraDetails(),
                    Test.Cif.TestSchedules.CreateOriginLocation(tiploc: "UNKNOWN1"),
                    Test.Cif.TestSchedules.CreateIntermediateLocation(tiploc: "UNKNOWN"),
                    Test.Cif.TestSchedules.CreatePassLocation(tiploc: "UNKNOWN2"),
                    Test.Cif.TestSchedules.CreateIntermediateLocation(tiploc: "UNKNOWN", sequence: 2),
                    Test.Cif.TestSchedules.CreateTerminalLocation(tiploc: "UNKNOWN3")
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
                    Test.Cif.TestSchedules.CreateScheduleDetails(),
                    Test.Cif.TestSchedules.CreateScheduleExtraDetails(),
                    Test.Cif.TestSchedules.CreateOriginLocation(departure: new TimeSpan(23, 30, 0)),
                    Test.Cif.TestSchedules.CreateIntermediateLocation(departure: new TimeSpan(23, 45, 0)),
                    Test.Cif.TestSchedules.CreatePassLocation(pass: new TimeSpan(0, 15, 0)),
                    Test.Cif.TestSchedules.CreateIntermediateLocation(departure: new TimeSpan(0, 30, 0), sequence: 2),
                    Test.Cif.TestSchedules.CreateTerminalLocation(arrival: new TimeSpan(0, 45, 0))
                })
            };
            
            var output = MapSchedule(schedule);

            var origin = output.Locations[0] as ScheduleStop;
            Assert.False(origin.Departure.IsNextDay);

            var stop = output.Locations[1] as ScheduleStop;
            Assert.False(stop.Arrival.IsNextDay);
            
            var pass = output.Locations[2] as SchedulePass;
            Assert.True(pass.PassesAt.IsNextDay);
            
            var stop2 = output.Locations[3] as ScheduleStop;
            Assert.True(stop2.Arrival.IsNextDay);

            var destination = output.Locations[4] as ScheduleStop;
            Assert.True(destination.Arrival.IsNextDay);
        }
        
        [Fact]
        public void ThrowsExceptionIfDoNotPassTocs()
        {
            var input = Test.Cif.TestSchedules.Test;
            var mapper = _fromCifProfileConfiguration.CreateMapper();

            var  ex = Assert.Throws<ArgumentException>(() => mapper.Map<CifParser.Schedule, Timetable.CifSchedule>(input, o =>
            {
                o.Items.Add("Locations", TestData.Locations);
                o.Items.Add("Timetable", CreateTimetable());
            }));
        }
    }
}