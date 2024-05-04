using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Licenses Controller Class
    /// </summary>
    /// <remarks>
    /// Licenses Controller Constructor
    /// </remarks>
    /// <param name="appsService"></param>
    /// <param name="requestService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    /// <param name="environment"></param>
    [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
    [Route("api/[controller]")]
    [ApiController]
    public class LicensesController(
        IAppsService appsService,
        IRequestService requestService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LicensesController> logger,
        IWebHostEnvironment environment) : ControllerBase
    {
        private readonly IAppsService _appsService = appsService;
        private readonly IRequestService _requestService = requestService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<LicensesController> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;

        /// <summary>
        /// An endpoint to create app licenses, requires superuser or admin roles.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An new app.</returns>
        /// <response code="201">Returns a result object with the new app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating validation erros with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="500">Returns a result object with the message stating any errors creating the app.</response>
        /// <remarks>
        /// The Post endpoint requires the user to be logged in. Requires superuser or admin roles. The request body parameter uses the request model.
        /// 
        /// The payload should be a LicensePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "name": string,        // name is required, represents the apps name
        ///         "ownerId": integer     // ownerId is required, represents the signed in users id
        ///         "localUrl": string,    // localUrl is not required, an example is https://localhost:8081; regex documented in app schema below
        ///         "stagingUrl": string,  // stagingUrl is not required, an exampled is https://example-app.herokuapp.com; regex documented in app schema below
        ///         "qaUrl": string,       // qaUrl is not required, an exampled is https://example-qa.herokuapp.com; regex documented in app schema below
        ///         "prodUrl": string,     // prodUrl is not required, an exampled is https://example-app.com; regex documented in app schema below
        ///       },
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost]
        public async Task<ActionResult<Result>> PostAsync([FromBody] Request request)
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
                    var result = await _appsService.CreateAync(request);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode201(result.Message);

                        return StatusCode((int)HttpStatusCode.Created, result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                        result.Message = ControllerMessages.StatusCode400(result.Message);

                        return BadRequest(result);
                    }
                }
                else
                {
                    return ControllerUtilities.ProcessTokenError(this);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<LicensesController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to obtain an apps license, requires superuser or admin roles.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A license for a given app.</returns>
        /// <response code="200">Returns a result object with the app license included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating validation erros with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors finding the app license.</response>
        /// <remarks>
        /// The Get endpoint requires the user to be logged in. Requires superuser or admin roles. The query parameter id refers to the relevant app. 
        /// The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpGet, Route("{id}")]
        public async Task<ActionResult<Result>> GetAsync(
            int id,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.GetLicenseAsync(id, request.RequestorId);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<LicensesController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }
    }
}
