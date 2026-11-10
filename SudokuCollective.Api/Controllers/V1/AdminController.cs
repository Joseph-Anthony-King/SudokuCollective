using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Admin Controller Class
    /// </summary>
    /// <remarks>
    /// Admin Controller Constructor
    /// </remarks>
    /// <param name="adminService"></param>
    /// <param name="appsService"></param>
    /// <param name="requestService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    [Authorize(Roles = "SUPERUSER")]
    [Route("api/v1/[controller]")]
    public partial class AdminController(
        IAdminService adminService,
        IAppsService appsService,
        IRequestService requestService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AdminController> logger) : ControllerBase
    {
        private readonly IAdminService _adminService = adminService;
        private readonly IAppsService _appsService = appsService;
        private readonly IRequestService _requestService = requestService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<AdminController> _logger = logger;

        /// <summary>
        /// An endpoint that returns the app wide admin system information.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns system wide info to super users.</returns>
        /// <response code="200">Returns a result object that includes the system info.</response>
        /// <response code="404">Returns a result object with the message stating the system info was not found.</response>
        [Authorize(Roles = "SUPERUSER")]
        [HttpPost]
        public async  Task<ActionResult<Result>> PostAsync([FromBody] Request request)
        {

            try
            {

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _adminService.GetSystemStatsAndValues(request.RequestorId);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        result.Message = ControllerMessages.StatusCode404(result.Message);

                        return NotFound(result);
                    }
                }
                else
                {
                    return ControllerUtilities.ProcessTokenError(this);
                }
            }
            catch (Exception e)
            {
                return ControllerUtilities.ProcessException(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }
    }
}
