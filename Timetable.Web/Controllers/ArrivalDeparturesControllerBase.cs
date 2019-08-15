using System;
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

        protected abstract GatherFilterFactory.GatherFilter CreateFilter(Station station);
        
        protected SearchRequest CreateRequest(string location, DateTime at, string toFrom, ushort before, ushort after, string requestType)
        {
            return new SearchRequest()
            {
                Location = location,
                At = new Window()
                {
                    At = at,
                    Before = before,
                    After = after
                },
                ComingFromGoingTo = toFrom,
                Type = requestType
            };
        }

        protected SearchRequest CreateFullDayRequest(string location, DateTime at, string toFrom, string requestType)
        {
            return new SearchRequest()
            {
                Location = location,
                At = new Window()
                {
                    At = at,
                    FullDay = true
                },
                ComingFromGoingTo = toFrom,
                Type = requestType
            };
        }
        
        protected IActionResult Process(SearchRequest request, Func<(FindStatus status, ResolvedServiceStop[] services)> find)
        {
            using (LogContext.PushProperty("Request", request, true))
            {
                FindStatus status;
                try
                {
                    var (findStatus, services) = find();

                    if (findStatus == FindStatus.Success)
                    {
                        var items = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundItem[]>(services);
                        return Ok(new Model.FoundResponse()
                        {
                            Request = request,
                            GeneratedAt = DateTime.Now,
                            Services = items
                        });
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
        
        protected GatherConfiguration CreateGatherConfig(ushort before, ushort after, string toFrom)
        {
            var filter = CreateFilter(toFrom);
            return new GatherConfiguration(before, after, filter);
        }
        
        protected GatherFilterFactory.GatherFilter CreateFilter(string toFrom)
        {
            var filter = _filters.NoFilter;
            if(string.IsNullOrEmpty(toFrom))
                return _filters.NoFilter;
            
            if (_timetable.TryGetLocation(toFrom, out Station station))
                filter = CreateFilter(station);
            else
                _logger.Warning("Not adding filter.  Did not find location {toFrom}", toFrom);
            
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