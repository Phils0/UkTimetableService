using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable;

public class CifServiceAnalyser
{
    public CifServiceAnalyser(CifService service,
        IReadOnlyDictionary<(StpIndicator indicator, ICalendar calendar), CifSchedule> schedules)
    {
        Schedules = schedules;
        Service = service;
    }
    
    public CifServiceAnalyser(CifService service, CifSchedule schedule) : this(service, CreateDictionary(schedule))
    {
    }

    private static IReadOnlyDictionary<(StpIndicator indicator, ICalendar calendar), CifSchedule> CreateDictionary(
        CifSchedule schedule)
    {
        var d = new Dictionary<(StpIndicator indicator, ICalendar calendar), CifSchedule>();
        d.Add((schedule.StpIndicator, schedule.Calendar), schedule);
        return d;
    }
    
    private IReadOnlyDictionary<(StpIndicator indicator, ICalendar calendar), CifSchedule> Schedules { get; }

    public CifService Service { get; }
    
    public  DaysFlag Days => ActiveSchedules
        .Aggregate(
            DaysFlag.None, 
            (current, schedule) => current | (schedule.Calendar as CifCalendar).DayMask);

    private IEnumerable<CifSchedule> ActiveSchedules => Schedules.Values
                .Where(s => s.StpIndicator != StpIndicator.Cancelled);
    
    private string GetAggregatedString(Func<CifSchedule, string> selector)
    {
        return ActiveSchedules
            .Aggregate("",
                (current, s) => current.Contains(selector(s)) ? current : $"{current}{selector(s)},")
            .TrimEnd(',');
    }

    public string Categories => GetAggregatedString(s => s.Category);

    public string TrainClasses => GetAggregatedString(s => s.SeatClass.ToString());
    
    public string Origins => GetAggregatedString(s => s.Origin.Location.ThreeLetterCode);

    public string Destinations => GetAggregatedString(s => s.Destination.Location.ThreeLetterCode);
    
    public string GetRoutes()
    {
        return GetAggregatedString(GetRoute);

        string GetRoute(CifSchedule s)
        {
            return $"{s.Origin.Location.ThreeLetterCode}-{s.Destination.Location.ThreeLetterCode}";
        }
    }

    public bool StopsAt(Station location)
    {
        return ActiveSchedules.Any(
            s => s.Locations
                .OfType<ScheduleStop>()
                .Any(l => l.IsStopAt(location))
            );
    }
}