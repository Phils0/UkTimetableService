using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IMapper _mapper;

        public ReferenceController(ILocationData data, IMapper mapper)
        {
            _data = data;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns UK rail locations that you can use in this API
        /// </summary>
        /// <returns>Set of locations</returns>
        /// <response code="200">Ok</response>
        [ProducesResponseType(200, Type = typeof(Model.Station[]))]
        [Route("location")]
        [HttpGet]
        public async Task<IActionResult> LocationAsync()
        {
            var model = _mapper.Map<IEnumerable<Timetable.Station>, Model.Station[]>(_data.Locations.Values);
            return await Task.FromResult(Ok(model));
        }
    }
}