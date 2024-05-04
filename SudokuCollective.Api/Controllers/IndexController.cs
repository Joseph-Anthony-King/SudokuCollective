using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SudokuCollective.Api.Controllers
{
    /// <summary>
    /// Index Controller Class
    /// </summary>
    /// <remarks>
    /// Index Controller Constructor
    /// </remarks>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController(IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
    {
        private IWebHostEnvironment _environment = environment;
        private IConfiguration _configuration { get; } = configuration;

        /// <summary>
        /// An endpoint to obtain and populate the Sudoku Collective mission statement on the index home page.
        /// </summary>
        /// <returns>The mission statement used for the API home landing page.</returns>
        /// <remarks>Returns the mission statement used for the API home landing page.</remarks>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            var missionStatement = _environment.IsDevelopment() ? 
                _configuration.GetSection("MissionStatement").Value : 
                Environment.GetEnvironmentVariable("MISSIONSTATEMENT");

            return Ok(new { missionStatement });
        }
    }
}
