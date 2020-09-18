using System;
using System.Linq;
using NSubstitute;
using Serilog;

namespace Timetable.Test.Data
{
    public static class TestSchedules
    {
        public static Toc VirginTrains => new Toc() {Code = "VT", Name = "Virgin Trains"};
        private static readonly DateTime MondayAugust12 = new DateTime(2019, 8, 12);
        
        public static ResolvedAssociation[] NoAssociations => new ResolvedAssociation[0];
        
        public static ResolvedServiceStop CreateResolvedArrivalStop(
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            int id = 1,
            Service service = null,
            string retailServiceId = null,
            DateTime on =  default(DateTime),
            bool isCancelled = false,
            Station atLocation = null,
            Time when = default(Time))
        {
            on = on == default(DateTime) ? MondayAugust12 : on;
            var schedule = CreateSchedule(timetableId, indicator, calendar, stops, service, retailServiceId);
            var resolved = new ResolvedService(schedule, on, isCancelled);

            var origin = schedule.Locations.First() as ScheduleStop; 
            atLocation = atLocation ?? origin.Station;
            when = when.Equals(default(Time)) ? origin.Departure : when;
            var find = new StopSpecification(atLocation, when, on, TimesToUse.Arrivals);
            resolved.TryFindStop(find, out var stop);
            return stop;
        }
        
        public static ResolvedServiceStop CreateResolvedDepartureStop(
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            Service service = null,
            string retailServiceId = null,
            DateTime on =  default(DateTime),
            bool isCancelled = false,
            Station atLocation = null,
            Time when = default(Time))
        {
            on = on == default(DateTime) ? MondayAugust12 : on;
            var schedule = CreateSchedule(timetableId, indicator, calendar, stops, service, retailServiceId);
            var resolved = new ResolvedService(schedule, on, isCancelled);

            var origin = schedule.Locations.First() as ScheduleStop; 
            atLocation = atLocation ?? origin.Station;
            when = when.Equals(default(Time)) ? origin.Departure : when;
            var find = new StopSpecification(atLocation, when, on, TimesToUse.Departures);
            resolved.TryFindStop(find, out var stop);
            return stop;
        }
        
        public static ResolvedService CreateService(
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            Service service = null,
            string retailServiceId = null,
            DateTime on =  default(DateTime),
            bool isCancelled = false)
        {
            on = on == default(DateTime) ? MondayAugust12 : on;
            var schedule = CreateSchedule(timetableId, indicator, calendar, stops, service, retailServiceId);
            return new ResolvedService(schedule, on, isCancelled);
        }
        
        public static Schedule CreateSchedule(string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            Service service = null,
            string retailServiceId = null)
        {
            retailServiceId = retailServiceId ?? $"VT{timetableId.Substring(1, 4)}00";

            var schedule = new Schedule()
            {
                TimetableUid = timetableId,
                StpIndicator = indicator,
                RetailServiceId = retailServiceId,
                TrainIdentity = $"9Z{timetableId.Substring(1, 2)}",
                Operator = new Toc() { Code = retailServiceId.Substring(0, 2)},
                Status = ServiceStatus.PermanentPassenger,
                Category = ServiceCategory.ExpressPassenger,
                ReservationIndicator = ReservationIndicator.Supported,
                SeatClass = AccomodationClass.Both,
                SleeperClass = AccomodationClass.None,
                Calendar = calendar ?? EverydayAugust2019,
            };

            if (service != null)
                schedule.AddToService(service);

            if (indicator != StpIndicator.Cancelled)
            {
                stops = stops ?? DefaultLocations;
                foreach (var location in stops)
                {
                    location.SetParent(schedule);
                }              
            }

            return schedule;
        }

        public static Schedule CreateScheduleWithService(string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            Service service = null,
            string retailServiceId = null)
        {
            service = service ?? new Service(timetableId, Substitute.For<ILogger>());

            return CreateSchedule(timetableId, indicator, calendar, stops, service, retailServiceId);
        }

        public static Schedule CreateScheduleInTimetable(TimetableData timetable, 
            string timetableId = "X12345",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null, 
            ScheduleLocation[] stops = null,
            string retailServiceId = null)
        {
            var schedule = CreateSchedule(timetableId, indicator, calendar, stops, retailServiceId: retailServiceId);

            timetable.AddSchedule(schedule);

            return schedule;
        }

        public static ICalendar CreateEverydayCalendar(DateTime runsFrom, DateTime runsTo)
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
        
        public static Time NineForty => Ten.AddMinutes(-20);
        public static Time NineFiftyFive => Ten.AddMinutes(-5);
        public static Time Ten => new Time(new TimeSpan(10, 0,0 ));
        public static Time TenFifteen => Ten.AddMinutes(15);
        public static Time TenSixteen => Ten.AddMinutes(16);
        public static Time TenThirty => Ten.AddMinutes(30);
        public static Time TenTen => Ten.AddMinutes(10);
        public static Time TenTwenty => Ten.AddMinutes(20);
        public static Time TenTwentyOne => Ten.AddMinutes(21);
        public static Time TenTwentyFive => Ten.AddMinutes(25);
        public static Time TenForty => Ten.AddMinutes(40);
        public static Time TenFortyOne => Ten.AddMinutes(41);
        public static Time TenFortyFive => Ten.AddMinutes(45);
        public static Time TenFiftyFive => Ten.AddMinutes(55);
        
        public static ScheduleLocation[] DefaultLocations => CreateThreeStopSchedule(Ten);
        
        public static ScheduleLocation[] CreateThreeStopSchedule(Time start) => new[]
        {
            (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, start),
            TestScheduleLocations.CreateStop(TestStations.ClaphamJunction, start.AddMinutes(15)),
            TestScheduleLocations.CreatePass(TestStations.Vauxhall, start.AddMinutes(20)),
            TestScheduleLocations.CreateDestination(TestStations.Waterloo, start.AddMinutes(30))
        };
        
        public static ScheduleLocation[] CreateFourStopSchedule(Time start) => new[]
        {
            (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Surbiton, start),
            TestScheduleLocations.CreateStop(TestStations.Wimbledon, start.AddMinutes(10)),
            TestScheduleLocations.CreateStop(TestStations.ClaphamJunction, start.AddMinutes(15)),
            TestScheduleLocations.CreatePass(TestStations.Vauxhall, start.AddMinutes(20)),
            TestScheduleLocations.CreateDestination(TestStations.Waterloo, start.AddMinutes(30))
        };
        
        public static ScheduleLocation[] CreateWokingClaphamSchedule(Time start) => new[]
        {
            (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.Woking, start),
            TestScheduleLocations.CreateStop(TestStations.Weybridge, start.AddMinutes(15)),
            TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, start.AddMinutes(30))
        };
        
        public static ScheduleLocation[] CreateClaphamWokingSchedule(Time start) => new[]
        {
            (ScheduleLocation) TestScheduleLocations.CreateOrigin(TestStations.ClaphamJunction, start),
            TestScheduleLocations.CreateStop(TestStations.Weybridge, start.AddMinutes(15)),
            TestScheduleLocations.CreateDestination(TestStations.Woking, start.AddMinutes(30))
        };
        
        public static ResolvedServiceWithAssociations CreateServiceWithAssociation(
            DateTime on =  default(DateTime),
            bool associationIsCancelled = false,
            string mainUid = "X12345",
            string associatedUid = "A98765", 
            bool isNextDay = false)
        {
            var resolved = CreateService(mainUid, on: on);
            return CreateServiceWithAssociation(resolved, associationIsCancelled, associatedUid, isNextDay);
        }

        public static ResolvedServiceWithAssociations CreateServiceWithAssociation(
            ResolvedService resolved,
            bool associationIsCancelled = false,
            string associatedUid = "A98765", 
            bool isNextDay = false)
        {
            var association = CreateAssociation(resolved, associatedUid, associationIsCancelled, isNextDay);
            var resolvedWithAssociations = new ResolvedServiceWithAssociations(resolved, new [] { association });
            return resolvedWithAssociations;
        }
        
        public static ResolvedAssociation CreateAssociation(ResolvedService main, string associatedUid, bool associationIsCancelled = false, bool isNextDay = false)
        {
            var associated = CreateScheduleWithService(associatedUid, stops: CreateWokingClaphamSchedule(NineForty));
            var association = TestAssociations.CreateAssociationWithServices(main.Details.Service, associated.Service);
            var associatedDate = isNextDay ? main.On.AddDays(1) : main.On;
            var resolvedAssociated = new ResolvedService(associated, associatedDate, false);
            return new ResolvedAssociation(
                association,
                main.On,
                associationIsCancelled,
                resolvedAssociated);
        }
    }
}