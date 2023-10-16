using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Api.Controllers.V1
{
    /// <summary>
    /// Login Controller Class
    /// </summary>
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IAuthenticateService _authService;
        private readonly IUserManagementService _userManagementService;
        private readonly IRequestService _requestService;
        private readonly ILogger<LoginController> _logger;

        /// <summary>
        /// Login Controller Constructor
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="userManagementService"></param>
        /// <param name="requestService"></param>
        /// <param name="logger"></param>
        public LoginController(
            IAuthenticateService authService,
            IUserManagementService userManagementService,
            IRequestService requestService,
            ILogger<LoginController> logger)
        {
            _authService = authService;
            _userManagementService = userManagementService;
            _requestService = requestService;
            _logger = logger;
        }

        /// <summary>
        /// An endpoint which issues authorization tokens, does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An authenticated user and a authorization token.</returns>
        /// <response code="200">Returns a result object with the an authenticated user, an authorization token, and the token's expiration date included in the payload array.</response>
		/// <response code="400">Returns a result object with a message describing any validation errors for the username or password.</response>
        /// <response code="404">Returns a result object with the message stating the user was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors logging in the user.</response>
        /// <remarks>
        /// The Post endpoint does not require a login. The request body parameter uses the custom LoginRequest model documented in the schema.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {
        ///       "license": string,  // the app license must be valid using the applicable regex pattern as documented in the LoginRequest schema below
        ///       "userName": string, // user name must be unique, the api will ensure this for you; the applicable regex pattern as documented in the LoginRequest schema below
        ///       "password": string, // password is required, the applicable regex pattern as documented in the LoginRequest schema below
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Result>> PostAsync([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.AuthenticateAsync(request);

                if (result.IsSuccess)
                {
                    result.Message = ControllerMessages.StatusCode200(result.Message);

                    return Ok(result);
                }
                else if (result.Message.Equals(AppsMessages.AppDeactivatedMessage) || 
                    result.Message.Equals(AppsMessages.UserIsNotARegisteredUserOfThisAppMessage))
                {
                    result.Message = ControllerMessages.StatusCode404(result.Message);

                    return NotFound(result);
                }
                else
                {
                    var confirmAuthenticationIssueResponse = await _userManagementService
                        .ConfirmAuthenticationIssueAsync(
                            request.UserName,
                            request.Password,
                            request.License);

                    if (confirmAuthenticationIssueResponse == UserAuthenticationErrorType.USERNAMEINVALID)
                    {
                        result.Message = ControllerMessages.StatusCode404("No user is using this user name");
                        return NotFound(result);
                    }
                    else if (confirmAuthenticationIssueResponse == UserAuthenticationErrorType.PASSWORDINVALID)
                    {
                        result.Message = ControllerMessages.StatusCode404("Password is incorrect");
                        return NotFound(result);
                    }
                    else
                    {
                        result.Message = ControllerMessages.StatusCode404(result.Message);
                        return NotFound(result);
                    }
                }
            }
            catch (Exception e)
            {
                return ControllerUtilities.ProcessException<LoginController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }

        /// <summary>
        /// A method which confirms user names for given emails, does not require a login.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The user name associated with the given email.</returns>
        /// <response code="200">Returns a result object with the user name included as the first element in the payload array.</response>
		/// <response code="400">Returns a result object with a message describing any validation errors for the email.</response>
        /// <response code="404">Returns a result object with the message stating the user name was not found.</response>
        /// <response code="500">Returns a result object with the message stating any errors finding the user name.</response>
        /// <remarks>
        /// The ConfirmUserNameAsync method does not require a login. The request body parameter uses a custom ConfirmUserNameRequest model documented in the schema.
        /// 
        /// The request should be structured as follows:
        /// ```
        ///     {
        ///       "license": string, // the applicable regex pattern as documented in the ConfirmUserNameRequest model
        ///       "email": string,   // email is required, the applicable regex pattern is documented in the ConfirmUserNameRequest model
        ///     }     
        /// ```
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("ConfirmUserName")]
        public async Task<ActionResult<Result>> ConfirmUserNameAsync([FromBody] ConfirmUserNameRequest request)
        {
            try
            {
                var result = await _userManagementService.ConfirmUserNameAsync(
                    request.Email,
                    request.License);

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
                return ControllerUtilities.ProcessException<LoginController>(
                    this,
                    _requestService,
                    _logger,
                    e);
            }
        }
    }
}
