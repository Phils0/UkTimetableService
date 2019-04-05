using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AutoMapper;
using CifParser.Records;
using Microsoft.CodeAnalysis.Operations;

namespace Timetable.Web.Mapping
{
    public class CalendarConverter : IValueConverter<ScheduleDetails, Calendar>
    {
        private ConcurrentDictionary<Calendar, Calendar> _lookup = new ConcurrentDictionary<Calendar, Calendar>();

        public Calendar Convert(ScheduleDetails source, ResolutionContext context)
        {
            var calendar = new Calendar(
                    source.RunsFrom,
                    source.RunsTo.Value,
                    MapMask(source.DayMask),
                    MapBankHoliday(source.BankHolidayRunning))
                ;
            var actual = _lookup.GetOrAdd(calendar, calendar);
            actual.Generate();
            return actual;
        }

        internal static DaysFlag MapMask(string mask)
        {
            var flags = 0;

            var values = mask.ToCharArray();

            for (int i = 0; i < 7; i++)
            {
                if (values[i] == 'Y')
                    flags = flags + (1 << i);
            }

            return (DaysFlag) flags;
        }

        internal static BankHolidayRunning MapBankHoliday(string bankHoliday)
        {
            switch (bankHoliday)
            {
                case "":
                    return BankHolidayRunning.RunsOnBankHoliday;
                case "X":
                    return BankHolidayRunning.DoesNotRunOnEnglishBankHolidays;
                case "G":
                    return BankHolidayRunning.DoesNotRunOnScotishBankHolidays;
                case "E":
                default:
                    throw new ArgumentOutOfRangeException($"Unknown bank holiday value: {bankHoliday}");
            }
        }
    }
}