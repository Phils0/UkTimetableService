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
    public class ArrivalsController : ControllerBase
    {
        private readonly ILocationData _timetable;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        
        public ArrivalsController(ILocationData data,  IMapper mapper, ILogger logger)
        {
            _timetable = data;
            _mapper = mapper;
            _logger = logger;
        }
        
        /// <summary>
        /// Returns arrivals at a location, optionally from another location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        [Route("arrivals/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, DateTime? at, [FromQuery] string from = "", [FromQuery] ushort before = 3, [FromQuery] ushort after = 3)
        {
            var request = new SearchRequest()
            {
                Location = location,
                At = new Window()
                {
                    At = at ?? DateTime.Now,
                    Before = before,
                    After = after
                },
                ComingFromGoingTo = from
            };
            
            var response = new Model.FoundResponse()
            {
                Request = request,
                GeneratedAt = DateTime.Now
            };
            
            return Ok(response);
        }
    }
}