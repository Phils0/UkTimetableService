using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Timetable.Web.Controllers
{
    /// <summary>
    /// Service Configuration controller
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class ServiceConfigurationController : ControllerBase
    {
        private readonly Model.Configuration _configuration;
        private readonly ILogger _logger;

        public ServiceConfigurationController(Model.Configuration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Service configuration
        /// </summary>
        /// <returns>App and data versions</returns>
        /// <response code="200">Ok</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(200, Type = typeof(Model.Configuration))]
        [ProducesResponseType(500)]
        [Route("api/[controller]")]
        [HttpGet]
        public async Task<IActionResult> LocationAsync()
        {
            try
            {
                return await Task.FromResult(Ok(_configuration));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error getting service configuration");
                return await Task.FromResult(StatusCode(500));;
            }
            
        }
    }
}