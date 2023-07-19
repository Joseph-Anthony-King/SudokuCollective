using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Difficulties Controller Class
    /// </summary>
    [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DifficultiesController : ControllerBase
    {
        private readonly IDifficultiesService _difficultiesService;
        private readonly IAppsService _appsService;
        private readonly IRequestService _requestService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DifficultiesController> _logger;

        /// <summary>
        /// Difficulties Controller Constructor
        /// </summary>
        /// <param name="difficultiesService"></param>
        /// <param name="appsService"></param>
        /// <param name="requestService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="logger"></param>
        public DifficultiesController(
            IDifficultiesService difficultiesService,
            IAppsService appsService,
            IRequestService requestService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DifficultiesController> logger)
        {
            _difficultiesService = difficultiesService;
            _appsService = appsService;
            _requestService = requestService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// An endpoint to get a difficulty, does not require a login.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A difficulty.</returns>
        /// <response code="200">Returns a result object with the difficulty included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the difficulty was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the difficulty.</response>
        /// <remarks>
        /// The Get endpoint does not require an authorization token.  Id refers to the requested difficulty id.  Returns a difficulty.  Does not
        /// return difficulties of id 1 or 2 as those difficulties are not functional.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetAsync(int id)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                var result = await _difficultiesService.GetAsync(id);

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
            catch (Exception e)
            {
                var result = new Result
                {
                    IsSuccess = false,
                    Message = ControllerMessages.StatusCode500(e.Message)
                };

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }

        /// <summary>
        /// An endpoint to get a list of difficulties, does not require a login.
        /// </summary>
        /// <returns>A list of difficulties.</returns>
        /// <response code="200">Returns a result object with difficulties included as the payload array.</response>
        /// <response code="404">Returns a result object with the message stating difficuties were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting difficulties.</response>
        /// <remarks>
        /// The GetDifficulties endpoint does not require an authorization token.  Returns all available difficulties.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetDifficultiesAsync()
        {
            try
            {
                var result = await _difficultiesService.GetDifficultiesAsync();

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
            catch (Exception e)
            {
                var result = new Result
                {
                    IsSuccess = false,
                    Message = ControllerMessages.StatusCode500(e.Message)
                };

                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }

        /// <summary>
        /// An endpoint to create a difficulty, requires the superuser role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A difficulty.</returns>
        /// <response code="201">Returns a result object with the new difficulty included as the first element of payload array.</response>
        /// <response code="400">Returns a result object with the message stating why the request could not be fulfilled.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="500">Returns a result object with the message stating any errors creating the new difficulty.</response>
        /// <remarks>
        /// The Post endpoint requires the user to be logged in. Requires the superuser role. The request body parameter uses the request model.
        /// Please not that in order to this action to be successful the difficulty level would have to be unique.
        /// 
        /// The payload should be a CreateDifficultyPayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "name": string,             // a name for the new difficulty
        ///         "displayName": integeer,    // a display name for the new difficulty
        ///         "difficultyLevel": integer, // integer for the new difficulty level, this value would have to be unique and added as an enumeration
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER")]
        [HttpPost]
        public async Task<ActionResult<Result>> PostAsync([FromBody] Request request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _difficultiesService.CreateAsync(request);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode201(result.Message);

                        return StatusCode((int)HttpStatusCode.Created, result);
                    }
                    else
                    {
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
                return ControllerUtilities.ProcessException<DifficultiesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to update a difficult, requires the superuser role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated difficulty.</returns>
        /// <response code="200">Returns a result object with the updated difficulty included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the difficulty was not updated</response>
        /// <response code="500">Returns a result object with the message stating any errors updating the difficulty.</response>
        /// <remarks>
        /// The Update endpoint requires the user to be logged in. Requires the superuser role. The request body parameter uses the request model.
        /// 
        /// The payload should be an UpdateDifficultyPayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "id": integer,           // id for the difficulty
        ///         "name": string,          // a name for the difficulty
        ///         "displayName": integeer, // a display name for the difficulty
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Result>> UpdateAsync(
            int id,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (request == null) throw new ArgumentNullException(nameof(request));

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _difficultiesService.UpdateAsync(id, request);

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
                return ControllerUtilities.ProcessException<DifficultiesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to delete a difficult, requires the superuser role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message indicating if the difficulty was deleted.</returns>
        /// <response code="200">Returns a result object with the message indicating the difficulty was deleted.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the difficulty was not found</response>
        /// <response code="500">Returns a result object with the message stating any errors deleting the difficulty.</response>
        /// <remarks>
        /// The Delete endpoint requires the user to be logged in. Requires the superuser role. The request body parameter uses the request model.
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
        [Authorize(Roles = "SUPERUSER")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> DeleteAsync(
            int id,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (request == null) throw new ArgumentNullException(nameof(request));

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _difficultiesService.DeleteAsync(id);

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
                return ControllerUtilities.ProcessException<DifficultiesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }
    }
}
