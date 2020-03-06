using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    [Produces("application/json")]
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
        /// <param name="fullDay">Return full day of departures.  fullDay=true and before\after are mutually exclusive.  If both provided fullDay will take precedence</param>
        /// <param name="dayBoundary">Time to start a day, use 24hr clock, format HH:mm.  The rail day is generally considered to start at 02:30  Default uses calendar day i.e. boundary is midnight</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
        /// <returns>Set of arrivals</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.FoundSummaryItem))]
        [ProducesResponseType(200, Type = typeof(Model.FoundServiceResponse))]
        [ProducesResponseType(404, Type = typeof(Model.NotFoundResponse))]
        [ProducesResponseType(500, Type = typeof(Model.NotFoundResponse))]
        [Route("arrivals/{location}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, [FromQuery] string from = "",
            [FromQuery] ushort before = 3, [FromQuery] ushort after = 3, [FromQuery] bool fullDay = false, [FromQuery] string dayBoundary = "00:00", [FromQuery] bool includeStops = false, [FromQuery] string[] toc = null)
        {
            return await Arrivals(location, DateTime.Now, @from, before, after, fullDay, dayBoundary, includeStops, toc);
        }

        /// <summary>
        /// Returns arrivals at a location, optionally from another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="at">Datetime</param>
        /// <param name="from">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <param name="fullDay">Return full day of departures.  fullDay=true and before\after are mutually exclusive.  If both provided fullDay will take precedence</param>
        /// <param name="dayBoundary">Time to start a day, use 24hr clock, format HH:mm.  The rail day is generally considered to start at 02:30  Default uses calendar day i.e. boundary is midnight</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
        /// <returns>Set of arrivals</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.FoundSummaryItem))]
        [ProducesResponseType(200, Type = typeof(Model.FoundServiceResponse))]
        [ProducesResponseType(404, Type = typeof(Model.NotFoundResponse))]
        [ProducesResponseType(500, Type = typeof(Model.NotFoundResponse))]
        [Route("arrivals/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, DateTime at, [FromQuery] string from = "",
            [FromQuery] ushort before = 3, [FromQuery] ushort after = 3, [FromQuery] bool fullDay = false, [FromQuery] string dayBoundary = "00:00", [FromQuery] bool includeStops = false, [FromQuery] string[] toc = null)
        {
            if (fullDay)
                return await FullDayArrivals(location, at.Date, from, includeStops, toc, dayBoundary);
            
            var request = CreateRequest(location, at, from, before, after, SearchRequest.ARRIVALS, toc);
            return await Process(request, async () =>
            {
                var config = CreateGatherConfig(request);
                var result =  _timetable.FindArrivals(request.Location, at, config);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        private async Task<IActionResult> FullDayArrivals(string location, DateTime onDate, string from, bool includeStops, string[] tocs,  string dayBoundary)
        {
            var request = CreateFullDayRequest(location, onDate, @from, SearchRequest.ARRIVALS, tocs, dayBoundary);
            return await Process(request, async () =>
            {
                var boundary = Time.Parse(dayBoundary);
                var filter = CreateFilter(request);
                var result = _timetable.AllArrivals(request.Location, onDate, filter, boundary);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        protected override GatherConfiguration.GatherFilter CreateFilter(Station station)
        {
            return _filters.ArrivalsComeFrom(station);
        }
    }
}