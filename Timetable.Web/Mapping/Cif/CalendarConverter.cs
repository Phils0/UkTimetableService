using System;
using System.Collections.Concurrent;
using AutoMapper;
using CifParser.Records;

namespace Timetable.Web.Mapping.Cif
{
    public class CalendarConverter : IValueConverter<ScheduleDetails, CifCalendar>, IValueConverter<CifParser.Records.Association, CifCalendar>
    {
        private ConcurrentDictionary<CifCalendar, CifCalendar> _lookup = new ConcurrentDictionary<CifCalendar, CifCalendar>();

        public CifCalendar Convert(ScheduleDetails source, ResolutionContext context)
        {
            var calendar = new CifCalendar(
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
                if (values[i] == '1')
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

        public CifCalendar Convert(CifParser.Records.Association source, ResolutionContext context)
        {
            var calendar = new CifCalendar(
                    source.RunsFrom,
                    source.RunsTo.Value,
                    MapMask(source.DayMask),
                    BankHolidayRunning.RunsOnBankHoliday)    // Default to applies Bank holidays
                ;
            var actual = _lookup.GetOrAdd(calendar, calendar);
            actual.Generate();
            return actual;
        }
    }
}