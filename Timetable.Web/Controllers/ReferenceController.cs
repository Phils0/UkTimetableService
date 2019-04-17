using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Timetable.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceController : ControllerBase
    {
        private readonly ILocationData _data;
        private readonly IMapper _mapper;

        public ReferenceController(ILocationData data, IMapper mapper)
        {
            _data = data;
            _mapper = mapper;
        }

        [Route("location")]
        [HttpGet]
        public async Task<IActionResult> LocationAsync()
        {
            var model = _mapper.Map<IEnumerable<Timetable.Station>, Model.Station[]>(_data.Locations.Values);
            return Ok(model);
        }
    }
}