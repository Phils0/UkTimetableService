using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    public abstract class ArrivalDeparturesControllerBase : ControllerBase, IGroupSearchDirection
    {
        protected readonly ILocationData _timetable;
        protected readonly IFilterFactory _filters;
        protected readonly StationGroupLookup _groups;
        protected readonly GroupSearchOrchestrator _orchestrator;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected ArrivalDeparturesControllerBase(ILocationData data, IFilterFactory filters,
            StationGroupLookup groups, GroupSearchOrchestrator orchestrator, IMapper mapper, ILogger logger)
        {
            _timetable = data;
            _filters = filters;
            _groups = groups;
            _orchestrator = orchestrator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>The resolved group/filter context for one request: the to/from filter plus the query-side and
        /// path-side groups (null when that side was a plain CRS). The path side is the one iterated across members.</summary>
        protected readonly record struct ResolvedGroupRequest(
            GatherConfiguration.GatherFilter Filter, StationGroup? QueryGroup, StationGroup? PathGroup);

        protected abstract GatherConfiguration.GatherFilter CreateFilter(Station station);

        protected abstract GatherConfiguration.GatherFilter CreateFilter(IReadOnlySet<Station> stations);

        // The direction-specific halves of IGroupSearchDirection: which optimiser entry-point to call (mapping the
        // path/query groups onto its origin/destination) and which stop time orders results. Kept protected (not
        // public) so MVC doesn't surface them as routable actions; the explicit interface members below adapt them
        // for the orchestrator. The controller "is" its own direction - the same shape as the CreateFilter overrides.
        protected abstract ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup);

        protected abstract Time TimeAtFoundStop(ResolvedServiceStop stop);

        ResolvedServiceStop[] IGroupSearchDirection.Optimise(ResolvedServiceStop[] candidates, StationGroup? pathGroup, StationGroup? queryGroup) =>
            Optimise(candidates, pathGroup, queryGroup);

        Time IGroupSearchDirection.TimeAtFoundStop(ResolvedServiceStop stop) => TimeAtFoundStop(stop);

        /// <summary>
        /// Resolves an inbound location code (case-insensitive) to either a single <see cref="Station"/> or a
        /// <see cref="StationGroup"/>, trying CRS first then the group lookup. Both members are null when neither
        /// matches; at most one is ever non-null, so the caller discriminates on <c>group != null</c>.
        /// </summary>
        protected (Station? station, StationGroup? group) ResolveGroupOrStation(string code)
        {
            if (string.IsNullOrEmpty(code))
                return (null, null);

            var normalised = code.ToUpperInvariant();
            if (_timetable.TryGetStation(normalised, out var station))
                return (station, null);

            return _groups.TryGet(normalised, out var group) ? (null, group) : (null, null);
        }

        /// <summary>
        /// Resolves the per-request group context shared by both windowed and full-day actions: the to/from filter
        /// (plus its query-side group) and the path-side group. Keeps the dispatch prologue in one place.
        /// </summary>
        protected ResolvedGroupRequest ResolveGroupRequest(SearchRequest request, TocFilter tocFilter)
        {
            var (filter, queryGroup) = ResolveQueryFilter(request, tocFilter);
            var (_, pathGroup) = ResolveGroupOrStation(request.Location);
            return new ResolvedGroupRequest(filter, queryGroup, pathGroup);
        }

        /// <summary>
        /// Runs a (possibly group) search and maps the response. When the path is a group it gathers across every
        /// member via <paramref name="findAtMember"/>, otherwise it finds at the single location; either way the
        /// gathered candidates are deduped (in <see cref="Process"/>) then collapsed/re-windowed. Windowing is taken
        /// from the request: full-day requests skip the re-window, windowed requests re-window around their pivot time.
        /// </summary>
        protected Task<IActionResult> RunSearch(SearchRequest request, TocFilter tocFilter, ResolvedGroupRequest resolved,
            Func<string, (FindStatus status, ResolvedServiceStop[] services)> findAtMember, bool includeStops, bool returnCancelled)
        {
            Func<Task<(FindStatus status, ResolvedServiceStop[] services)>> find = () => Task.FromResult(
                resolved.PathGroup != null
                    ? _orchestrator.GatherAcrossGroupMembers(resolved.PathGroup, findAtMember)
                    : findAtMember(request.Location));

            var pivot = request.At.FullDay ? (DateTime?)null : request.At.At;
            var optimise = _orchestrator.BuildOptimise(this, resolved.PathGroup, resolved.QueryGroup, pivot, request.At.Before, request.At.After);

            return Process(request, tocFilter, find, includeStops, returnCancelled, optimise);
        }

        protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after, string requestType, TocFilter tocs)
        {
            return CreateRequest(location, at, toFrom, before, after, requestType, tocs, false, "");
        }

        protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after, string requestType, TocFilter tocs, bool fullDay,  string dayBoundary)
        {
            return new SearchRequest()
            {
                Location = location,
                At = new Window()
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
        
        protected SearchRequest CreateFullDayRequest(string location, DateTime at, string toFrom, string requestType, TocFilter tocs,  string dayBoundary)
        {
            return CreateRequest(location, at, toFrom, 0, 0, requestType, tocs, true, dayBoundary);
        }
        
        protected async Task<IActionResult> Process(SearchRequest request, TocFilter tocFilter, Func<Task<(FindStatus status, ResolvedServiceStop[] services)>> find, bool includeStops, bool returnCancelled, Func<ResolvedServiceStop[], ResolvedServiceStop[]> optimise)
        {
            using (LogContext.PushProperty("Request", request, true))
            {
                if (tocFilter.HasInvalidTocs)
                {
                    _logger.Information("Invalid tocs provided in request {t}", tocFilter.Tocs); 
                    return await Task.FromResult(
                        BadRequest(new BadRequestResponse()
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
                        var (findStatusAfterFilter, services) = _timetable.Filters.Filter(found, returnCancelled);
                        if (findStatusAfterFilter == FindStatus.Success)
                        {
                            // Station-group searches collapse a service's multiple member-stops to one canonical row
                            // here - AFTER the cancelled/STP-overlay dedup above, so the optimiser only ever sees one
                            // row per (TimetableUid, On) per member. The transform is the identity for plain CRS
                            // searches, leaving their behaviour byte-for-byte unchanged.
                            services = optimise(services);
                            return MapSuccessResponse(request, includeStops, services);
                        }

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
                    _logger.Error(e, "Error when processing : {@request}", request);
                }

                return CreateNoServiceResponse(status, request);
            }
        }

        private IActionResult MapSuccessResponse(SearchRequest request, bool includeStops, ResolvedServiceStop[] services)
        {
            if (includeStops)
            {
                var items = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundServiceItem[]>(services,
                    options => ServiceController.InitialiseContext(options));
                return Ok(new Model.FoundServiceResponse()
                {
                    Request = request,
                    GeneratedAt = DateTime.Now,
                    Services = items
                });
            }
            else
            {
                var items = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundSummaryItem[]>(services,
                    options => ServiceController.InitialiseContext(options));
                return Ok(new Model.FoundSummaryResponse()
                {
                    Request = request,
                    GeneratedAt = DateTime.Now,
                    Services = items
                });
            }
        }

        /// <summary>
        /// Builds the to/from query filter and, when that query parameter resolves to a station group, returns the
        /// resolved <see cref="StationGroup"/> so the caller can hand it to the optimiser. A single CRS yields a
        /// single-station filter (group null); a group yields the multi-station filter; an unknown code is
        /// warn-and-dropped (no filter, group null) - matching the long-standing handling of unknown CRS in to/from.
        /// </summary>
        protected (GatherConfiguration.GatherFilter filter, StationGroup? queryGroup) ResolveQueryFilter(SearchRequest request, TocFilter tocFilter)
        {
            GatherConfiguration.GatherFilter filter = _filters.NoFilter;
            StationGroup? queryGroup = null;
            if (!string.IsNullOrEmpty(request.ComingFromGoingTo))
            {
                var (station, group) = ResolveGroupOrStation(request.ComingFromGoingTo);
                if (group != null)
                {
                    filter = CreateFilter(group.Members);
                    queryGroup = group;
                }
                else if (station != null)
                {
                    filter = CreateFilter(station);
                }
                else
                {
                    _logger.Warning("Not adding filter.  Did not find location {toFrom}", request.ComingFromGoingTo);
                }
            }

            filter = _filters.ProvidedByToc(tocFilter, filter);
            return (filter, queryGroup);
        }
        
        private ObjectResult CreateNoServiceResponse(FindStatus status, SearchRequest request)
        {
            var reason = "";
            
            switch (status)
            {
                case FindStatus.LocationNotFound :
                    reason = $"Did not find location {request.Location}";
                    return  NotFound(CreateResponseObject());
                case FindStatus.NoServicesForLocation :
                    reason = $"Did not find services for {request}";
                    return  NotFound(CreateResponseObject());
                case FindStatus.Error :
                    reason = $"Error while finding services for {request}";
                    return StatusCode(500,  CreateResponseObject());
                default:
                    reason = $"Unknown reason why could not find anything";
                    _logger.Error("{reason} : {@request}", reason, request);
                    return StatusCode(500,  CreateResponseObject());
            }

            NotFoundResponse CreateResponseObject()
            {
                return new NotFoundResponse()
                {
                    Request = request,
                    GeneratedAt = DateTime.Now,
                    Reason = reason
                };
            }
        }
    }
}