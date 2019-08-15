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
    public class ArrivalsController : ArrivalDeparturesControllerBase
    {
        public ArrivalsController(ILocationData data, IFilterFactory filters, IMapper mapper, ILogger logger) :
            base(data, filters, mapper, logger)
        {
        }

        /// <summary>
        /// Returns arrivals at a location, optionally from another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="from">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <returns></returns>
        [Route("arrivals/{location}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, [FromQuery] string from = "",
            [FromQuery] ushort before = 3, [FromQuery] ushort after = 3)
        {
            return await Arrivals(location, DateTime.Now, from, before, after);
        }

        /// <summary>
        /// Returns arrivals at a location, optionally from another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="at">Datetime</param>
        /// <param name="from">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <returns></returns>
        [Route("arrivals/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, DateTime at, [FromQuery] string from = "",
            [FromQuery] ushort before = 3, [FromQuery] ushort after = 3)
        {
            var request = CreateRequest(location, at, from, before, after, SearchRequest.ARRIVALS);
            return await Process(request, async () =>
            {
                var config = CreateGatherConfig( before, after, request.ComingFromGoingTo);
                var result =  _timetable.FindArrivals(request.Location, at, config);
                return await Task.FromResult(result);
            });
        }
        
        /// <summary>
        /// Returns arrivals at a location for the whole day
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="onDate">Datetime</param>
        /// <param name="from">Optional from location filter </param>
        /// <returns>A list of arriving services</returns>
        [Route("arrivals/{location}/day/{onDate}")]
        [HttpGet]
        public async Task<IActionResult> FullDayArrivals(string location, DateTime onDate, [FromQuery] string from = "")
        {
            var request = CreateFullDayRequest(location, onDate, @from, SearchRequest.ARRIVALS);
            return await Process(request, async () =>
            {
                var filter = CreateFilter(request.ComingFromGoingTo);
                var result = _timetable.AllArrivals(request.Location, onDate, filter);
                return await Task.FromResult(result);
            });
        }
        
        protected override GatherConfiguration.GatherFilter CreateFilter(Station station)
        {
            return _filters.ArrivalsComeFrom(station);
        }
    }
}