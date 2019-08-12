using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Departures controller
    /// </summary>
    [Route("api/timetable")]
    [ApiController]
    public class DeparturesController : ControllerBase
    {
        private readonly ILocationData _timetable;
        private readonly IFilterFactory _filters;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        
        public DeparturesController(ILocationData data,  IFilterFactory filters, IMapper mapper, ILogger logger)
        {
            _timetable = data;
            _filters = filters;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Returns departures at a location now, optionally to another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="to">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <returns></returns>
        [Route("departures/{location}")]
        [HttpGet]
        public async Task<IActionResult> Departures(string location, [FromQuery] string to = "", [FromQuery] ushort before = 1, [FromQuery] ushort after = 5)
        {
            return await Departures(location, DateTime.Now, to, before, after);
        }
        
        /// <summary>
        /// Returns departures at a location, optionally to another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="at">Datetime</param>
        /// <param name="to">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <returns></returns>
        [Route("departures/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Departures(string location, DateTime at, [FromQuery] string to = "", [FromQuery] ushort before = 1, [FromQuery] ushort after = 5)
        {
            var request = CreateRequest(location, at, to, before, after);
            FindStatus status;

            using (LogContext.PushProperty("Request", request, true))
            {
                try
                {
                    var config = CreateGatherConfig(before, after, to);
                    var (findStatus, services) = _timetable.FindDepartures(location, at, config);
                
                    if (findStatus == FindStatus.Success)
                    {
                        var departures = _mapper.Map<Timetable.ResolvedServiceStop[], Model.FoundItem[]>(services);
                        return Ok(new Model.FoundResponse()
                        {
                            Request = request,
                            GeneratedAt = DateTime.Now,
                            Services = departures
                        });               
                    }  
                                                                             
                    status = findStatus;
                }
                catch (Exception e)
                {
                    status = FindStatus.Error;
                    _logger.Error(e, "Error when processing : {@request}", request);
                }
                                                                         
                return CreateNoServiceResponse(status, request);;               
            }
        }

        private GatherConfiguration CreateGatherConfig(ushort before, ushort after, string to)
        {
            var filter = _filters.NoFilter;
            if (_timetable.TryGetLocation(to, out Station destination))
                filter = _filters.DeparturesGoTo(destination);
            else
                _logger.Warning("Not adding departure filter.  Did not find to location {to}", to);
            return new GatherConfiguration(before, after, filter);
        }

        private SearchRequest CreateRequest(string location, DateTime at, string to, ushort before, ushort after)
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
                ComingFromGoingTo = to,
                Type = SearchRequest.DEPARTURES
            };
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