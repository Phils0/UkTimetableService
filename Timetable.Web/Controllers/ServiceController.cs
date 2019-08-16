﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly ITimetable _timetable;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ServiceController(ITimetable timetable, IMapper mapper, ILogger logger)
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
                    var onDate = service.service.On;
                    var model = _mapper.Map<Timetable.ResolvedService, Model.Service>(service.service,
                        opts => opts.Items["On"] = onDate);
                    return await Task.FromResult(Ok(model));
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, serviceId, @on));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {serviceId} on {on:d}", serviceId, on);
                throw;
            }
        }

        private ObjectResult CreateNoServiceResponse(LookupStatus serviceStatus, string serviceId, DateTime date)
        {
            var reason = "";
            switch (serviceStatus)
            {
                case LookupStatus.ServiceNotFound:
                    reason = $"{serviceId} not found in timetable";
                    break;
                case LookupStatus.NoScheduleOnDate:
                    reason = $"{serviceId} does not run on {date:d}";
                    break;
                default:
                    reason = $"Unknown reason why could not find {serviceId} on {date:d}";
                    _logger.Error(reason);
                    break;
            }

            //Return 404
            return NotFound(new ServiceNotFound()
            {
                Id = serviceId,
                Date = date,
                Reason = reason
            });
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
                    var onDate = service.services.First().On;
                    var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services,
                        opts => opts.Items["On"] = onDate);
                    return await Task.FromResult(Ok(model));
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, serviceId, @on));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {serviceId} on {on:d}", serviceId, on);
                throw;
            }
        }

        /// <summary>
        /// Returns all Toc services on a particular day
        /// </summary>
        /// <param name="toc">Two letter TOC code</param>
        /// <param name="on">date</param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <returns>Set of services</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(200, Type = typeof(Model.Service[]))]
        [ProducesResponseType(200, Type = typeof(Model.ServiceSummary[]))]
        [ProducesResponseType(404, Type = typeof(Model.ServiceNotFound))]
        [Route("toc/{toc}/{on}")]
        [HttpGet]
        public async Task<IActionResult> GetTocServices(string toc, DateTime @on, [FromQuery] bool includeStops = false)
        {
            try
            {
                var service = _timetable.GetSchedulesByToc(toc, @on);
                if (service.status == LookupStatus.Success)
                {
                    var onDate = service.services.First().On;
                    if (includeStops)
                    {
                        var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services,
                            opts => opts.Items["On"] = onDate);
                        return await Task.FromResult(Ok(model));
                    }
                    else
                    {
                        var model = _mapper.Map<Timetable.ResolvedService[], Model.ServiceSummary[]>(service.services,
                            opts => opts.Items["On"] = onDate);
                        return await Task.FromResult(Ok(model));
                    }
                }

                return await Task.FromResult(CreateNoServiceResponse(service.status, toc, @on));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when processing : {toc} on {on:d}", toc, on);
                throw;
            }
        }
    }
}