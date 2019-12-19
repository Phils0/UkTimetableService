using System;
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
        protected ILocationData _timetable;
        protected IFilterFactory _filters;
        protected IMapper _mapper;
        protected ILogger _logger;

        protected ArrivalDeparturesControllerBase(ILocationData data,  IFilterFactory filters, IMapper mapper, ILogger logger)
        {
            _timetable = data;
            _filters = filters;
            _mapper = mapper;
            _logger = logger;
        }

        protected abstract GatherConfiguration.GatherFilter CreateFilter(Station station);
        
        protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after, string requestType, string[] tocs)
        {
            return CreateRequest(location, at, toFrom, before, after, requestType, tocs, false, false);
        }

        protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after, string requestType, string[] tocs, bool fullDay,  bool useRailDay)
        {
            var request = new SearchRequest()
            {
                Location = location,
                At = new Window()
                {
                    At = at,
                    Before = before,
                    After = after,
                    FullDay = fullDay
                },
                ComingFromGoingTo = toFrom,
                Type = requestType
            };
            request.SetTocs(tocs);
            return request;
        }
        
        protected SearchRequest CreateFullDayRequest(string location, DateTime at, string toFrom, string requestType, string[] tocs,  bool useRailDay)
        {
            return CreateRequest(location, at, toFrom, 0, 0, requestType, tocs, true, useRailDay);
        }
        
        protected async Task<IActionResult> Process(SearchRequest request, Func<Task<(FindStatus status, ResolvedServiceStop[] services)>> find, bool includeStops)
        {
            using (LogContext.PushProperty("Request", request, true))
            {
                FindStatus status;
                try
                {
                    var (findStatus, services) = await find();

                    if (findStatus == FindStatus.Success)
                    {
                        if (includeStops)
                        {
                            var items = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundServiceItem[]>(services, ServiceController.InitialiseContext);
                            return Ok(new Model.FoundServiceResponse()
                            {
                                Request = request,
                                GeneratedAt = DateTime.Now,
                                Services = items
                            });                           
                        }
                        else
                        {
                            var items = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundSummaryItem[]>(services, ServiceController.InitialiseContext);
                            return Ok(new Model.FoundSummaryResponse()
                            {
                                Request = request,
                                GeneratedAt = DateTime.Now,
                                Services = items
                            });
                        }
                    }

                    status = findStatus;
                }
                catch (Exception e)
                {
                    status = FindStatus.Error;
                    _logger.Error(e, "Error when processing : {@request}", request);
                }

                return CreateNoServiceResponse(status, request);
            }
        }
        
        protected GatherConfiguration CreateGatherConfig(SearchRequest request)
        {
            var filter = CreateFilter(request);
            return new GatherConfiguration(request.At.Before, request.At.After, false, filter);
        }
        
        protected GatherConfiguration.GatherFilter CreateFilter(SearchRequest request)
        {
            GatherConfiguration.GatherFilter filter = _filters.NoFilter;
            if (!string.IsNullOrEmpty(request.ComingFromGoingTo))
            {
                if (_timetable.TryGetLocation(request.ComingFromGoingTo, out Station station))
                    filter = CreateFilter(station);
                else
                    _logger.Warning("Not adding filter.  Did not find location {toFrom}", request.ComingFromGoingTo);                
            }
            
            if (!string.IsNullOrEmpty(request.TocFilter))
                filter = _filters.ProvidedByToc(request.TocFilter, filter);
            
            return filter;
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