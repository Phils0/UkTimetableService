using System;
using System.Collections.Generic;

namespace Timetable
{
    internal static class BankHolidays
    {
        private static readonly ISet<DateTime> English = new HashSet<DateTime>()
        {
            new DateTime(2020, 1, 1),
            new DateTime(2020, 4, 10),
            new DateTime(2020, 4, 13),
            new DateTime(2020, 5, 8),
            new DateTime(2020, 5, 25),
            new DateTime(2020, 8, 31),
            new DateTime(2020, 12, 25),
            new DateTime(2020, 12, 28),
            new DateTime(2021, 1, 1),
            new DateTime(2021, 4, 2),
            new DateTime(2021, 4, 5),
            new DateTime(2021, 5, 3),
            new DateTime(2021, 5, 31),
            new DateTime(2021, 8, 30),
            new DateTime(2021, 12, 27),
            new DateTime(2021, 12, 28),
            new DateTime(2022, 1, 3),
            new DateTime(2022, 4, 15),
            new DateTime(2022, 4, 18),
            new DateTime(2022, 5, 2),
            new DateTime(2022, 5, 30),
            new DateTime(2022, 8, 29),
            new DateTime(2022, 12, 26),
            new DateTime(2022, 12, 27),            
        };

        internal static bool IsEnglishBankHoliday(DateTime on) => English.Contains(on.Date);         
        
        private static readonly ISet<DateTime> Scotish = new HashSet<DateTime>()
        {
            new DateTime(2020, 1, 1),
            new DateTime(2020, 1, 2),
            new DateTime(2020, 4, 10),
            new DateTime(2020, 5, 8),
            new DateTime(2020, 5, 25),
            new DateTime(2020, 8, 3),
            new DateTime(2020, 11, 30),
            new DateTime(2020, 12, 25),
            new DateTime(2020, 12, 28),
            new DateTime(2021, 1, 1),
            new DateTime(2021, 1, 2),
            new DateTime(2021, 4, 2),
            new DateTime(2021, 5, 3),
            new DateTime(2021, 5, 31),
            new DateTime(2021, 8, 2),
            new DateTime(2021, 11, 30),
            new DateTime(2021, 12, 27),
            new DateTime(2021, 12, 28),
            new DateTime(2022, 1, 3),
            new DateTime(2022, 1, 4),
            new DateTime(2022, 4, 15),
            new DateTime(2022, 5, 3),
            new DateTime(2022, 5, 30),
            new DateTime(2022, 8, 1),
            new DateTime(2022, 11, 30),
            new DateTime(2022, 12, 26),
            new DateTime(2022, 12, 27),
        };
        
        internal static bool IsScotishBankHoliday(DateTime on) => Scotish.Contains(on.Date);
    }
}