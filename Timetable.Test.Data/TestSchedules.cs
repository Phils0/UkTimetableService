using System;
using System.Linq;

namespace Timetable.Test.Data
{
    public static class TestSchedules
    {
        private static readonly Toc VirginTrains = new Toc() {Code = "VT", Name = "Virgin Trains"};
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        
        public static ResolvedServiceStop CreateResolvedStop(
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, ScheduleLocation[] locations = null,
            int id = 1,
            Service service = null,
            string retailServiceId = null,
            DateTime on =  default(DateTime),
            bool isCancelled = false,
            Station atLocation = null,
            Time when = default(Time))
        {
            on = on == default(DateTime) ? MondayAugust12 : on;
            var schedule = CreateSchedule(timetableId, indicator, calendar, locations, id, service, retailServiceId);
            var resolved = new ResolvedService(schedule, on, isCancelled);

            var origin = schedule.Locations.First() as ScheduleOrigin; 
            atLocation = atLocation ?? origin.Station;
            when = when.Equals(default(Time)) ? origin.Departure : when;
            resolved.TryFindStop(atLocation, when, out var stop);
            return stop;
        }
        
        public static ResolvedService CreateService(
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, ScheduleLocation[] locations = null,
            int id = 1,
            Service service = null,
            string retailServiceId = null,
            DateTime on =  default(DateTime),
            bool isCancelled = false)
        {
            on = on == default(DateTime) ? MondayAugust12 : on;
            var schedule = CreateSchedule(timetableId, indicator, calendar, locations, id, service, retailServiceId);
            return new ResolvedService(schedule, on, isCancelled);
        }
        
        public static Schedule CreateSchedule(string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, ScheduleLocation[] locations = null,
            int id = 1,
            Service service = null,
            string retailServiceId = null)
        {
            var schedule = new Schedule(id)
            {
                TimetableUid = timetableId,
                StpIndicator = indicator,
                RetailServiceId = retailServiceId ?? $"VT{timetableId.Substring(1, 4)}00",
                TrainIdentity = $"9Z{timetableId.Substring(1, 2)}",
                Operator = VirginTrains,
                Status = ServiceStatus.PermanentPassenger,
                Category = ServiceCategory.ExpressPassenger,
                ReservationIndicator = ReservationIndicator.Supported,
                SeatClass = AccomodationClass.Both,
                SleeperClass = AccomodationClass.None,
                Calendar = calendar ?? EverydayAugust2019,
            };

            if (service != null)
                schedule.AddToService(service);
            
            locations = locations ?? DefaultLocations;
            foreach (var location in locations)
            {
                location.SetParent(schedule);
            }

            return schedule;
        }

        public static Schedule CreateScheduleWithService(string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, ScheduleLocation[] locations = null,
            int id = 1,
            Service service = null)
        {
            service = service ?? new Service(timetableId);

            return CreateSchedule(timetableId, indicator, calendar, locations, id, service);
        }

        public static Schedule CreateScheduleInTimetable(TimetableData timetable, 
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] locations = null,
            int id = 1,
            string retailServiceId = null)
        {
            var schedule = CreateSchedule(timetableId, indicator, calendar, locations, id, retailServiceId: retailServiceId);

            timetable.AddSchedule(schedule);

            return schedule;
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
        public static Time Ten => new Time(new TimeSpan(10, 0,0 ));
        public static Time TenFifteen => new Time(new TimeSpan(10, 15,0 ));
        public static Time TenThirty => new Time(new TimeSpan(10, 30,0 ));

        public static ScheduleLocation[] DefaultLocations => new[]
        {
            (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, Ten),
            (ScheduleLocation) TestScheduleLocations.CreateStop(TestStations.ClaphamJunction, TenFifteen),
            TestScheduleLocations.CreateDestination(TestStations.Waterloo, TenThirty)
        };
    }
}