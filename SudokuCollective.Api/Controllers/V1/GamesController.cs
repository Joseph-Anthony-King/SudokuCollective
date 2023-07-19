using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Core.Enums;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Api.V1.Controllers
{
    /// <summary>
    /// Games Controller Class
    /// </summary>
    [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IGamesService _gamesService;
        private readonly IAppsService _appsService;
        private readonly IRequestService _requestService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GamesController> _logger;

        /// <summary>
        /// Games Controller Constructor
        /// </summary>
        /// <param name="gamesService"></param>
        /// <param name="appsService"></param>
        /// <param name="requestService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="logger"></param>
        public GamesController(
            IGamesService gamesService,
            IAppsService appsService,
            IRequestService requestService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GamesController> logger)
        {
            _gamesService = gamesService;
            _appsService = appsService;
            _requestService = requestService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// An endpoint to create a game, requires the user role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A game.</returns>
        /// <response code="201">Returns a result object with the new game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating any issuies processing the request.</response>
        /// <response code="500">Returns a result object with the message stating any errors creating the game.</response>
        /// <remarks>
        /// The Post endpoint requires the user to be logged in. Requires the user role. The request body parameter uses the request model.
        /// 
        /// The payload should be a CreateGamePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "userId": integer,        // userId is required, represents the userId of the signed in user
        ///         "difficultyId": integeer, // difficultyId is required, represents the difficultyId of the requested difficulty for the new game
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "USER")]
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
                    var result = await _gamesService.CreateAsync(request);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode201(result.Message);

                        return StatusCode((int)HttpStatusCode.Created, result);
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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to update a game, requires the superuser or admin roles.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated game.</returns>
        /// <response code="200">Returns a result object with the updated game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors updating the game.</response>
        /// <remarks>
        /// The Update endpoint requires the user to be logged in. Requires the superuser or admin roles. The query parameter id refers to the relevant game. 
        /// The request body parameter uses the request model.
        /// 
        /// The payload should be a GamePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "SudokuCells": SudokuCells[], // SudokuCells is required, represents the array of a games sudoku cells for updating
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
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
                    var result = await _gamesService.UpdateAsync(
                        id, 
                        request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to delete a game, requires the superuser or admin roles.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message indicating if the game was deleted.</returns>
        /// <response code="200">Returns a result object with the message indicating the game was deleted.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deleting the game.</response>
        /// <remarks>
        /// The Delete endpoint requires the user to be logged in. Requires the superuser or admin roles. The query parameter id refers to the relevant game. 
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
                    var result = await _gamesService.DeleteAsync(id);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to get a game, requires the superuser or admin roles
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A game.</returns>
        /// <response code="200">Returns a result object with the game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the game.</response>
        /// <remarks>
        /// The GetGame endpoint requires the user to be logged in. Requires superuser or admin roles. The query parameter id refers to the relevant game. 
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
        [HttpPost("{id}")]
        public async Task<ActionResult<Result>> GetGameAsync(
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
                    var result = await _gamesService.GetGameAsync(
                        id, 
                        request.AppId);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to get all games, requires the superuser or admin roles
        /// </summary>
        /// <param name="request"></param>
        /// <returns>All games.</returns>
        /// <response code="200">Returns a result object with all games included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all games were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting all games.</response>
        /// <remarks>
        /// The GetGames endpoint requires the user to be logged in. Requires the superuser or admin roles The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an enumeration indicating the field for sorting, accepts values 1, 10, 11, 12, 14
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to users are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to games
        ///     2,  \\ indicates "userName" and is not applicable to games
        ///     3,  \\ indicates "firstName" and is not applicable to games
        ///     4,  \\ indicates "lastName" and is not applicable to games
        ///     5,  \\ indicates "fullName" and is not applicable to games
        ///     6,  \\ indicates "nickName" and is not applicable to games
        ///     7,  \\ indicates "gameCount" and is not applicanle to games
        ///     8,  \\ indicates "appCount" and is not applicable to games
        ///     9,  \\ indicates "name" and is not applicable to games
        ///     10, \\ indicates "dateCreated" and is applicable to games
        ///     11, \\ indicates "dateUpdated" and is applicable to games
        ///     12, \\ indicates "difficultyLevel" and is applicable to games
        ///     13, \\ indicates "userCount" and is not applicable to games
        ///     14, \\ indicates "score" and is applicable to games
        ///     15  \\ indicates "url" and is not applicable to games
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost, Route("GetGames")]
        public async Task<ActionResult<Result>> GetGamesAsync([FromBody] Request request)
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
                    var result = await _gamesService.GetGamesAsync(request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to get a logged in user's game, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A logged in user's game.</returns>
        /// <response code="200">Returns a result object with all signed in user's game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all signed in user's game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting all the signed in user's game.</response>
        /// <remarks>
        /// The GetMyGame endpoint requires the user to be logged in. Requires the user role. This endpoint provides additional checks to ensure the requesting user
        /// is the originator of the game. User is indicated by the request requestorId. The query parameter  id refers to the relevant game. The request body 
        /// parameter uses the request model.
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
        [HttpPost, Route("{id}/GetMyGame")]
        public async Task<ActionResult<Result>> GetMyGameAsync(
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
                    var result = await _gamesService.GetMyGameAsync(
                        id,
                        request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to get a logged in user's games, requires the user role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A logged in user's games.</returns>
        /// <response code="200">Returns a result object with all signed in user's games included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all signed in user's games were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting all signed in user's games.</response>
        /// <remarks>
        /// The GetMyGames endpoint requires the user to be logged in. Requires the user role. This endpoint provides additional checks to ensure the requesting user
        /// is the originator of the game. User is indicated by the request requestorId. The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an enumeration indicating the field for sorting, accepts values 1, 10, 11, 12, 14
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to users are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to games
        ///     2,  \\ indicates "userName" and is not applicable to games
        ///     3,  \\ indicates "firstName" and is not applicable to games
        ///     4,  \\ indicates "lastName" and is not applicable to games
        ///     5,  \\ indicates "fullName" and is not applicable to games
        ///     6,  \\ indicates "nickName" and is not applicable to games
        ///     7,  \\ indicates "gameCount" and is not applicanle to games
        ///     8,  \\ indicates "appCount" and is not applicable to games
        ///     9,  \\ indicates "name" and is not applicable to games
        ///     10, \\ indicates "dateCreated" and is applicable to games
        ///     11, \\ indicates "dateUpdated" and is applicable to games
        ///     12, \\ indicates "difficultyLevel" and is applicable to games
        ///     13, \\ indicates "userCount" and is not applicable to games
        ///     14, \\ indicates "score" and is applicable to games
        ///     15  \\ indicates "url" and is not applicable to games
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpPost, Route("GetMyGames")]
        public async Task<ActionResult<Result>> GetMyGamesAsync([FromBody] Request request)
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
                    var result = await _gamesService.GetMyGamesAsync(request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to update a logged in user's games, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated logged in user's game.</returns>
        /// <response code="200">Returns a result object with the signed in user's updated game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all signed in user's game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors updating the signed in user's games.</response>
        /// <remarks>
        /// The UpdateMyGame endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant game. 
        /// This endpoint provides additional checks to ensure the requesting user is the originator of the game. User is indicated by the 
        /// request requestorId. The request body parameter uses the request model.
        /// 
        /// The payload should be a GamePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "SudokuCells": SudokuCells[], // SudokuCells is required, represents the array of a games sudoku cells for updating
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpPut("{id}/UpdateMyGame")]
        public async Task<ActionResult<Result>> UpdateMyGameAsync(
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
                    var result = await _gamesService.UpdateMyGameAsync(
                        id, 
                        request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to delete a logged in user's game, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message indicating if the user's game was deleted.</returns>
        /// <response code="200">Returns a result object with the message indicating the signed in user's game was deleted.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the signed in user's game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deleting the signed in user's game.</response>
        /// <remarks>
        /// The DeleteMyGame endpoint requires the user to be logged in. Requires the user role. This endpoint provides additional checks to ensure the requesting user
        /// is the originator of the games. User is indicated by the request requestorId. The query parameter id refers to the relevant game. The request body parameter 
        /// uses the request model.
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
        [HttpDelete("{id}/DeleteMyGame")]
        public async Task<ActionResult<Result>> DeleteMyGameAsync(
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
                    var result = await _gamesService.DeleteMyGameAsync(
                        id,
                        request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to check a game, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A checked game to see if it's been solved.</returns>
        /// <response code="200">Returns a result object with the checked game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the game was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors checking the game.</response>
        /// <remarks>
        /// The Check endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant game. The request body 
        /// parameter uses the request model.
        /// 
        /// The payload should be a GamePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "SudokuCells": SudokuCells[], // SudokuCells is required, represents the array of a games sudoku cells for updating
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpPut, Route("{id}/Check")]
        public async Task<ActionResult<Result>> CheckAsync(
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
                    var result = await _gamesService.CheckAsync(id, request);

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
                return ControllerUtilities.ProcessException<GamesController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// An endpoint to create an annonymous game without a signed in user, does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An annonymous game.</returns>
        /// <response code="200">Returns a result object with the annonymous game included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation issues with the request.</response>
        /// <response code="500">Returns a result object with the message stating any errors creating the annonymous game.</response>
        /// <remarks>
        /// The CreateAnnonymous endpoint does not require a logged in user. The request query parameter uses the AnnonymousGameRequest model documented in the schema.
        /// Please note that 0 represents a difficulty level of null and 1 represents a difficulty level of test and neither value is valid. Requesting a game with
        /// either difficulty level will trigger a code 400 error due to difficulty level validity.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {
        ///       "difficultyLevel": integer, // The id for the requested difficulty level for the new annonymous game
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("CreateAnnonymous")]
        public async Task<ActionResult<Result>> CreateAnnonymousAsync([FromQuery] AnnonymousGameRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IResult result;

                if (request.DifficultyLevel == DifficultyLevel.NULL || request.DifficultyLevel == DifficultyLevel.TEST)
                {
                    result = new Result
                    { 
                        Message = ControllerMessages.StatusCode400(DifficultiesMessages.DifficultyNotValidMessage)
                    };
                    
                    return BadRequest(result);
                }

                result = await _gamesService.CreateAnnonymousAsync(request.DifficultyLevel);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    result.Message = ControllerMessages.StatusCode400(result.Message);

                    return BadRequest(result);
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
        /// An endpoint to check an annonymous game without a signed in user, does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A message indicating if the sudoku puzzle has been solved.</returns>
        /// <response code="200">Returns a result object with the solved sudoku puzzle included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating if the annonymous game was not solved.</response>
        /// <response code="500">Returns a result object with the message stating any errors solving the sudoku puzzle.</response>
        /// <remarks>
        /// The CheckAnnonymous endpoint does not require a logged in user. The request body parameter uses the AnnonymousCheckRequest model documented in the schema.
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
        [HttpPost("CheckAnnonymous")]
        public ActionResult<Result> CheckAnnonymous([FromBody] AnnonymousCheckRequest request)
        {
            try
            {
                var intList = new List<int>();

                intList.AddRange(request.FirstRow);
                intList.AddRange(request.SecondRow);
                intList.AddRange(request.ThirdRow);
                intList.AddRange(request.FourthRow);
                intList.AddRange(request.FifthRow);
                intList.AddRange(request.SixthRow);
                intList.AddRange(request.SeventhRow);
                intList.AddRange(request.EighthRow);
                intList.AddRange(request.NinthRow);

                var result = _gamesService.CheckAnnonymous(intList);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    result.Message = ControllerMessages.StatusCode400(result.Message);

                    return BadRequest(result);
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
    }
}
