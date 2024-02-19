using System;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Test;

public class CifServiceAnalyserTest
{
    [Fact]
    public void ReturnsDays()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal(DaysFlag.Everyday, analyser.Days);
    }
    
    [Fact]
    public void ReturnsCategory()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("XX", analyser.Categories);
    }
    
    [Fact]
    public void ReturnsSeatClass()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("Both", analyser.SeatClasses);
    }
    
    [Fact]
    public void ReturnsOrigins()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("SUR", analyser.Origins);
    }
    
    [Fact]
    public void ReturnsDestinations()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("WAT", analyser.Destinations);
    }
    
    [Fact]
    public void ReturnsRoutes()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("SUR-WAT", analyser.GetRoutes());
    }
    
    [Fact]
    public void ReturnsMultipleDays()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Weekend));
        var service = baseSchedule.Service;
        TestSchedules.CreateSchedule(indicator: StpIndicator.New, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);
        
        var analyser = service.CreateAnalyser();
        Assert.Equal(DaysFlag.Monday | DaysFlag.Saturday | DaysFlag.Sunday, analyser.Days);
    }
    
    [Fact]
    public void ReturnsMultipleCategory()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        var overrideSchedule = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);
        overrideSchedule.Category = ServiceCategory.BusReplacement;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("BR,XX", analyser.Categories);
    }
    
    [Fact]
    public void ReturnsMultipleSeatClass()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        var overrideSchedule = TestSchedules.CreateSchedule(indicator: StpIndicator.Override, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);
        overrideSchedule.SeatClass = AccomodationClass.Standard;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("Standard,Both", analyser.SeatClasses);
    }
    
    public static Time Ten => TestSchedules.Ten;
    
    [Fact]
    public void ReturnsMultipleOrigins()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        var overrideSchedule = TestSchedules.CreateSchedule(
            indicator: StpIndicator.New, 
            calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Saturday), 
            stops: new ScheduleLocation[]
            {
                TestScheduleLocations.CreateOrigin(TestStations.Vauxhall, Ten.AddMinutes(20)),
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, Ten.AddMinutes(30))
            }, 
            service: service);
        overrideSchedule.Category = ServiceCategory.BusReplacement;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("VXH,SUR", analyser.Origins);
    }
    
    [Fact]
    public void ReturnsMultipleDestinations()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        var overrideSchedule = TestSchedules.CreateSchedule(
            indicator: StpIndicator.New, 
            calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Saturday), 
            stops: new ScheduleLocation[]
            {
                TestScheduleLocations.CreatePass(TestStations.Vauxhall, Ten.AddMinutes(20)),
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, Ten.AddMinutes(30))
            }, 
            service: service);
        overrideSchedule.Category = ServiceCategory.BusReplacement;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("CLJ,WAT", analyser.Destinations);
    }
    
    [Fact]
    public void ReturnsMultipleRoutes()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        var overrideSchedule = TestSchedules.CreateSchedule(
            indicator: StpIndicator.New, 
            calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Saturday), 
            stops: new ScheduleLocation[]
            {
                TestScheduleLocations.CreateOrigin(TestStations.Vauxhall, Ten.AddMinutes(20)),
                TestScheduleLocations.CreateDestination(TestStations.ClaphamJunction, Ten.AddMinutes(30))
            }, 
            service: service);
        overrideSchedule.Category = ServiceCategory.BusReplacement;
        
        var analyser = service.CreateAnalyser();
        Assert.Equal("VXH-CLJ,SUR-WAT", analyser.GetRoutes());
    }
    
    [Fact]
    public void IgnoresCancelledSchedules()
    {
        var baseSchedule = TestSchedules.CreateScheduleWithService(indicator: StpIndicator.Permanent, calendar: TestSchedules.EverydayAugust2019);
        var service = baseSchedule.Service;
        TestSchedules.CreateSchedule(indicator: StpIndicator.Cancelled, calendar: TestSchedules.CreateAugust2019Calendar(DaysFlag.Monday), service: service);

        var analyser = service.CreateAnalyser();
        Assert.Equal(DaysFlag.Everyday, analyser.Days);
    }
}