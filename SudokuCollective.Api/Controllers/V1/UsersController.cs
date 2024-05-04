using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Validation.Attributes;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Users Controller Class
    /// </summary>
    /// <remarks>
    /// Users Controller Constructor
    /// </remarks>
    /// <param name="usersService"></param>
    /// <param name="appsService"></param>
    /// <param name="requestService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    /// <param name="environment"></param>
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController(
        IUsersService usersService,
        IAppsService appsService,
        IRequestService requestService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UsersController> logger,
        IWebHostEnvironment environment) : ControllerBase
    {
        private readonly IUsersService _usersService = usersService;
        private readonly IAppsService _appsService = appsService;
        private readonly IRequestService _requestService = requestService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<UsersController> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;

        /// <summary>
        /// An endpoint which gets a given user relative to the requesting app, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>Records for a given user relative to the requesting app.</returns>
        /// <response code="200">Returns a result object with the user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the user.</response>
        /// <remarks>
        /// The Get endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant user.
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
                    var result = await _usersService.GetAsync(
                        id,
                        request.License,
                        request);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to update users, requires the user role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated user.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors updating the user.</response>
        /// <remarks>
        /// The Update endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant user. 
        /// The request body parameter uses the request model.
        /// 
        /// The payload should be an UpdateUserPayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "userName": string,  // username is required; uses the same regex pattern as documented in the LoginRequest schema below
        ///         "firstName": string, // firstname is required
        ///         "lastName": string,  // lastname is required
        ///         "nickName": string,  // nickname is not required
        ///         "email": string,     // email is required; uses the same regex pattern as documented in the SignupRequest schema below
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Result>> UpdateAsync(
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
                    string baseUrl;

                    if (Request != null)
                    {
                        baseUrl = Request.Host.ToString();
                    }
                    else
                    {
                        baseUrl = "https://SudokuCollective.com";
                    }

                    string emailtTemplatePath;

                    if (!string.IsNullOrEmpty(_environment.WebRootPath))
                    {
                        emailtTemplatePath = Path.Combine(_environment.WebRootPath, "/Content/EmailTemplates/confirm-old-email-inlined.html");

                        var currentDirectory = string.Format("{0}{1}", AppContext.BaseDirectory, "{0}");

                        emailtTemplatePath = string.Format(currentDirectory, emailtTemplatePath);
                    }
                    else
                    {
                        emailtTemplatePath = "../../Content/EmailTemplates/confirm-old-email-inlined.html";
                    }

                    var result = await _usersService.UpdateAsync(
                        id,
                        request,
                        baseUrl,
                        emailtTemplatePath);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to delete users, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message documenting the result of the delete request.</returns>
        /// <response code="200">Returns a result object with a message documenting the result of the delete request.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deleting the user.</response>
        /// <remarks>
        /// The Delete endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant user. 
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
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> DeleteAsync(
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
                    var result = await _usersService.DeleteAsync(id, request.License);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to get a list of all users, requires superuser or admin role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Returns a result object with all user included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all users were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting all users.</response>
        /// <remarks>
        /// The GetUsers endpoint requires the user to be logged in. Requires superuser or admin roles. The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                  // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer           // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue               // an enumeration indicating the field for sorting, accepts values 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        ///         "OrderByDescending": boolean      // a boolean to indicate is the order is ascending or descending
        ///         "includeCompletedGames": boolean, // a boolean which only applies to game lists
        ///       },
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to users are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to users
        ///     2,  \\ indicates "userName" and is applicable to users
        ///     3,  \\ indicates "firstName" and is applicable to users
        ///     4,  \\ indicates "lastName" and is applicable to users
        ///     5,  \\ indicates "fullName" and is applicable to users
        ///     6,  \\ indicates "nickName" and is applicable to users
        ///     7,  \\ indicates "gameCount" and is applicanle to users
        ///     8,  \\ indicates "appCount" and is applicable to users
        ///     9,  \\ indicates "name" and is not applicable to users
        ///     10, \\ indicates "dateCreated" and is applicable to users
        ///     11, \\ indicates "dateUpdated" and is applicable to users
        ///     12, \\ indicates "difficultyLevel" and is not applicable to users
        ///     13, \\ indicates "userCount" and is not applicable to users
        ///     14, \\ indicates "score" and is not applicable to users
        ///     15  \\ indicates "url" and is not applicable to users
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost]
        public async Task<ActionResult<Result>> GetUsersAsync(
            [FromBody] Request request)
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
                    var result = await _usersService.GetUsersAsync(
                        request.RequestorId,
                        request.License,
                        request.Paginator);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to add roles to a user for a given app, requires superuser or admin roles.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated copy of the user with the added role.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user or role was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors adding the role to the user.</response>
        /// <remarks>
        /// The AddRoles endpoint requires the user to be logged in. Requires superuser or admin roles.  Only the admin role (role id 3) can be added to 
        /// a user, the superuser role cannot be added using this endpoint. The query parameter id refers to the relevant user. The request body 
        /// parameter uses the request model.
        /// 
        /// The payload should be an UpdateUserRolePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "roleIds": Array[integer],  // an array of integers for the roles you wish to add
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPut("{id}/AddRoles")]
        public async Task<ActionResult<Result>> AddRolesAsync(
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
                    var result = await _usersService.AddUserRolesAsync(
                        id,
                        request,
                        request.License);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to remove roles from a user for a given app, requires superuser or admin roles.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated copy of the user with the removed role.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user or role was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors remove the roles from the user.</response>
        /// <remarks>
        /// The RemoveRoles endpoint requires the user to be logged in. Requires superuser or admin roles. The query parameter id refers to the relevant user. 
        /// The request body parameter uses the request model.
        /// 
        /// The payload should be an UpdateUserRolePayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "roleIds": Array[integer],  // an array of integers for the roles you wish to remove
        ///       }
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPut("{id}/RemoveRoles")]
        public async Task<ActionResult<Result>> RemoveRolesAsync(
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
                    var result = await _usersService.RemoveUserRolesAsync(
                        id,
                        request,
                        request.License);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to activate a user, requires the superuser role.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An updated copy of the user with the new active status.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors activating the user.</response>
        /// <remarks>
        /// The Activate endpoint requires the user to be logged in. Requires the superuser role.
        /// </remarks>
        [Authorize(Roles = "SUPERUSER")]
        [HttpPut("{id}/Activate")]
        public async Task<ActionResult<Result>> ActivateAsync(int id)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                var result = await _usersService.ActivateAsync(id);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to deactivate a user, requires the superuser role.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An updated copy of the user with the new active status.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deactivating the user.</response>
        /// <remarks>
        /// The Deactivate endpoint requires the user to be logged in. Requires the superuser role.
        /// </remarks>
        [Authorize(Roles = "SUPERUSER")]
        [HttpPut("{id}/Deactivate")]
        public async Task<ActionResult<Result>> DeactivateAsync(int id)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                var result = await _usersService.DeactivateAsync(id);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint which confirms user emails, does not require a login.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>A response in the payload of type ConfirmEmailResult documented in the schema below that provides the results of the email confirmation.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the user's email was confirmed.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating user was not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The ConfirmEmail endpoint does not require a login. If you've implemented a custom confirm email action that action will link back to this 
        /// endpoint once confirmed.  Your action will send the token to this endpoint to complete the process, the boolean will indicate if the email 
        /// was confirmed.
        /// 
        /// The payload will be of type ConfirmEmailResult documenting the results of the email confirmation as indicated in the schema below.
        /// 
        /// This enpdoint applies to two work flows: new sign ups and email updates.  The 'IsUpdate' property will indicate to you if this is a
        /// new sign up or an update of a pre-existing user email.  Additionally, there is a property 'ConfirmationType' which indicates where
        /// the user is in the email confirmation process.  For this property the enumeration indicates the following:
        /// /// ```
        /// {
        ///     0, \\ is null
        ///     1, \\ is a new profile sign up
        ///     2, \\ is an old email confirmed
        ///     3, \\ is a new email confirmed
        /// }
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("ConfirmEmail/{token}")]
        public async Task<ActionResult<Result>> ConfirmEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

                var guidValidator = new GuidValidatedAttribute();

                if (!guidValidator.IsValid(token)) throw new ArgumentException(AttributeMessages.InvalidToken);

                string baseUrl;

                if (Request != null)
                {
                    baseUrl = Request.Host.ToString();
                }
                else
                {
                    baseUrl = "https://SudokuCollective.com";
                }

                string emailtTemplatePath;

                if (!string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    emailtTemplatePath = Path.Combine(_environment.WebRootPath, "/Content/EmailTemplates/confirm-new-email-inlined.html");

                    var currentDirectory = string.Format("{0}{1}", AppContext.BaseDirectory, "{0}");

                    emailtTemplatePath = string.Format(currentDirectory, emailtTemplatePath);
                }
                else
                {
                    emailtTemplatePath = "../../Content/EmailTemplates/confirm-new-email-inlined.html";
                }

                var result = await _usersService.ConfirmEmailAsync(token, baseUrl, emailtTemplatePath);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint which cancels oustanding email confirmation requests, requires the user role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A boolean indicating if the outstanding email confirmation was cancelled.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the user's email confirmation request was cancelled.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user's email confirmation was not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The CancelEmailConfirmation endpoint requires the user to be logged in. Requires the user role. The request body parameter uses the request model.
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
        /// 
        /// A copy of the updated user record will be included in the payload data.
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpPut, Route("cancelEmailConfirmation")]
        public async Task<ActionResult<Result>> CancelEmailConfirmationAsync([FromBody] Request request)
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
                    var result = await _usersService.CancelEmailConfirmationRequestAsync(request.RequestorId, request.AppId);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to reset user password, your custom password action will plug into this endpoint; does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns>An updated copy of the user.</returns>
        /// <response code="200">Returns a result object with the updated user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors resetting the user's password.</response>
        /// <remarks>
        /// The ResetPassword endpoint does not require a login. This method works in conjunction with the custom email action for your app. When your custom action 
        /// is ready to submit it's data it will submit the token it receives from the email along with the new password to this endpoint. The request body parameter 
        /// uses the custom ResetPasswordRequest model documented in the schema.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {
        ///       "newPassword": string, // the new password, the applicable regex pattern is documented in the ResetPasswordRequest model
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpPut("ResetPassword/{token}")]
        public async Task<ActionResult<Result>> ResetPasswordAsync([FromBody] ResetPasswordRequest request, string token)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);

                var guidValidator = new GuidValidatedAttribute();

                if (!guidValidator.IsValid(token)) throw new ArgumentException(AttributeMessages.InvalidToken);

                User user = null;
                Core.Interfaces.Models.DomainObjects.Params.IResult result = null;

                var userResult = await _usersService.GetUserByPasswordTokenAsync(token);

                if (userResult.IsSuccess)
                {
                    user = (User)userResult.Payload[0];
                }
                else
                {
                    result = new Result
                    {
                        Message = ControllerMessages.StatusCode404(userResult.Message)
                    };

                    return NotFound(result);
                }

                var license = (await _usersService.GetAppLicenseByPasswordTokenAsync(token)).License;

                var updatePasswordRequest = new UpdatePasswordRequest
                {
                    UserId = user.Id,
                    NewPassword = request.NewPassword,
                    License = license
                };

                result = await _usersService.UpdatePasswordAsync(updatePasswordRequest);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to request that a password reset email; does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A boolean indicating if the password reset email was sent.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the password reset email was sent.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The RequestPasswordReset endpoint does not require a login. It sends password reset emails. The request body parameter uses the custom RequestPasswordResetRequest 
        /// model documented in the schema.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string, // the app license must be valid using the applicable regex pattern as documented in the RequestPasswordResetRequest model
        ///       "email": string,   // email is required, the applicable regex pattern is documented in the RequestPasswordResetRequest model
        ///     }     
        /// ```
        /// </remarks>
        /// 
        /// A copy of the updated user record will be included in the payload data.
        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        public async Task<ActionResult<Result>> RequestPasswordResetAsync([FromBody] RequestPasswordResetRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);

                string baseUrl;

                if (Request != null)
                {
                    baseUrl = Request.Host.ToString();
                }
                else
                {
                    baseUrl = "https://SudokuCollective.com";
                }

                string emailtTemplatePath;

                if (!string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    emailtTemplatePath = Path.Combine(_environment.WebRootPath, "/Content/EmailTemplates/password-reset-requested-inlined.html");

                    var currentDirectory = string.Format("{0}{1}", AppContext.BaseDirectory, "{0}");

                    emailtTemplatePath = string.Format(currentDirectory, emailtTemplatePath);
                }
                else
                {
                    emailtTemplatePath = "../../Content/EmailTemplates/password-reset-requested-inlined.html";
                }

                var result = await _usersService.RequestPasswordResetAsync(request, baseUrl, emailtTemplatePath);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint to request that a password reset email be resent; does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A boolean indicating if the password reset email was resent.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the password reset email was resent.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the user or request password reset were not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The ResendPasswordReset endpoint does not require a login. It resends password reset emails if the user has lost the original email. 
        /// The request body parameter uses the custom ResendRequestPasswordRequest model documented in the schema.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "userId": integer, // the id for the requesting user
        ///       "appId": integer,  // the id for the relevant app
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpPut("ResendPasswordReset")]
        public async Task<ActionResult<Result>> ResendPasswordResetAsync([FromBody] ResendRequestPasswordRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);

                string baseUrl;

                if (Request != null)
                {
                    baseUrl = Request.Host.ToString();
                }
                else
                {
                    baseUrl = "https://SudokuCollective.com";
                }

                string emailtTemplatePath;

                if (!string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    emailtTemplatePath = Path.Combine(_environment.WebRootPath, "/Content/EmailTemplates/password-reset-requested-inlined.html");

                    var currentDirectory = string.Format("{0}{1}", AppContext.BaseDirectory, "{0}");

                    emailtTemplatePath = string.Format(currentDirectory, emailtTemplatePath);
                }
                else
                {
                    emailtTemplatePath = "../../Content/EmailTemplates/password-reset-requested-inlined.html";
                }

                var result = await _usersService.ResendPasswordResetAsync(
                    request.UserId,
                    request.AppId,
                    baseUrl,
                    emailtTemplatePath);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else
                {
                    if (_environment.IsDevelopment() == false)
                        result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
            }
            catch (Exception e)
            {
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint which cancels oustanding password reset requests, requires the user role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A boolean indicating if the outstanding password reset request was cancelled.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the user's password reset request was cancelled.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user or the password request request were not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The CancelPasswordReset endpoint requires the user to be logged in. Requires the user role. The request body parameter uses the request model.
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
        /// 
        /// A copy of the updated user record will be included in the payload data.
        [Authorize(Roles = "USER")]
        [HttpPut("cancelPasswordReset")]
        public async Task<ActionResult<Result>> CancelPasswordResetAsync([FromBody] Request request)
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
                    var result = await _usersService.CancelPasswordResetRequestAsync(request.RequestorId, request.AppId);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }

        /// <summary>
        /// An endpoint which cancels oustanding email confirmation and password reset requests, requires the user role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A boolean indicating if outstanding email confirmation and password reset requests were cancelled.</returns>
        /// <response code="200">Returns a result object with the message and isSuccess value indicating if the user's cancel email request and password reset request was cancelled.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating if the user or requests were not found. </response>
        /// <response code="500">Returns a result object with the message stating any errors processing the request.</response>
        /// <remarks>
        /// The CancelAllEmailRequests endpoint requires the user to be logged in. Requires the user role. The request body parameter uses the request model.
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
        /// 
        /// A copy of the updated user record will be included in the payload data.
        [Authorize(Roles = "USER")]
        [HttpPut("cancelAllEmailRequests")]
        public async Task<ActionResult<Result>> CancelAllEmailRequestsAsync([FromBody] Request request)
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
                    var result = await _usersService.CancelAllEmailRequestsAsync(request.RequestorId, request.AppId);

                    if (result.IsSuccess)
                    {
                        result.Message = ControllerMessages.StatusCode200(result.Message);

                        return Ok(result);
                    }
                    else
                    {
                        if (_environment.IsDevelopment() == false)
                            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

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
                return await ControllerUtilities.ProcessException<UsersController>(
                    this,
                    _requestService,
                    _logger,
                    e,
                    environment);
            }
        }
    }
}
