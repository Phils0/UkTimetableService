using System;
using System.Collections.Generic;

namespace Timetable
{
    internal static class BankHolidays
    {
        private static readonly ISet<DateTime> English = new HashSet<DateTime>()
        {
            new DateTime(2019, 4, 19),
            new DateTime(2019, 4, 22),
            new DateTime(2019, 5, 6),
            new DateTime(2019, 5, 27),
            new DateTime(2019, 8, 26),
            new DateTime(2019, 12, 25),
            new DateTime(2019, 12, 26),
            new DateTime(2020, 1, 1),
            new DateTime(2020, 4, 10),
            new DateTime(2020, 4, 13),
            new DateTime(2020, 5, 4),
            new DateTime(2020, 5, 25),
            new DateTime(2020, 8, 31),
            new DateTime(2020, 12, 25),
            new DateTime(2020, 12, 28),
        };

        internal static bool IsEnglishBankHoliday(DateTime on) => English.Contains(on.Date);         
        
        private static readonly ISet<DateTime> Scotish = new HashSet<DateTime>()
        {
            new DateTime(2019, 4, 19),
            new DateTime(2019, 5, 6),
            new DateTime(2019, 5, 27),
            new DateTime(2019, 8, 5),
            new DateTime(2019, 12, 2),
            new DateTime(2019, 12, 25),
            new DateTime(2019, 12, 26),
            new DateTime(2020, 1, 1),
            new DateTime(2020, 1, 2),
            new DateTime(2020, 4, 10),
            new DateTime(2020, 5, 4),
            new DateTime(2020, 5, 25),
            new DateTime(2020, 8, 3),
            new DateTime(2020, 11, 30),
            new DateTime(2020, 12, 25),
            new DateTime(2020, 12, 28),
        };
        
        internal static bool IsScotishBankHoliday(DateTime on) => Scotish.Contains(on.Date);
    }
}