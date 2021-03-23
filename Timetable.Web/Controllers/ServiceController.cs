using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Service controller
    /// </summary>
    [Route("api/timetable/")]
    [ApiController]
    [Produces("application/json")]
    public class ServiceController : ControllerBase
    {
        private readonly ITimetableLookup _timetable;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ServiceController(ITimetableLookup timetable, IMapper mapper, ILogger logger)
        {
            _timetable = timetable;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Returns a scheduled service on a particular day
        /// </summary>
        /// <param name="serviceId">Timetable UID - letter plus 5 digits</param>
        /// <param name="on">Date</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Service))]
        [ProducesResponseType(404, Type = typeof(Model.ServiceNotFound))]
        [Route("service/{serviceId}/{on}")]
        [HttpGet]
        public async Task<IActionResult> GetServiceByTimetableId(string serviceId, DateTime @on)
        {
            try
            {
                var service = _timetable.GetScheduleByTimetableUid(serviceId, @on);
                if (service.status == LookupStatus.Success)
                {
                    var model = _mapper.Map<Timetable.ResolvedService, Model.Service>(service.service, InitialiseContext);
                    return await Task.FromResult(Ok(model));
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, serviceId, @on));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {serviceId} on {on}", serviceId, on.ToYMD());
                return CreateNoServiceResponse(LookupStatus.Error, serviceId, @on);
            }
        }

        public static void InitialiseContext(IMappingOperationOptions opts)
        {
            opts.Items["Dummy"] = "";
        }

        private ObjectResult CreateNoServiceResponse(LookupStatus serviceStatus, string id, DateTime date, string searchType = "Service")
        {
            var reason = "";
            switch (serviceStatus)
            {
                case LookupStatus.ServiceNotFound:
                    reason = $"{id} not found in timetable";
                    return NotFound(CreateServiceNotFound());
                case LookupStatus.NoScheduleOnDate:
                    reason = $"{id} does not run on {date.ToYMD()}";
                    return NotFound(CreateServiceNotFound());
                case LookupStatus.InvalidRetailServiceId:
                    reason = $"{searchType} {id} is invalid";
                    return BadRequest(CreateServiceNotFound());
                case LookupStatus.Error:
                    reason = $"Error looking for {searchType} {id} on {date.ToYMD()}";
                    return StatusCode(StatusCodes.Status500InternalServerError, CreateServiceNotFound());
                default:
                    reason = $"Unknown reason why could not find {searchType} {id} on {date.ToYMD()}";
                    return StatusCode(StatusCodes.Status500InternalServerError, CreateServiceNotFound());
            }

            ServiceNotFound CreateServiceNotFound()
            {
                return new ServiceNotFound()
                {
                    Id = id,
                    Date = date,
                    Reason = reason
                };
            }
        }

        /// <summary>
        /// Returns a scheduled service on a particular day
        /// </summary>
        /// <param name="serviceId">Retail Service Id, two letters plus 6 or 4 digits</param>
        /// <param name="on">date</param>
        /// <returns>Set of services</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Service[]))]
        [ProducesResponseType(404, Type = typeof(Model.ServiceNotFound))]
        [Route("retailService/{serviceId}/{on}")]
        [HttpGet]
        public async Task<IActionResult> GetServiceByRetailServiceId(string serviceId, DateTime @on)
        {
            try
            {
                var service = _timetable.GetScheduleByRetailServiceId(serviceId, @on);
                if (service.status == LookupStatus.Success)
                {
                    var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services, InitialiseContext);
                    return await Task.FromResult(Ok(model));
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, serviceId, @on));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {serviceId} on {on}", serviceId, on.ToYMD());
                return CreateNoServiceResponse(LookupStatus.Error, serviceId, @on);
            }
        }

        /// <summary>
        /// Returns all Toc services on a particular day
        /// </summary>
        /// <param name="toc">Two letter TOC code</param>
        /// <param name="on">date</param>
        /// <param name="dayBoundary">Time to start a day, use 24hr clock, format HH:mm.  The rail day is generally considered to start at 02:30  Default uses calendar day i.e. boundary is midnight</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <param name="returnCancelledServices">Whether to return cancelled scheduled services</param>
        /// <returns>Set of services</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Service[]))]
        [ProducesResponseType(200, Type = typeof(Model.ServiceSummary[]))]
        [ProducesResponseType(404, Type = typeof(Model.ServiceNotFound))]
        [Route("toc/{toc}/{on}")]
        [HttpGet]
        public async Task<IActionResult> GetTocServices(string toc, DateTime @on, [FromQuery] string dayBoundary = "00:00", [FromQuery] bool includeStops = false, [FromQuery] bool returnCancelledServices = false)
        {
            try
            {
                var boundary = Time.Parse(dayBoundary);
                var service = _timetable.CreateFilter().GetServicesByToc(returnCancelledServices)(toc, @on, boundary);
                if (service.status == LookupStatus.Success)
                {
                    if (includeStops)
                    {
                        var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services, InitialiseContext);
                        return await Task.FromResult(Ok(model));
                    }
                    else
                    {
                        var model = _mapper.Map<Timetable.ResolvedService[], Model.ServiceSummary[]>(service.services, InitialiseContext);
                        return await Task.FromResult(Ok(model));
                    }
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, toc, @on, "Toc"));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {toc} on {on}", toc, on.ToYMD());
                return CreateNoServiceResponse(LookupStatus.Error, toc, @on, "Toc");
            }
        }
    }
}