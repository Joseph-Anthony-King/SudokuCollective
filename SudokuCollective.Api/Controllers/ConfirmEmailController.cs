using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Api.Models;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Api.Controllers
{
    /// <summary>
    /// Confirm Email Controller
    /// </summary>
    /// <remarks>
    /// Confirm Email Controller Constructor
    /// </remarks>
    [Route("[controller]")]
    [Controller]
    public class ConfirmEmailController(
        IUsersService usersServ,
        ILogger<ConfirmEmailController> logger,
        IWebHostEnvironment environment) : Controller
    {
        private readonly IUsersService _usersService = usersServ;
        private readonly ILogger<ConfirmEmailController> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;

        /// <summary>
        /// A default endpoint to process confirm email requests, does not require a login.
        /// </summary>
        /// <returns>A redirect to the default email confirmation view.</returns>
        /// <response code="200">A redirect to the default email confirmation view.</response>
        /// <remarks>
        /// This is a default endpoint to handle email confirmations.  It is strongly recommended that you implement a 
        /// custom email confirmation action to handle such requests, this endpoint is simply a placeholder to handle
        /// such requests until you've implemented a custom action.  In order to implement such a request you have to 
        /// create it within your app (the details of which are dependent upon your apps particular framework) and 
        /// then enable it by setting the following app properties:
        ///
        /// ```DisableCustomUrls``` = ```false```
        ///
        /// ```UseCustomEmailConfirmationAction``` = ```true```
        ///
        /// ```CustomEmailConfirmationAction``` = the custom action you've created
        ///
        /// So if the url for your app is https://yourapp and the custom action is ```confirmEmail``` then your users 
        /// will be directed to the following:
        ///
        /// ```https://yourapp/confirmEmail/{token}```
        ///
        /// Please note the url is dependent on the release environment, so if your release environment is set to local 
        /// the requests will be directed to your local url and if set to staging the requests will be directed to your
        /// staging url, etc.
        ///
        /// Until such time as the above conditions are met such requests will continue to be directed to this default page.
        ///
        /// The token will be provided by the api and will be sent to the user in the confirmation email, along with a 
        /// link to either this default email confirmation action or to your custom email confirmation action. Once your
        /// custom email action is implemented it will submit the token and new password to the ConfirmEmail endpoint 
        /// in the user controller.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{token}")]
        public async Task<IActionResult> Index(string token)
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
                var confirmEmailModel = new ConfirmEmail
                {
                    ConfirmationType =((ConfirmEmailResult)result.Payload[0]).ConfirmationType,
                    UserName = ((ConfirmEmailResult)result.Payload[0]).UserName,
                    AppTitle = ((ConfirmEmailResult)result.Payload[0]).AppTitle,
                    Url = ((ConfirmEmailResult)result.Payload[0]).AppUrl,
                    Email = ((ConfirmEmailResult)result.Payload[0]).Email,
                    NewEmailAddressConfirmed = ((ConfirmEmailResult)result.Payload[0]).NewEmailAddressConfirmed != null && 
                        (bool)((ConfirmEmailResult)result.Payload[0]).NewEmailAddressConfirmed,
                    IsSuccess = result.IsSuccess
                };

                return View(confirmEmailModel);
            }
            else
            {
                if (_environment.IsDevelopment() == false)
                    result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions(result, _environment, _logger);

                var confirmEmailModel = new ConfirmEmail
                {
                    IsSuccess = result.IsSuccess,
                    ErrorMessage = result.Message,
                };

                return View(confirmEmailModel);
            }
        } 
    }
}