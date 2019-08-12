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
    public class DeparturesController : ArrivalDeparturesControllerBase
    {
        public DeparturesController(ILocationData data,  IFilterFactory filters, IMapper mapper, ILogger logger) :
            base(data, filters, mapper, logger)
        {
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
            var request = CreateRequest(location, at, to, before, after, SearchRequest.DEPARTURES);
            return Process(request);
        }
        
        protected override (FindStatus status, ResolvedServiceStop[] services) FindServices(string location, DateTime at, GatherConfiguration config)
        {
            return _timetable.FindDepartures(location, at, config);
        }
        
        protected override GatherFilterFactory.GatherFilter CreateFilter(Station station)
        {
            return _filters.DeparturesGoTo(station);
        }
    }
}