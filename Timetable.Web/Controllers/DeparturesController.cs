using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
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
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        
        public DeparturesController(ILocationData data,  IMapper mapper, ILogger logger)
        {
            _timetable = data;
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
            
            var services = _timetable.FindDepartures(location, at, before, after, to);
            
            if (services.status == FindStatus.Success)
            {
                var model = _mapper.Map<Timetable.ResolvedService[], Model.FoundItem[]>(services.services);
                return Ok(new Model.FoundResponse()
                {
                    Request = request,
                    GeneratedAt = DateTime.Now,
                    Services = model
                });               
            }  
            
            return CreateNoServiceResponse(services.status, request);;
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
                default:
                    reason = $"Unknown reason why could not find anything";
                    _logger.Error("{reason} : {@request}", reason, request);
                    break;
            }
            
            //Return 404
            return  NotFound(new NotFoundResponse()
            {
                Request = request,
                GeneratedAt = DateTime.Now,
                Reason = reason
            });
        }
    }
}