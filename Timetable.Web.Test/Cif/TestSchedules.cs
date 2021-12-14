using System;
using System.Collections.Generic;
using CifParser;
using CifParser.Records;

namespace Timetable.Web.Test.Cif
{
    internal static class TestSchedules
    {
        internal const string X12345 = "X12345";
        internal const string SW1234 = "SW123400";
        internal static CifParser.Schedule Test => CreateSchedule(X12345);

        internal static CifParser.Schedule CreateSchedule(string timetableUid)
        {
            var schedule = new CifParser.Schedule();
            schedule.Add(CreateScheduleDetails(timetableUid));
            schedule.Add(CreateScheduleExtraDetails());
            schedule.Add(CreateOriginLocation());
            schedule.Add(CreateIntermediateLocation());
            schedule.Add(CreatePassLocation(pass: TestTime.TenTwenty));
            schedule.Add(CreateIntermediateLocation(departure: TestTime.TenTwentyFive, sequence: 2));
            schedule.Add(CreateTerminalLocation());
            return schedule;
        }

        internal static ScheduleDetails CreateScheduleDetails(
            string timetableUid = X12345, 
            CifParser.Records.StpIndicator stp = CifParser.Records.StpIndicator.P)
        {
            return new ScheduleDetails()
            {
                TimetableUid = timetableUid,
                TrainIdentity = "9X12",
                StpIndicator = stp,
                RunsFrom = new DateTime(2019, 8, 1),
                RunsTo = new DateTime(2019, 8, 31),
                DayMask = "1111100",
                BankHolidayRunning = "",
                Status =  ServiceStatus.PermanentPassenger,
                Category = ServiceCategory.ExpressPassenger,
                SeatClass = ServiceClass.B,
                SleeperClass = ServiceClass.None,
                ReservationIndicator = CifParser.Records.ReservationIndicator.R
            };
        }

        public static ScheduleExtraData CreateScheduleExtraDetails(
            string retailServiceId = SW1234,
            string toc = "SW")
        {
            return new ScheduleExtraData()
            {
                RetailServiceId = retailServiceId,
                Toc = toc
            };
        }
        internal static OriginLocation CreateOriginLocation(string tiploc= "SURBITN", TimeSpan? departure = null)
        {
            departure = departure ?? TestTime.Ten;
            
            return new OriginLocation()
            {
                Location = tiploc,
                Sequence = 1,
                PublicDeparture = departure,
                WorkingDeparture =  departure.Value.Add(TestTime.ThirtySeconds),
                Platform = "1",
                Activities = "TB"
            };
        }
        
        internal static IntermediateLocation CreateIntermediateLocation(string tiploc= "CLPHMJC", TimeSpan? departure = null, int sequence = 1)
        {
            departure = departure ?? TestTime.TenFifteen;

            return new IntermediateLocation()
            {
                Location = tiploc,
                Sequence = sequence,
                PublicArrival = departure.Value.Subtract(TestTime.OneMinute),
                WorkingArrival =  departure.Value.Subtract(TestTime.ThirtySeconds),
                PublicDeparture = departure,
                WorkingDeparture =  departure.Value.Add(TestTime.ThirtySeconds),
                Platform = "10",
                Activities = "T"               
            };
        }

        internal static IntermediateLocation CreatePassLocation(string tiploc= "CLPHMJN", TimeSpan? pass = null)
        {
            pass = pass ?? TestTime.TenFifteen;

            return new IntermediateLocation()
            {
                Location = tiploc,
                Sequence = 1,
                WorkingPass = pass,
                Platform = "2",
                Activities = ""               
            };
        }
        
        internal static TerminalLocation CreateTerminalLocation(string tiploc= "WATRLMN", TimeSpan? arrival = null)
        {
            arrival = arrival ?? TestTime.TenThirty;
            
            return new TerminalLocation()
            {
                Location = tiploc,
                Sequence = 1,
                PublicArrival = arrival,
                WorkingArrival =  arrival.Value.Subtract(TestTime.ThirtySeconds),
                Platform = "5",
                Activities = "TF"               
            };
        }

        internal static ScheduleChange CreateScheduleChange()
        {
            return new ScheduleChange();
        }
    }
}