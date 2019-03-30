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
        private readonly IReference _service;
        private readonly IMapper _mapper;

        public ReferenceController(IReference service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [Route("location")]
        [HttpGet]
        public async Task<IActionResult> LocationAsync()
        {
            var locations = await _service.GetLocationsAsync(CancellationToken.None).
                ConfigureAwait(false);
            var model = _mapper.Map<IEnumerable<Timetable.Station>, Model.Station[]>(locations);
            return Ok(model);
        }
    }
}