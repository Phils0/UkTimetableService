using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    public abstract class ArrivalDeparturesControllerBase : ControllerBase
    {
        protected readonly ILocationData _timetable;
        protected readonly IFilterFactory _filters;
        protected readonly StationGroupLookup _groups;
        protected readonly IStationGroupStopOptimiser _optimiser;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected ArrivalDeparturesControllerBase(ILocationData data, IFilterFactory filters,
            StationGroupLookup groups, IStationGroupStopOptimiser optimiser, IMapper mapper, ILogger logger)
        {
            _timetable = data;
            _filters = filters;
            _groups = groups;
            _optimiser = optimiser;
            _mapper = mapper;
            _logger = logger;
        }

        protected abstract GatherConfiguration.GatherFilter CreateFilter(Station station);

        protected abstract GatherConfiguration.GatherFilter CreateFilter(IReadOnlySet<Station> stations);

        /// <summary>
        /// Collapses one service's gathered member-stops to a single canonical row. Departures and arrivals differ
        /// only in which optimiser entry-point they call and how they map the origin/destination groups.
        /// </summary>
        protected abstract ResolvedServiceStop[] Optimise(ResolvedServiceStop[] candidates, StationGroup? originGroup, StationGroup? destinationGroup);

        /// <summary>
        /// The time of the canonical found stop used to order and re-window results: the departure time at the origin
        /// for /departures, the arrival time at the destination for /arrivals.
        /// </summary>
        protected abstract Time TimeAtFoundStop(ResolvedServiceStop stop);

        /// <summary>
        /// Gathers results across every member of a path-side group, concatenating the per-member finds. The status is
        /// Success if any member returned services; the optimiser later collapses a service calling at several members
        /// down to one canonical row.
        /// </summary>
        protected (FindStatus status, ResolvedServiceStop[] services) GatherAcrossGroupMembers(
            StationGroup group, Func<string, (FindStatus status, ResolvedServiceStop[] services)> findAtMember)
        {
            var all = new List<ResolvedServiceStop>();
            var anySuccess = false;
            foreach (var member in group.Members)
            {
                var (status, services) = findAtMember(member.ThreeLetterCode);
                if (status == FindStatus.Success)
                {
                    all.AddRange(services);
                    anySuccess = true;
                }
            }

            return anySuccess
                ? (FindStatus.Success, all.ToArray())
                : (FindStatus.NoServicesForLocation, Array.Empty<ResolvedServiceStop>());
        }

        /// <summary>
        /// Builds the post-dedup transform handed to <see cref="Process"/>: collapse member-stops via
        /// <see cref="Optimise"/>, then - in windowed mode only, when the path side was a group and so over-gathered
        /// one window per member (<paramref name="pathGroup"/> non-null, <paramref name="pivot"/> non-null) - re-window
        /// the merged set back around the requested time. Returns null when neither side is a group, leaving the
        /// plain-CRS path byte-for-byte unchanged.
        /// </summary>
        protected Func<ResolvedServiceStop[], ResolvedServiceStop[]>? BuildGroupOptimise(
            StationGroup? originGroup, StationGroup? destinationGroup, StationGroup? pathGroup,
            Time? pivot, ushort before, ushort after)
        {
            if (originGroup == null && destinationGroup == null)
                return null;

            return stops =>
            {
                var optimised = Optimise(stops, originGroup, destinationGroup);
                return pathGroup != null && pivot != null
                    ? ReWindow(optimised, pivot.Value, before, after)
                    : optimised;
            };
        }

        private ResolvedServiceStop[] ReWindow(IEnumerable<ResolvedServiceStop> stops, Time pivot, int before, int after)
        {
            if (before == 0 && after == 0) after = 1; // mirror GatherConfiguration's "always return at least one"
            var ordered = stops.OrderBy(TimeAtFoundStop, Time.EarlierLaterComparer).ToList();
            var beforePivot = ordered.Where(s => TimeAtFoundStop(s).IsBefore(pivot)).TakeLast(before);
            var fromPivot = ordered.Where(s => !TimeAtFoundStop(s).IsBefore(pivot)).Take(after);
            return beforePivot.Concat(fromPivot).ToArray();
        }

        /// <summary>
        /// Resolves an inbound location code (case-insensitive) to either a single <see cref="Station"/> or a
        /// <see cref="StationGroup"/>. CRS is tried first, then the group lookup. Returns false when neither matches.
        /// </summary>
        protected bool TryResolveGroupOrStation(string code, out Station station, out StationGroup? group)
        {
            group = null;
            station = Station.NotSet;
            if (string.IsNullOrEmpty(code))
                return false;

            var normalised = code.ToUpperInvariant();
            if (_timetable.TryGetStation(normalised, out station))
                return true;

            return _groups.TryGet(normalised, out group);
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
        
        protected async Task<IActionResult> Process(SearchRequest request, TocFilter tocFilter, Func<Task<(FindStatus status, ResolvedServiceStop[] services)>> find, bool includeStops, bool returnCancelled, Func<ResolvedServiceStop[], ResolvedServiceStop[]>? optimise = null)
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
                            // Station-group searches collapse a service's multiple member-stops to one canonical
                            // row here - AFTER the cancelled/STP-overlay dedup above, so the optimiser only ever
                            // sees one row per (TimetableUid, On) per member. Null for plain CRS searches.
                            if (optimise != null)
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
                if (TryResolveGroupOrStation(request.ComingFromGoingTo, out var station, out var group))
                {
                    filter = group != null ? CreateFilter(group.Members) : CreateFilter(station);
                    queryGroup = group;
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