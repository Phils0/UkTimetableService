using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Timetable.Web.Model;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Service Configuration controller
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class ServiceConfigurationController : ControllerBase
    {
        private readonly Configuration _configuration;

        public ServiceConfigurationController(Model.Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Service configuration
        /// </summary>
        /// <returns>App and data versions</returns>
        /// <response code="200">Ok</response>
        [ProducesResponseType(200, Type = typeof(Model.Configuration))]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> LocationAsync()
        {
            return await Task.FromResult(Ok(_configuration));
        }
    }
}