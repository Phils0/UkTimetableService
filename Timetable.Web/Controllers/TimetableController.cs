using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetable _timetable;
        private readonly IMapper _mapper;

        public TimetableController(ITimetable timetable, IMapper mapper)
        {
            _timetable = timetable;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a scheduled service on a particular day
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [Route("service/{serviceId}/{date}")]
        [HttpGet]
        public async Task<IActionResult> GetService(string serviceId, DateTime date)
        {
            var service =  _timetable.GetSchedule(serviceId, date);
            if (service.schedule == null)
                return NotFound( 
                    new ServiceNotFound()
                    {
                        TimetableUid = serviceId,
                        Date = date,
                        Reason = service.reason
                    });
       
            var model = _mapper.Map<Timetable.Schedule, Model.Service>(service.schedule, o => { o.Items["On"] = date; });
            return Ok(model);
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
        /// Returns arrivals at a location, optionally from another location
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
    }
}