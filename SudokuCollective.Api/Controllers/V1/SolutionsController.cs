using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Api.V1.Controllers
{
    /// <summary>
    /// Solutions Controller Class
    /// </summary>
    /// <remarks>
    /// Solutions Controller Constructor
    /// </remarks>
    /// <param name="solutionsService"></param>
    /// <param name="appsService"></param>
    /// <param name="requestService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SolutionsController(
        ISolutionsService solutionsService,
        IAppsService appsService,
        IRequestService requestService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SolutionsController> logger) : ControllerBase
    {
        private readonly ISolutionsService _solutionsService = solutionsService;
        private readonly IAppsService _appsService = appsService;
        private readonly IRequestService _requestService = requestService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<SolutionsController> _logger = logger;

        /// <summary>
        /// An endpoint to get a solution, requires the user role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A solution.</returns>
        /// <response code="200">Returns a result object with the a solution included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the solution was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors finding the solution.</response>
        /// <remarks>
        /// The Get endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant solution. 
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
        [Authorize(Roles = "USER")]
        [HttpPost("{id}")]
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
                    var result = await _solutionsService.GetAsync(id);

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
                return ControllerUtilities.ProcessException<SolutionsController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to get solutions, requires the user role
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of solutions.</returns>
        /// <response code="200">Returns a result object with solutions included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating solutions were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting solutions.</response>
        /// <remarks>
        /// The GetSolutions endpoint requires the user to be logged in. Requires the user role. The request body parameter uses the request model.
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
        [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
        [HttpPost]
        public async Task<ActionResult<Result>> GetSolutionsAsync([FromBody] Request request)
        {
            try
            {
                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    ArgumentNullException.ThrowIfNull(request);

                    _requestService.Update(request);

                    var result = await _solutionsService.GetSolutionsAsync(request);

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
                return ControllerUtilities.ProcessException<SolutionsController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to solve sudoku puzzles, does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>If solvable, a solved sudoku puzzle.</returns>
        /// <response code="200">Returns a result object with the a solved sudoku puzzle included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="500">Returns a result object with the message stating any errors solving the sudoku puzzle.</response>
        /// <remarks>
        /// The Solve endpoint does not require a logged in user. The request body parameter uses the AnnonymousCheckRequest model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {
        ///       "firstRow": integer[],   // An array of integers representing the first row of the annonymous game, unknown values are represented by 0
        ///       "secondRow": integer[],  // An array of integers representing the second row of the annonymous game, unknown values are represented by 0
        ///       "thirdRow": integer[],   // An array of integers representing the third row of the annonymous game, unknown values are represented by 0
        ///       "fourthRow": integer[],  // An array of integers representing the fourth row of the annonymous game, unknown values are represented by 0
        ///       "fifthRow": integer[],   // An array of integers representing the fifth row of the annonymous game, unknown values are represented by 0
        ///       "sixthRow": integer[],   // An array of integers representing the sixth row of the annonymous game, unknown values are represented by 0
        ///       "seventhRow": integer[], // An array of integers representing the seventhRow row of the annonymous game, unknown values are represented by 0
        ///       "eighthRow": integer[],  // An array of integers representing the eighthRow row of the annonymous game, unknown values are represented by 0
        ///       "ninthRow": integer[],   // An array of integers representing the ninthRow row of the annonymous game, unknown values are represented by 0
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpPost, Route("Solve")]
        public async Task<ActionResult<Result>> SolveAsync([FromBody] AnnonymousCheckRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);

                var result = await _solutionsService.SolveAsync(request);

                if (result.IsSuccess)
                {
                    if (result.Payload.Count > 0)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                }
                else
                {
                    result.Message = ControllerMessages.StatusCode400(result.Message);

                    return BadRequest(result);
                }
            }
            catch (Exception e)
            {
                return ControllerUtilities.ProcessException<SolutionsController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to generate sudoku puzzles, does not require a login.
        /// </summary>
        /// <returns>A sudoku puzzle.</returns>
        /// <response code="200">Returns a result object with the a sudoku puzzle included as the first element in the payload array.</response>
        /// <response code="404">Returns a result object with the message stating why the request could not be fulfilled.</response>
        /// <response code="500">Returns a result object with the message stating any errors generating the sudoku puzzle.</response>
        /// <remarks>
        /// The Generate endpoint does not require a logged in user.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet, Route("Generate")]
        public async Task<ActionResult<Result>> GenerateAsync()
        {
            try
            {
                var result = await _solutionsService.GenerateAsync();

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
                return ControllerUtilities.ProcessException<SolutionsController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to generate solutions, requires the superuser or admin roles
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A message indicating that solutions are being created.</returns>
        /// <response code="200">Returns a result object with a message indicating sudoku solutions are being generated.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating why the request could not be fulfilled.</response>
        /// <response code="500">Returns a result object with the message stating any errors generating the sudoku solutions.</response>
        /// <remarks>
        /// The AddSolutions endpoint requires the user to be logged in. Requires the superuser or admin roles The request body parameter uses the request model.
        /// 
        /// The payload should be an AddSolutionsPayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "limit": integer, // Amount of solutions to generate, limited to 100
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost, Route("AddSolutions")]
        public async Task<ActionResult<Result>> AddSolutionsAsync([FromBody] Request request)
        {
            try
            {
                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = _solutionsService.GenerateSolutions(request);

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
                return ControllerUtilities.ProcessException<SolutionsController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }
    }
}
