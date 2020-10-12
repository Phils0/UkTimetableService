using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Reference data controller
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceController : ControllerBase
    {
        private readonly ILocationData _data;
        private readonly ITocLookup _tocs;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ReferenceController(ILocationData data, ITocLookup tocs, IMapper mapper, ILogger logger)
        {
            _data = data;
            _tocs = tocs;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Returns UK rail locations that you can use in this API
        /// </summary>
        /// <param name="toc">Only locations with services from included TOCs included.  Can add multiple to querystring, then any location with any service ran by any of them is returned</param>
        /// <returns>Set of locations</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Station[])), 
         ProducesResponseType(400),
         ProducesResponseType(404), 
         ProducesResponseType(500), 
         Route("location"), HttpGet]
        public async Task<IActionResult> LocationAsync([FromQuery] string[] toc = null)
        {
            try
            {
                var tocFilter = new TocFilter(toc);
                if (tocFilter.HasInvalidTocs)
                {
                    _logger.Information("Invalid tocs provided in request {t}", tocFilter.Tocs); 
                    return await Task.FromResult(
                        BadRequest(new ReferenceError($"Invalid tocs provided in request {tocFilter.Tocs}")));                   
                }

                var locations = tocFilter.NoFilter
                    ? _data.Locations.Values
                    : _data.Locations.Values.Where(s => s.TocServices.Any(t => tocFilter.IsValid(t)));
                var model = _mapper.Map<IEnumerable<Timetable.Station>, Model.Station[]>(locations);
                if (model.Any())
                    return await Task.FromResult(Ok(model));

                return await Task.FromResult(
                    NotFound(new ReferenceError($"No stations found.  Toc filter {tocFilter.Tocs}")));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting locations");
                return await Task.FromResult(StatusCode(500, new ReferenceError("Server error")));
            }

        }
        
        
                /// <summary>
        /// Returns UK tos that you can use in this API
        /// </summary>
        /// <returns>Set of locations</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Station[])), 
         ProducesResponseType(400),
         ProducesResponseType(404), 
         ProducesResponseType(500), 
         Route("toc"), HttpGet]
        public async Task<IActionResult> TocsAsync()
        {
            try
            {
                var model = _mapper.Map<IEnumerable<Timetable.Toc>, Model.Toc[]>(_tocs);
                if (model.Any())
                    return await Task.FromResult(Ok(model));

                return await Task.FromResult(
                    NotFound(new ReferenceError("No tocs found.")));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting tocs");
                return await Task.FromResult(StatusCode(500, new ReferenceError("Server error")));
            }

        }
    }
}