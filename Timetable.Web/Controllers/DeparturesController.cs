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
    [Produces("application/json")]
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
        /// <param name="fullDay">Return full day of departures.  fullDay=true and before\after are mutually exclusive.  If both provided fullDay will take precedence</param>
        /// <param name="dayBoundary">Time to start a day, use 24hr clock, format HH:mm.  The rail day is generally considered to start at 02:30  Default uses calendar day i.e. boundary is midnight</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
        /// <param name="returnCancelledServices">Whether to return cancelled services</param>
        /// <returns>A list of departing services</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.FoundSummaryItem))]
        [ProducesResponseType(200, Type = typeof(Model.FoundServiceResponse))]
        [ProducesResponseType(404, Type = typeof(Model.NotFoundResponse))]
        [ProducesResponseType(500, Type = typeof(Model.NotFoundResponse))]
        [Route("departures/{location}")]
        [HttpGet]
        public async Task<IActionResult> Departures(string location, [FromQuery] string to = "", 
            [FromQuery] ushort before = 1, [FromQuery] ushort after = 5, 
            [FromQuery] bool fullDay = false, [FromQuery] string dayBoundary = "00:00", [FromQuery] bool includeStops = false, 
            [FromQuery] string[] toc = null, [FromQuery] bool returnCancelledServices = false)
        {
            return await Departures(location, DateTime.Now, to, before, after, fullDay, dayBoundary, includeStops, toc, returnCancelledServices);
        }

        /// <summary>
        /// Returns departures at a location, optionally to another location
        /// </summary>
        /// <param name="location">Three letter code</param>
        /// <param name="at">Datetime</param>
        /// <param name="to">Optional to location filter </param>
        /// <param name="before">Number of services to return that depart before the time</param>
        /// <param name="after">Number of services to return that depart after the time, includes any at the specific time</param>
        /// <param name="fullDay">Return full day of departures.  fullDay=true and before\after are mutually exclusive.  If both provided fullDay will take precedence</param>
        /// <param name="dayBoundary">Time to start a day, use 24hr clock, format HH:mm.  The rail day is generally considered to start at 02:30  Default uses calendar day i.e. boundary is midnight</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
        /// <param name="returnCancelledServices">Whether to return cancelled services</param>
        /// <returns>A list of departing services</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.FoundSummaryItem))]
        [ProducesResponseType(200, Type = typeof(Model.FoundServiceResponse))]
        [ProducesResponseType(404, Type = typeof(Model.NotFoundResponse))]
        [ProducesResponseType(500, Type = typeof(Model.NotFoundResponse))]
        [Route("departures/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Departures(string location, DateTime at, [FromQuery] string to = "", 
            [FromQuery] ushort before = 1, [FromQuery] ushort after = 5, 
            [FromQuery] bool fullDay = false, [FromQuery] string dayBoundary = "00:00", [FromQuery] bool includeStops = false, 
            [FromQuery] string[] toc = null, [FromQuery] bool returnCancelledServices = false)
        {
            var tocFilter = new TocFilter(toc);
            if (fullDay)
                return await FullDayDepartures(location, at.Date, to, includeStops, tocFilter, returnCancelledServices, dayBoundary);
            
            var request = CreateRequest(location, at, to, before, after, SearchRequest.DEPARTURES, tocFilter);
            return await Process(request, tocFilter, async () =>
            {
                var config = CreateGatherConfig(request, tocFilter);
                var result = _timetable.FindDepartures(request.Location, at, config, returnCancelledServices);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        private async Task<IActionResult> FullDayDepartures(string location, DateTime onDate, string to, bool includeStops, TocFilter tocFilter, bool returnCancelledServices, string dayBoundary)
        {
            var request = CreateFullDayRequest(location, onDate, to, SearchRequest.DEPARTURES, tocFilter, dayBoundary);
            return await Process(request, tocFilter, async () =>
            {
                var boundary = Time.Parse(dayBoundary);
                var filter = CreateFilter(request, tocFilter);
                var result = _timetable.AllDepartures(request.Location, onDate, filter, returnCancelledServices, boundary);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        protected override GatherConfiguration.GatherFilter CreateFilter(Station station)
        {
            return _filters.DeparturesGoTo(station);
        }
    }
}