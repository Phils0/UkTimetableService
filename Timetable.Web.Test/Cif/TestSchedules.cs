using System;
using System.Collections.Generic;
using CifParser;
using CifParser.Records;

namespace Timetable.Web.Test.Cif
{
    internal static class TestSchedules
    {
        internal static CifParser.Schedule Test => new CifParser.Schedule()
        {
            Records = new List<IRecord>(new IRecord[]
            {
                CreateScheduleDetails(),
                CreateScheduleExtraDetails(),
                CreateOriginLocation(),
                CreateIntermediateLocation(),
                CreateIntermediateLocation(),
                CreateTerminalLocation()
            })
        };

        internal static ScheduleDetails CreateScheduleDetails(
            string timetableUid = "X12345", 
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

        internal static OriginLocation CreateOriginLocation()
        {
            return new OriginLocation();
        }

        internal static IntermediateLocation CreateIntermediateLocation()
        {
            return new IntermediateLocation();
        }

        internal static TerminalLocation CreateTerminalLocation()
        {
            return new TerminalLocation();
        }

        internal static ScheduleChange CreateScheduleChange()
        {
            return new ScheduleChange();
        }
    }
}