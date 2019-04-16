using System;
using System.Collections.Generic;
using CifParser;
using CifParser.Records;

namespace Timetable.Web.Test.Cif
{
    internal static class TestTime
    {
        internal static readonly TimeSpan Ten = new TimeSpan(10, 0, 0);
        internal static readonly TimeSpan TenFifteen = new TimeSpan(10, 15, 0);
        internal static readonly TimeSpan TenTwenty = new TimeSpan(10, 20, 0);
        internal static readonly TimeSpan TenTwentyFive = new TimeSpan(10, 25, 0);
        internal static readonly TimeSpan TenThirty = new TimeSpan(10, 30, 0);
        
        internal static readonly TimeSpan OneMinute = new TimeSpan(0, 1, 0);
        internal static readonly TimeSpan ThirtySeconds = new TimeSpan(0, 0, 30);     
    }
    
    internal static class TestSchedules
    {
        internal const string X12345 = "X12345";

        internal static CifParser.Schedule Test => new CifParser.Schedule()
        {
            Records = new List<IRecord>(new IRecord[]
            {
                CreateScheduleDetails(),
                CreateScheduleExtraDetails(),
                CreateOriginLocation(),
                CreateIntermediateLocation(),
                CreatePassLocation(pass: TestTime.TenTwenty),
                CreateIntermediateLocation(departure: TestTime.TenTwentyFive, sequence: 2),
                CreateTerminalLocation()
            })
        };

        internal static ScheduleDetails CreateScheduleDetails(
            string timetableUid = X12345, 
            CifParser.Records.StpIndicator stp = CifParser.Records.StpIndicator.P)
        {
            return new ScheduleDetails()
            {
                TimetableUid = timetableUid,
                StpIndicator = stp,
                RunsFrom = new DateTime(2019, 8, 1),
                RunsTo = new DateTime(2019, 8, 31),
                DayMask = "YYYYYNN",
                BankHolidayRunning = "",
                Status =  ServiceStatus.PermanentPassenger,
                Category = ServiceCategory.ExpressPassenger,
                SeatClass = ServiceClass.B,
                SleeperClass = ServiceClass.None,
                ReservationIndicator = CifParser.Records.ReservationIndicator.R
            };
        }

        internal static ScheduleExtraData CreateScheduleExtraDetails(
            string retailServieId = "SW123400",
            string toc = "SW")
        {
            return new ScheduleExtraData()
            {
                RetailServiceId = retailServieId,
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