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
    /// Apps Controller Class
    /// </summary>
    /// <remarks>
    /// Apps Controller Constructor
    /// </remarks>
    /// <param name="appsService"></param>
    /// <param name="requestService"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    [Authorize(Roles = "SUPERUSER, ADMIN, USER")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public partial class AppsController(
        IAppsService appsService,
        IRequestService requestService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AppsController> logger) : ControllerBase
    {
        private readonly IAppsService _appsService = appsService;
        private readonly IRequestService _requestService = requestService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<AppsController> _logger = logger;

        /// <summary>
        /// An endpoint which gets an app, requires the user role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>Records for a given app.</returns>
        /// <response code="200">Returns a result object with the app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the app id.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the app.</response>
        /// <remarks>
        /// The Get endpoint requires the user to be logged in. Requires the user role. The query parameter id refers to the relevant app. 
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
        [HttpPost, Route("{id}")]
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
                    var result = await _appsService.GetAsync(id, request.RequestorId);

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

        /// <summary>
        /// An endpoint to update an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>An updated app.</returns>
        /// <response code="200">Returns a result object with the updated app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the app id.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors updating the app.</response>
        /// <remarks>
        /// The Update endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter id refers to the relevant app. 
        /// The request body parameter uses the request model.
        /// 
        /// The payload should be an AppPayload as documented in the schema. The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": paginator, // an object to control list pagination, not applicable here
        ///       "payload": {
        ///         "name": string,        // name is required, represents the apps name
        ///         "localUrl": string,    // localUrl is not required, an example is https://localhost:8081; regex documented in app schema below
        ///         "stagingUrl": string,  // stagingUrl is not required, an exampled is https://example-app.herokuapp.com; regex documented in app schema below
        ///         "TestUrl": string,     // TestUrl is not required, an exampled is https://example-test.herokuapp.com; regex documented in app schema below
        ///         "prodUrl": string,     // prodUrl is not required, an exampled is https://example-app.com; regex documented in app schema below
        ///         "isActive": boolean,   // isActive is required, represents the apps active status
        ///         "environment": integer // environment is required, this integer represents the apps release status: local, test, staging, or production
        ///         "permitSuperUserAccess": boolean, // permitSuperUserAccess is required, indicates if the super user has to register for access
        ///         "permitCollectiveLogins": boolean, // permitCollectiveLogins is required, indicates if collective users have to register for access
        ///         "disableCustomUrls": boolean, // disableCustomUrls is required, indicates if the app uses custom email and password actions
        ///         "customEmailConfirmationAction": string, // customEmailConfirmationAction is required, if implemented this represents the custom action
        ///         "customPasswordResetAction": string, // customPasswordResetAction is required, if implemented this represents the custom action
        ///         "useCustomSMTPServer": boolean, // useCustomSMTPServer is required, indicates if you've configured a custom SMTP server
        ///         "smtpServerSettings": { // smtpServerSettings is not required, this object holds your custom SMTP server settings
        ///           "smtpServer": string, // This value will be obtained from your custom SMTP server
        ///           "port": integer,      // This value will be obtained from your custom SMTP server
        ///           "userName": string,   // This value will be obtained from your custom SMTP server
        ///           "password": string,   // This value will be obtained from your custom SMTP server, will be encrypted in the database and will not return in requests
        ///           "fromEmail": string,  // This value will be obtained from your custom SMTP server
        ///         },
        ///         "timeFrame": integer,   // timeFrame is required, represents the timeFrame applied to authorization tokens, if set to years accessDuration is limited to 5
        ///         "accessDuration": integer, // accessDuration is required, represents the magnitude of the timeframe: eq: 1 day
        ///       },
        ///     }     
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPut, Route("{id}")]
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
                    var result = await _appsService.UpdateAsync(id, request);

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

        /// <summary>
        /// An endpoint to delete an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="license"></param>
        /// <param name="request"></param>
        /// <returns>A message documenting the result of the delete request.</returns>
        /// <response code="200">Returns a result object with the message documenting the result of the delete request.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the app id.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user is not the owner of the app.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deleting the app.</response>
        /// <remarks>
        /// The Delete endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameters id and license refers to the relevant app. 
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
        [HttpDelete, Route("{id}")]
        public async Task<ActionResult<Result>> DeleteAsync(
            int id,
            string license,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(request));

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsUserOwnerOThisfAppAsync(
                    _httpContextAccessor,
                    license,
                    request.RequestorId,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.DeleteOrResetAsync(id);

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
                    var result = new Result
                    {
                        IsSuccess = false,
                        Message = ControllerMessages.NotOwnerMessage
                    };

                    return StatusCode((int)HttpStatusCode.Forbidden, result);
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

        /// <summary>
        /// An endpoint which gets an app by its license, requires the superuser or admin role.
        /// </summary>
        /// <param name="license"></param>
        /// <param name="request"></param>
        /// <returns>Records for a given app.</returns>
        /// <response code="200">Returns a result object with the app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for license or request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the app.</response>
        /// <remarks>
        /// The GetByLicense endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter license refers to the relevant app. 
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
        [HttpPost, Route("{license}/GetByLicense")]
        public async Task<ActionResult<Result>> GetByLicenseAsync(
            string license,
            [FromBody] Request request)
        {
            try
            {
                if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(request));

                var licenseRegex = ControllerUtilities.GuidRegex();

                if (!licenseRegex.IsMatch(license)) throw new ArgumentException(null, nameof(license));

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.GetByLicenseAsync(license, request.RequestorId);

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

        /// <summary>
        /// An endpoint to get a list of all apps, requires the superuser or admin role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of all apps.</returns>
        /// <response code="200">Returns a result object with all apps included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating all apps were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting all apps.</response>
        /// <remarks>
        /// The GetApps endpoint requires the user to be logged in. Requires superuser or admin role. The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                 // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,         // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue,             // an enumeration indicating the field for sorting, accepts values 1, 9, 10, 11, 13
        ///         "OrderByDescending": boolean,    // a boolean to indicate is the order is ascending or descending
        ///         "includeCompletedGames": boolean // a boolean which only applies to game lists
        ///       },
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to apps are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to apps
        ///     2,  \\ indicates "userName" and is not applicable to apps
        ///     3,  \\ indicates "firstName" and is not applicable to apps
        ///     4,  \\ indicates "lastName" and is not applicable to apps
        ///     5,  \\ indicates "fullName" and is not applicable to apps
        ///     6,  \\ indicates "nickName" and is not applicable to apps
        ///     7,  \\ indicates "gameCount" and is not applicanle to apps
        ///     8,  \\ indicates "appCount" and is not applicable to apps
        ///     9,  \\ indicates "name" and is applicable to apps
        ///     10, \\ indicates "dateCreated" and is applicable to apps
        ///     11, \\ indicates "dateUpdated" and is applicable to apps
        ///     12, \\ indicates "difficultyLevel" and is not applicable to apps
        ///     13, \\ indicates "userCount" and is applicable to apps
        ///     14, \\ indicates "score" and is not applicable to apps
        ///     15  \\ indicates "url" and is not applicable to apps
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost]
        public async Task<ActionResult<Result>> GetAppsAsync([FromBody] Request request)
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
                    var result = await _appsService.GetAppsAsync(request);

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

        /// <summary>
        /// An endpoint to get a list of all apps associated to the logged in user as owner, requires the superuser or admin role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of all apps associated to the logged in user as owner.</returns>
        /// <response code="200">Returns a result object with the logged in user's apps included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the logged in user's apps were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the logged in user's apps.</response>
        /// <remarks>
        /// The GetMyApps endpoint requires the user to be logged in. Requires the superuser or admin role. Unlike the above GetApps endpoint this endpoint specifically gets 
        /// apps associated with the logged in user as the owner. The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                 // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,         // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue,             // an enumeration indicating the field for sorting, accepts values 1, 9, 10, 11, 13
        ///         "OrderByDescending": boolean,    // a boolean to indicate is the order is ascending or descending
        ///         "includeCompletedGames": boolean // a boolean which only applies to game lists
        ///       },
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to apps are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to apps
        ///     2,  \\ indicates "userName" and is not applicable to apps
        ///     3,  \\ indicates "firstName" and is not applicable to apps
        ///     4,  \\ indicates "lastName" and is not applicable to apps
        ///     5,  \\ indicates "fullName" and is not applicable to apps
        ///     6,  \\ indicates "nickName" and is not applicable to apps
        ///     7,  \\ indicates "gameCount" and is not applicanle to apps
        ///     8,  \\ indicates "appCount" and is not applicable to apps
        ///     9,  \\ indicates "name" and is applicable to apps
        ///     10, \\ indicates "dateCreated" and is applicable to apps
        ///     11, \\ indicates "dateUpdated" and is applicable to apps
        ///     12, \\ indicates "difficultyLevel" and is not applicable to apps
        ///     13, \\ indicates "userCount" and is applicable to apps
        ///     14, \\ indicates "score" and is not applicable to apps
        ///     15  \\ indicates "url" and is not applicable to apps
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost, Route("GetMyApps")]
        public async Task<ActionResult<Result>> GetMyAppsAsync([FromBody] Request request)
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
                    var result = await _appsService
                        .GetMyAppsAsync(
                        request.RequestorId,
                        request.Paginator);

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

        /// <summary>
        /// An endpoint to get a list of all apps associated to the logged in user as a user, requires the superuser or admin role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of all apps associated to the logged in user asa user.</returns>
        /// <response code="200">Returns a result object with the logged in user's registered apps included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the logged in user's registered apps were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the logged in user's registered apps.</response>
        /// <remarks>
        /// The GetMyRegisteredApps endpoint requires the user to be logged in. Requires the superuser or admin role. Unlike the above GetMyApps endpoint this endpoint 
        /// specifically gets apps associated with the logged in user as a user. The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                  // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,          // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue,              // an enumeration indicating the field for sorting, accepts values 1, 9, 10, 11, 13
        ///         "OrderByDescending": boolean,     // a boolean to indicate is the order is ascending or descending
        ///         "includeCompletedGames": boolean, // a boolean which only applies to game lists
        ///       },
        ///       "payload": {},          // an object holding additional request parameters, not applicable here
        ///     }     
        /// ```
        ///
        /// Sort values are as follows, those applicable to apps are indicated below:
        /// ```
        /// {
        ///     1,  \\ indicates "id" and is applicable to apps
        ///     2,  \\ indicates "userName" and is not applicable to apps
        ///     3,  \\ indicates "firstName" and is not applicable to apps
        ///     4,  \\ indicates "lastName" and is not applicable to apps
        ///     5,  \\ indicates "fullName" and is not applicable to apps
        ///     6,  \\ indicates "nickName" and is not applicable to apps
        ///     7,  \\ indicates "gameCount" and is not applicanle to apps
        ///     8,  \\ indicates "appCount" and is not applicable to apps
        ///     9,  \\ indicates "name" and is applicable to apps
        ///     10, \\ indicates "dateCreated" and is applicable to apps
        ///     11, \\ indicates "dateUpdated" and is applicable to apps
        ///     12, \\ indicates "difficultyLevel" and is not applicable to apps
        ///     13, \\ indicates "userCount" and is applicable to apps
        ///     14, \\ indicates "score" and is not applicable to apps
        ///     15  \\ indicates "url" and is not applicable to apps
        /// }
        /// ```
        /// </remarks>
        [Authorize(Roles = "SUPERUSER, ADMIN")]
        [HttpPost, Route("GetMyRegisteredApps")]
        public async Task<ActionResult<Result>> GetMyRegisteredAppsAsync([FromBody] Request request)
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
                    var result = await _appsService.GetMyRegisteredAppsAsync(
                        request.RequestorId,
                        request.Paginator);

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

        /// <summary>
        /// An endpoint to get a list of all users registered or non registered to an app, requires the superuser or admin role; retrieveAppUsers indicates if you want registered users (true) or non registered users (false).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="retrieveAppUsers"></param>
        /// <param name="request"></param>
        /// <returns>A list of all users registered to an app.</returns>
        /// <response code="200">Returns a result object with an apps registered users included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the apps registered users were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting the apps registered users.</response>
        /// <remarks>
        /// The GetAppUsers endpoint requires the user to be logged in. Requires the superuser or admin role. Returns a list of all users registered to an app. 
        /// The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                  // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,          // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue,              // an enumeration indicating the field for sorting, accepts values 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        ///         "OrderByDescending": boolean,     // a boolean to indicate is the order is ascending or descending
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
        [HttpPost, Route("{id}/GetAppUsers/{retrieveAppUsers}")]
        public async Task<ActionResult<Result>> GetAppUsersAsync(
            int id,
            bool retrieveAppUsers,
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
                    var result = await _appsService
                        .GetAppUsersAsync(
                            id,
                            request.RequestorId,
                            request.Paginator,
                            retrieveAppUsers);

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

        /// <summary>
        /// An endpoint to get a list of all users not registered to an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A list of all users not registered to an app.</returns>
        /// <response code="200">Returns a result object with all users not registered to an app included as the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating users not registered to an app were not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors getting users not registered to an app.</response>
        /// <remarks>
        /// The GetNonAppUsers endpoint requires the user to be logged in. Requires the superuser or admin role. Returns a list of all users not registered to an app. 
        /// The request body parameter uses the request model.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {                                 
        ///       "license": string,      // the app license must be valid using the applicable regex pattern as documented in the request schema below
        ///       "requestorId": integer, // the user id for the requesting logged in user
        ///       "appId": integer,       // the app id for the app the requesting user is logged into
        ///       "paginator": {
        ///         "page": integer,                  // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,          // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue ,             // an enumeration indicating the field for sorting, accepts values 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        ///         "OrderByDescending": boolean,     // a boolean to indicate is the order is ascending or descending
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
        [HttpPost, Route("{id}/GetNonAppUsers")]
        public async Task<ActionResult<Result>> GetNonAppUsersAsync(
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
                    var result = await _appsService
                        .GetAppUsersAsync(
                            id,
                            request.RequestorId,
                            request.Paginator,
                            false);

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

        /// <summary>
        /// An endpoint which adds a user to an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns>A user added to an app.</returns>
        /// <response code="200">Returns a result object with the added user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors adding the user to the app.</response>
        /// <remarks>
        /// The AddUser endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter id refers to the relevant app and the 
        /// query parameter userId refers to the relevant user to be added to the app. The request body parameter uses the request model.
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
        [HttpPut, Route("{id}/AddUser")]
        public async Task<ActionResult<Result>> AddUserAsync(
            int id,
            int userId,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (userId == 0) throw new ArgumentException(ControllerMessages.UserIdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.AddAppUserAsync(id, userId);

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

        /// <summary>
        /// An endpoint which removes a user from an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns>A message detailing a user has been removed from an app.</returns>
        /// <response code="200">Returns a result object with the message detailing a user has been removed from an app.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors removing the user from the app.</response>
        /// <remarks>
        /// The RemoveUser endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter id refers to the relevant app and the query 
        /// parameter userId refers to the relevant user to be removed from the app. The request body parameter  uses the request model.
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
        [HttpPut, Route("{id}/RemoveUser")]
        public async Task<ActionResult<Result>> RemoveUserAsync(
            int id,
            int userId,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (userId == 0) throw new ArgumentException(ControllerMessages.UserIdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.RemoveAppUserAsync(id, userId);

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

        /// <summary>
        /// An endpoint to activate an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message detailing if an app has been activated.</returns>
        /// <response code="200">Returns a result object with the activated app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors activating the app.</response>
        /// <remarks>
        /// The Activate endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter id refers to the relevant app. The 
        /// request body parameter uses the request model.
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
        [HttpPut, Route("{id}/Activate")]
        public async Task<ActionResult<Result>> ActivateAsync(
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
                    var result = await _appsService.ActivateAsync(id);

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

        /// <summary>
        /// An endpoint to deactivate an app, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A message detailing if an app has been deactivated.</returns>
        /// <response code="200">Returns a result object with the deactivated app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors deactivating the app.</response>
        /// <remarks>
        /// The Deactivate endpoint requires the user to be logged in. Requires the superuser or admin role. The query parameter id refers to the relevant app. The 
        /// request body parameter uses the request model.
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
        [HttpPut, Route("{id}/Deactivate")]
        public async Task<ActionResult<Result>> DeactivateAsync(
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
                    var result = await _appsService.DeactivateAsync(id);

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

        /// <summary>
        /// An endpoint to reset apps, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>A reset app with all games deleted.</returns>
        /// <response code="200">Returns a result object with the reset app included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user is not the owner of the app.</response>
        /// <response code="404">Returns a result object with the message stating the app was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors resetting the app.</response>
        /// <remarks>
        /// The Reset endpoint requires the user to be logged in. Requires the superuser or admin role. Returns a copy of the app with all games deleted. The 
        /// query parameters id refers to the relevant app. The request body parameter uses the request model.
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
        [HttpPut, Route("{id}/Reset")]
        public async Task<ActionResult<Result>> ResetAsync(
            int id,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsUserOwnerOThisfAppAsync(
                    _httpContextAccessor,
                    request.License,
                    request.RequestorId,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.DeleteOrResetAsync(id, true);

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
                    var result = new Result
                    {
                        IsSuccess = false,
                        Message = ControllerMessages.NotOwnerMessage
                    };

                    return StatusCode((int)HttpStatusCode.Forbidden, result);
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

        /// <summary>
        /// An endpoint to add admin privileges to a given user, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns>A copy of the user with admin privileges added for the given app.</returns>
        /// <response code="200">Returns a result object with the promoted user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors promoting the user within the app.</response>
        /// <remarks>
        /// The ActivateAdminPrivileges endpoint requires the user to be logged in. Requires the superuser or admin role. Returns a copy of the relevant user with admin 
        /// privileges added. The query parameters id refers to the relevant app. The request body parameter uses the request model.
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
        [HttpPut, Route("{id}/ActivateAdminPrivileges")]
        public async Task<ActionResult<Result>> ActivateAdminPrivilegesAsync(
            int id,
            int userId,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (userId == 0) throw new ArgumentException(ControllerMessages.UserIdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.ActivateAdminPrivilegesAsync(id, userId);

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

        /// <summary>
        /// An endpoint to remove admin privileges from a given user, requires the superuser or admin role.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns>A copy of the user with admin privileges removed for the given app.</returns>
        /// <response code="200">Returns a result object with the demoted user included as the first element in the payload array.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <response code="403">Returns a result object stating the request is forbidden if the user and app do not match that attached to the token.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors demoting the user within the app.</response>
        /// <remarks>
        /// The DeactivateAdminPrivileges endpoint requires the user to be logged in. Requires the superuser or admin role. Returns a copy of  the relevant user with admin 
        /// privileges removed. The query parameters id refers to the relevant app. The request body parameter uses the request model.
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
        [HttpPut, Route("{id}/DeactivateAdminPrivileges")]
        public async Task<ActionResult<Result>> DeactivateAdminPrivilegesAsync(
            int id,
            int userId,
            [FromBody] Request request)
        {
            try
            {
                if (id == 0) throw new ArgumentException(ControllerMessages.IdCannotBeZeroMessage);

                if (userId == 0) throw new ArgumentException(ControllerMessages.UserIdCannotBeZeroMessage);

                ArgumentNullException.ThrowIfNull(request);

                _requestService.Update(request);

                if (await _appsService.IsRequestValidOnThisTokenAsync(
                    _httpContextAccessor,
                    request.License,
                    request.AppId,
                    request.RequestorId))
                {
                    var result = await _appsService.DeactivateAdminPrivilegesAsync(id, userId);

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

        /// <summary>
        /// An endpoint to to get the apps gallery, does not require a login.
        /// </summary>
        /// <param name="paginator"></param>
        /// <returns>A list of all apps set for display within the gallery.</returns>
        /// <response code="200">Returns a result object with the all apps set to display in DisplayInGallery true.</response>
        /// <response code="400">Returns a result object with the message stating any validation errors for the request.</response>
        /// <remarks>
        /// The GetGallery endpoint does not require a login.  It has an optional body parameter for type Paginator to allow pagination over the results.
        /// If not included the endpoint will return a list of apps that are active and where the display in gallery parameter is set to true.  If the
        /// paginator is included it will paginate over the results, otherwise you will receive the entire gallery.
        ///
        /// The paginator object is structured as follows:
        /// ```
        ///     {
        ///         "page": integer,                 // this param works in conjection with itemsPerPage starting with page 1
        ///         "itemsPerPage": integer,         // in conjunction with page if you want items 11 through 21 page would be 2 and this would be 10
        ///         "sortBy": sortValue,             // an enumeration indicating the field for sorting, accepts values 1, 2, 9, 10, 11, 15
        ///         "OrderByDescending": boolean,    // a boolean to indicate is the order is ascending or descending
        ///         "includeCompletedGames": boolean // a boolean which does not apply here
        ///     }     
        /// ```
        ///
        /// The list of parameters included in the app items are actually a subset of the app parameters and are represented below in the GalleryApp
        /// schema.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost, Route("GetGallery")]
        public async Task<ActionResult<Result>> GetGalleryAppsAsync([FromBody] Paginator paginator = null)
        {
            var result = new Result();

            try
            {
                result = (Result)await _appsService.GetGalleryAppsAsync(paginator);

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
            catch(Exception e)
            {
                result.IsSuccess = false;

                result.Message = ControllerMessages.StatusCode400(e.Message);

                return BadRequest(result);
            }
        }
    }
}
