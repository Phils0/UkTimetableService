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
        /// <param name="useRailDay">Only used when fullDay=true.  If true day is calculated as 02:30 to 02:30 next day.  False uses calendar day</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
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
            [FromQuery] ushort before = 1, [FromQuery] ushort after = 5, [FromQuery] bool fullDay = false, [FromQuery] bool useRailDay = false, [FromQuery] bool includeStops = false, [FromQuery] string[] toc = null)
        {
            return await Departures(location, DateTime.Now, to, before, after, fullDay, useRailDay, includeStops, toc);
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
        /// <param name="useRailDay">Only used when fullDay=true.  If true day is calculated as 02:30 to 02:30 next day.  False uses calendar day</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="toc">Only services from included TOCs included.  Can add multiple to querystring, then any service ran by any of them returned</param>
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
            [FromQuery] ushort before = 1, [FromQuery] ushort after = 5, [FromQuery] bool fullDay = false, [FromQuery] bool useRailDay = false, [FromQuery] bool includeStops = false, [FromQuery] string[] toc = null)
        {
            if (fullDay)
                return await FullDayDepartures(location, at.Date, to, includeStops, toc, useRailDay);
            
            var request = CreateRequest(location, at, to, before, after, SearchRequest.DEPARTURES, toc);
            return await Process(request, async () =>
            {
                var config = CreateGatherConfig(request);
                var result = _timetable.FindDepartures(request.Location, at, config);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        private async Task<IActionResult> FullDayDepartures(string location, DateTime onDate, string to, bool includeStops, string[] tocs, bool useRailDay)
        {
            var request = CreateFullDayRequest(location, onDate, to, SearchRequest.DEPARTURES, tocs, useRailDay);
            return await Process(request, async () =>
            {
                var filter = CreateFilter(request);
                var result = _timetable.AllDepartures(request.Location, onDate, filter, useRailDay);
                return await Task.FromResult(result);
            }, includeStops);
        }
        
        protected override GatherConfiguration.GatherFilter CreateFilter(Station station)
        {
            return _filters.DeparturesGoTo(station);
        }
    }
}