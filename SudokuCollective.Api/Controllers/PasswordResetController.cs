using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Api.Controllers
{
    /// <summary>
    /// Password Reset Controller
    /// </summary>
    [Route("[controller]")]
    [Controller]
    public class PasswordResetController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IAppsService _appsService;

        /// <summary>
        /// Password Reset Controller Constructor
        /// </summary>
        /// <param name="usersService"></param>
        /// <param name="appsService"></param>
        public PasswordResetController(
            IUsersService usersService,
            IAppsService appsService)
        {
            _usersService = usersService;
            _appsService = appsService;
        }

        /// <summary>
        /// A default endpoint to process password reset requests, does not require a login.
        /// </summary>
        /// <returns>A redirect to the default password reset view.</returns>
        /// <response code="200">A redirect to the default password reset view.</response>
        /// <remarks>
        /// This is a default endpoint to handle password resets.  It is strongly recommended that you implement a 
        /// custom password reset action to handle such requests, this endpoint is simply a placeholder to handle
        /// such requests until you've implemented a custom action.  In order to implement such a request you have to 
        /// create it within your app (the details of which are dependent upon your apps particular framework) and 
        /// then enable it by setting the following app properties:
        ///
        /// ```DisableCustomUrls``` = ```false```
        ///
        /// ```UseCustomPasswordResetAction``` = ```true```
        ///
        /// ```CustomPasswordResetAction``` = the custom action you've created
        ///
        /// So if the url for your app is https://yourapp and the custom action is ```passwordReset``` then your users 
        /// will be directed to the following:
        ///
        /// ```https://yourapp/passwordReset/{token}```
        ///
        /// Please note the url is dependent on the release environment, so if your release environment is set to local 
        /// the requests will be directed to your local url and if set to staging the requests will be directed to your
        /// staging url, etc.
        ///
        /// Until such time as the above conditions are met such requests will continue to be directed to this default page.
        ///
        /// The token will be provided by the api and will be sent to the user in the confirmation email, along with a 
        /// link to either this default password reset action or to your custom password reset action. Once your
        /// password reset action is implemented it will submit the token and new password to the RequestPasswordReset 
        /// endpoint in the user controller.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{token}")]
        public async Task<IActionResult> Index(string token)
        {
            var licenseResult = await _usersService
                .GetAppLicenseByPasswordTokenAsync(token);

            var result = await _usersService
                .InitiatePasswordResetAsync(
                    token,
                    licenseResult.License);

            if (result.IsSuccess)
            {
                var passwordReset = new SudokuCollective.Api.Models.PasswordReset
                {
                    IsSuccess = result.IsSuccess,
                    UserId = ((InitiatePasswordResetResult)result.Payload[0]).User.Id,
                    UserName = ((InitiatePasswordResetResult)result.Payload[0]).User.UserName,
                    AppTitle = ((InitiatePasswordResetResult)result.Payload[0]).App.Name,
                    AppId = ((InitiatePasswordResetResult)result.Payload[0]).App.Id,
                    AppUrl = ((InitiatePasswordResetResult)result.Payload[0]).App.Environment == ReleaseEnvironment.LOCAL ? ((InitiatePasswordResetResult)result.Payload[0]).App.LocalUrl : 
                        ((InitiatePasswordResetResult)result.Payload[0]).App.Environment == ReleaseEnvironment.QA ? ((InitiatePasswordResetResult)result.Payload[0]).App.QaUrl :
                        ((InitiatePasswordResetResult)result.Payload[0]).App.Environment == ReleaseEnvironment.STAGING ? ((InitiatePasswordResetResult)result.Payload[0]).App.StagingUrl :
                        ((InitiatePasswordResetResult)result.Payload[0]).App.ProdUrl
                };

                return View(passwordReset);
            }
            else
            {
                var passwordReset = new SudokuCollective.Api.Models.PasswordReset
                {
                    IsSuccess = result.IsSuccess
                };

                return View(passwordReset);
            }
        }


        /// <summary>
        /// The results of the default endpoint above link back to here upon completion, does not require a login.
        /// </summary>
        /// <returns>A redirect to the default password reset result view.</returns>
        /// <response code="200">A redirect to the default password reset result view.</response>
        /// <remarks>
        /// This is a default endpoint to handle password resets links back to this endpoint upon completion. The results
        /// of the default password reset operation are displayed here.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Result(
            SudokuCollective.Api.Models.PasswordReset passwordReset)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", passwordReset);
            }

            var app = (App)(await _appsService.GetAsync(passwordReset.AppId, passwordReset.UserId)).Payload[0];
            app.License = (await _appsService.GetLicenseAsync(app.Id, passwordReset.UserId)).License;

            var userResut = await _usersService.GetAsync(
                passwordReset.UserId, 
                app.License);

            if (userResut.IsSuccess)
            {
                var updatePasswordRequest = new UpdatePasswordRequest
                {
                    UserId = ((User)userResut.Payload[0]).Id,
                    NewPassword = passwordReset.NewPassword,
                    License = app.License
                };

                var updatePasswordResult = await _usersService.UpdatePasswordAsync(updatePasswordRequest);

                passwordReset.NewPassword = string.Empty;

                if (updatePasswordResult.IsSuccess)
                {
                    passwordReset.IsSuccess = updatePasswordResult.IsSuccess;
                    passwordReset.ErrorMessage = updatePasswordResult.Message;

                    return View(passwordReset);
                }
                else
                {
                    passwordReset.IsSuccess = updatePasswordResult.IsSuccess;
                    passwordReset.ErrorMessage = updatePasswordResult.Message;

                    return View(passwordReset);
                }
            }
            else
            {
                passwordReset.NewPassword = string.Empty;
                passwordReset.ErrorMessage = userResut.Message;

                return View(passwordReset);
            }
        }
    }
}
