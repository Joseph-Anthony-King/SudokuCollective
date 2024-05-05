using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Heroku;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using IResult = SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IResult;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Api.Utilities
{
    /// <summary>
    /// ControllerUtilities class
    /// </summary>
    /// <remarks>
    /// Class to define utilities methods for API controllers
    /// </remarks>
    public static partial class ControllerUtilities
    {
        /// <summary>
        /// ProcessException method
        /// </summary>
        /// <remarks>
        /// A method to handle exceptions thrown within controllers
        /// </remarks>
        /// <param name="controller"></param>
        /// <param name="requestService"></param>
        /// <param name="logger"></param>
        /// <param name="e"></param>
        /// <param name="environment"></param>
        /// <param name="httpMessageHandler"></param>
        public static async Task<ObjectResult> ProcessException<T>(
            ControllerBase controller,
            IRequestService requestService,
            ILogger<T> logger,
            Exception e,
            IWebHostEnvironment environment,
            HttpMessageHandler httpMessageHandler = null)
        {
            var isArgumentException = e.GetType() == typeof(ArgumentException) || 
                e.GetType() == typeof(ArgumentNullException);

            var message = e.Message;

            if (environment.IsDevelopment() == false &&
                message.Contains("redis server(s)"))
            {
                message = await InterceptHerokuIOExceptionsLogic<T>(message, environment, logger, httpMessageHandler);
            }

            var result = new Result
            {
                IsSuccess = false,
                Message = isArgumentException ? 
                    ControllerMessages.StatusCode400(message) : 
                    ControllerMessages.StatusCode500(message)
            };

            SudokuCollectiveLogger.LogError<T>(
                logger,
                LogsUtilities.GetControllerErrorEventId(),
                result.Message,
                e,
                (SudokuCollective.Logs.Models.Request)requestService.Get());

            return isArgumentException ? 
                controller.BadRequest(result) : 
                controller.StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        /// <summary>
        /// ProcessTokenError method
        /// </summary>
        /// <remarks>
        /// A method to handle token exceptions thrown within controllers
        /// </remarks>
        /// <param name="controller"></param>
        public static ObjectResult ProcessTokenError(ControllerBase controller)
        {
            var result = new Result
            {
                IsSuccess = false,
                Message = ControllerMessages.InvalidTokenMessage
            };

            return controller.StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// InterceptHerokuIOExceptions method
        /// </summary>
        /// <remarks>
        /// A method to check the result of type IResult for Heroku IO Exceptions
        /// </remarks>
        /// <param name="result"></param>
        /// <param name="environment"></param>
        /// <param name="logger"></param>
        /// <param name="httpMessageHandler"></param>
        public static async Task<IResult> InterceptHerokuIOExceptions<T>(
            IResult result, 
            IWebHostEnvironment environment,
            ILogger<T> logger,
            HttpMessageHandler httpMessageHandler = null)
        {
            result.Message = await InterceptHerokuIOExceptionsLogic(result.Message, environment, logger, httpMessageHandler);

            return result;
        }

        /// <summary>
        /// InterceptHerokuIOExceptions method
        /// </summary>
        /// <remarks>
        /// A method to check the result of type ILicenseResult for Heroku IO Exceptions
        /// </remarks>
        /// <param name="result"></param>
        /// <param name="environment"></param>
        /// <param name="logger"></param>
        /// <param name="httpMessageHandler"></param>
        public static async Task<ILicenseResult> InterceptHerokuIOExceptions<T>(
            ILicenseResult result,
            IWebHostEnvironment environment,
            ILogger<T> logger,
            HttpMessageHandler httpMessageHandler = null)
        {
            result.Message = await InterceptHerokuIOExceptionsLogic(result.Message, environment, logger, httpMessageHandler);

            return result;
        }

        /// <summary>
        /// GuidRegex regex
        /// </summary>
        /// <remarks>
        /// This regex matches the GuidRegexPattern located in SudokuCollective.Core.Validation.RegexValidators
        /// </remarks> 
        [GeneratedRegex("(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)")]
        public static partial Regex GuidRegex();

        private static async Task<string> InterceptHerokuIOExceptionsLogic<T>(
            string message, 
            IWebHostEnvironment environment, 
            ILogger<T> logger,
            HttpMessageHandler httpMessageHandler = null)
        {
            if (message.Contains("redis server(s)"))
            {
                var isStaging = environment.IsStaging() == true && environment.IsProduction() == false;

                var proxyResponse = await HerokuProxy.UpdateHerokuRedisConnectionStringAsync(isStaging, logger, httpMessageHandler);

                message = proxyResponse ?
                    "It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request." :
                    "It was not possible to connect to the redis server, the attempt to restart the redis server connections failed. Please resubmit your request.";
            }

            return message;
        }
    }
}
