using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers;

public abstract class ArrivalDeparturesControllerBase : ControllerBase
{
    protected readonly IFilterFactory Filters;
    protected readonly ILogger Logger;
    protected readonly IMapper Mapper;
    protected readonly ILocationData Timetable;

    protected ArrivalDeparturesControllerBase(ILocationData data, IFilterFactory filters, IMapper mapper,
        ILogger logger)
    {
        Timetable = data;
        Filters = filters;
        Mapper = mapper;
        Logger = logger;
    }

    protected abstract GatherConfiguration.GatherFilter CreateFilter(Station station);

    protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after,
        string requestType, TocFilter tocs)
    {
        return CreateRequest(location, at, toFrom, before, after, requestType, tocs, false, "");
    }

    private SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after,
        string requestType, TocFilter tocs, bool fullDay, string dayBoundary)
    {
        return new SearchRequest
        {
            Location = location,
            At = new Window
            {
                At = at,
                Before = before,
                After = after,
                FullDay = fullDay,
                DayBoundary = dayBoundary
            },
            TocFilter = tocs.Tocs,
            ComingFromGoingTo = toFrom,
            Type = requestType
        };
    }

    protected SearchRequest CreateFullDayRequest(string location, DateTime at, string toFrom, string requestType,
        TocFilter tocs, string dayBoundary)
    {
        return CreateRequest(location, at, toFrom, 0, 0, requestType, tocs, true, dayBoundary);
    }

    protected async Task<IActionResult> Process(SearchRequest request, TocFilter tocFilter,
        Func<Task<(FindStatus status, ResolvedServiceStop[] services)>> find, bool includeStops, bool returnCancelled)
    {
        using (LogContext.PushProperty("Request", request, true))
        {
            if (tocFilter.HasInvalidTocs)
            {
                Logger.Information("Invalid tocs provided in request {t}", tocFilter.Tocs);
                return await Task.FromResult(
                    BadRequest(new BadRequestResponse
                    {
                        Request = request,
                        GeneratedAt = DateTime.Now,
                        Reason = $"Invalid tocs provided in request {tocFilter.Tocs}"
                    }));
            }

            FindStatus status;
            try
            {
                var (findStatus, found) = await find();

                if (findStatus == FindStatus.Success)
                {
                    var (findStatusAfterFilter, services) = Timetable.Filters.Filter(found, returnCancelled);
                    if (findStatusAfterFilter == FindStatus.Success)
                        return MapSuccessResponse(request, includeStops, services);

                    status = findStatusAfterFilter;
                }
                else
                {
                    status = findStatus;
                }
            }
            catch (Exception e)
            {
                status = FindStatus.Error;
                Logger.Error(e, "Error when processing : {@request}", request);
            }

            return CreateNoServiceResponse(status, request);
        }
    }

    private IActionResult MapSuccessResponse(SearchRequest request, bool includeStops, ResolvedServiceStop[] services)
    {
        if (includeStops)
        {
            var items = Mapper.Map<ResolvedServiceStop[], FoundServiceItem[]>(services,
                options => ServiceController.InitialiseContext(options));
            return Ok(new FoundServiceResponse
            {
                Request = request,
                GeneratedAt = DateTime.Now,
                Services = items
            });
        }
        else
        {
            var items = Mapper.Map<ResolvedServiceStop[], FoundSummaryItem[]>(services,
                options => ServiceController.InitialiseContext(options));
            return Ok(new FoundSummaryResponse
            {
                Request = request,
                GeneratedAt = DateTime.Now,
                Services = items
            });
        }
    }

    protected GatherConfiguration CreateGatherConfig(SearchRequest request, TocFilter tocFilter)
    {
        var filter = CreateFilter(request, tocFilter);
        return new GatherConfiguration(request.At.Before, request.At.After, false, filter);
    }

    protected GatherConfiguration.GatherFilter CreateFilter(SearchRequest request, TocFilter tocFilter)
    {
        var filter = Filters.NoFilter;
        if (!string.IsNullOrEmpty(request.ComingFromGoingTo))
        {
            if (Timetable.TryGetStation(request.ComingFromGoingTo, out var station))
                filter = CreateFilter(station);
            else
                Logger.Warning("Not adding filter.  Did not find location {toFrom}", request.ComingFromGoingTo);
        }

        filter = Filters.ProvidedByToc(tocFilter, filter);
        return filter;
    }

    private ObjectResult CreateNoServiceResponse(FindStatus status, SearchRequest request)
    {
        var reason = "";

        switch (status)
        {
            case FindStatus.LocationNotFound:
                reason = $"Did not find location {request.Location}";
                return NotFound(CreateResponseObject());
            case FindStatus.NoServicesForLocation:
                reason = $"Did not find services for {request}";
                return NotFound(CreateResponseObject());
            case FindStatus.Error:
                reason = $"Error while finding services for {request}";
                return StatusCode(500, CreateResponseObject());
            default:
                reason = "Unknown reason why could not find anything";
                Logger.Error("{reason} : {@request}", reason, request);
                return StatusCode(500, CreateResponseObject());
        }

        NotFoundResponse CreateResponseObject()
        {
            return new NotFoundResponse
            {
                Request = request,
                GeneratedAt = DateTime.Now,
                Reason = reason
            };
        }
    }
}