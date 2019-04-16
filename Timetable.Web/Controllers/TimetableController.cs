using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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

        [Route("{service}/{date}")]
        [HttpGet]
        public async Task<IActionResult> GetService(string service, DateTime date)
        {
            var schedule =  _timetable.GetSchedule(service, date);
            if (schedule == null)
                return NotFound((service, date));
            
            var model = _mapper.Map<Timetable.Schedule, Model.Service>(schedule, o => { o.Items["On"] = date; });
            return Ok(model);
        }
    }
}