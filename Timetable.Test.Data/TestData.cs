using System;
using NSubstitute;
using NSubstitute.Core.Arguments;
using Serilog;

namespace Timetable.Test.Data
{
    public static class TestData
    {
        private static readonly Toc VirginTrains = new Toc() { Code = "VT" , Name = "Virgin Trains"};

        public static ILocationData Locations => new Timetable.LocationData(
            new []
            {
                TestLocations.Surbiton,
                TestLocations.WaterlooMain,
                TestLocations.WaterlooWindsor,
                TestLocations.CLPHMJN,
                TestLocations.CLPHMJC
            }, Substitute.For<ILogger>());

        public static Schedule CreateSchedule(string id = "X12345", StpIndicator indicator = StpIndicator.Permanent, ICalendar calendar = null, ScheduleLocation[] locations = null)
        {
            return new Schedule()
            {
                TimetableUid = id,
                StpIndicator = indicator,
                RetailServiceId = $"VT{id.Substring(1, 4)}00",
                Toc = VirginTrains,
                Status = ServiceStatus.PermanentPassenger,
                Category = ServiceCategory.ExpressPassenger,
                ReservationIndicator = ReservationIndicator.Supported,
                SeatClass = AccomodationClass.Both,
                SleeperClass = AccomodationClass.None,
                Calendar = calendar ?? EverydayAugust2019,
                Locations = locations ?? DefaultLocations
            };
        }
        
        public static ICalendar CreateEverydayCalendar(DateTime runsFrom,
            DateTime runsTo)
        {
            var calendar = new Calendar(
                runsFrom,
                runsTo,
                DaysFlag.Everyday,
                BankHolidayRunning.RunsOnBankHoliday);
            calendar.Generate();
            return calendar;
        }       
        
        public static ICalendar CreateAugust2019Calendar(DaysFlag dayMask = DaysFlag.Everyday,
            BankHolidayRunning bankHolidays = BankHolidayRunning.RunsOnBankHoliday)
        {
            var calendar = new Calendar(
                new DateTime(2019, 8, 1),
                new DateTime(2019, 8, 31),
                dayMask,
                bankHolidays);
            calendar.Generate();
            return calendar;
        }

        public static ICalendar EverydayAugust2019 => CreateAugust2019Calendar();

        public static ScheduleLocation[] DefaultLocations => new[]
        {
            TestScheduleLocations.CreateOrigin(TestLocations.Surbiton, new Time())
        };


    }
}