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
    [Route("api/[controller]/")]
    [ApiController]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetable _timetable;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        
        public TimetableController(ITimetable timetable, IMapper mapper, ILogger logger)
        {
            _timetable = timetable;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Returns a scheduled service on a particular day
        /// </summary>
        /// <param name="serviceId">Timetable Id</param>
        /// <param name="date"></param>
        /// <returns></returns>
        [Route("service/{serviceId}/{date}")]
        [HttpGet]
        public async Task<IActionResult> GetServiceByTimetableId(string serviceId, DateTime date)
        {
            var service =  _timetable.GetScheduleByTimetableUid(serviceId, date);
            if (service.status == LookupStatus.Success)
            {
                var model = _mapper.Map<Timetable.ResolvedService, Model.Service>(service.service);
                return Ok(model);             
            }

            return CreateNoServiceResponse(service.status, serviceId, date);
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
            return  NotFound(new ServiceNotFound()
            {
                Id = serviceId,
                Date = date,
                Reason = reason
            });
        }
        
        /// <summary>
        /// Returns a scheduled service on a particular day
        /// </summary>
        /// <param name="serviceId">Retail Service Id</param>
        /// <param name="date"></param>
        /// <returns></returns>
        [Route("retailService/{serviceId}/{date}")]
        [HttpGet]
        public async Task<IActionResult> GetServiceByRetailServiceId(string serviceId, DateTime date)
        {
            var service =  _timetable.GetScheduleByRetailServiceId(serviceId, date);
            if (service.status == LookupStatus.Success)
            {
                 var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services);
                 return Ok(model);               
            }    

            return CreateNoServiceResponse(service.status, serviceId, date);
        }
        
        /// <summary>
        /// Returns arrivals at a location, optionally from another location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        [Route("arrivals/{location}/{at}")]
        [HttpGet]
        public async Task<IActionResult> Arrivals(string location, DateTime? at, [FromQuery] string from = "", [FromQuery] ushort before = 5, [FromQuery] ushort after = 1)
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
                ComingFromGoingTo = from
            };
            
            var response = new Model.LocationTimetable()
            {
                Request = request,
                GeneratedAt = DateTime.Now
            };
            
            return Ok(response);
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
        
        /// <summary>
        /// Returns all Toc services on a particular day
        /// </summary>
        /// <param name="toc">Timetable Id</param>
        /// <param name="date"></param>
        /// <param name="includeStops">Whether to return a full schedule</param>
        /// <returns></returns>
        [Route("toc/{toc}/{date}")]
        [HttpGet]
        public async Task<IActionResult> GetTocServices(string toc, DateTime date, [FromQuery] bool includeStops = false)
        {
            var service =  _timetable.GetSchedulesByToc(toc, date);
            if (service.status == LookupStatus.Success)
            {
                if (includeStops)
                {
                    var model = _mapper.Map<Timetable.ResolvedService[], Model.Service[]>(service.services);
                    return Ok(model);                               
                }
                else
                {
                    var model = _mapper.Map<Timetable.ResolvedService[], Model.ServiceSummary[]>(service.services);
                    return Ok(model);                               
                }
            }
            return CreateNoServiceResponse(service.status, toc, date);
        }
    }
}