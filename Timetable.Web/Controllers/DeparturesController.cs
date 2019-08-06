using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
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
        /// Returns departures at a location, optionally to another location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        [Route("departures/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Departures(string location, DateTime? at, [FromQuery] string to = "", [FromQuery] ushort before = 1, [FromQuery] ushort after = 5)
        {
            var request = new LocationTimetableRequest()
            {
                Location = location,
                At = new Window()
                {
                    At = at ?? DateTime.Now,
                    Before = before,
                    After = after
                },
                ComingFromGoingTo = to
            };
            
            var response = new Model.LocationTimetable()
            {
                Request = request,
                GeneratedAt = DateTime.Now
            };
            
            return Ok(response);
        }
    }
}