using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
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
        public static  ObjectResult ProcessException<T>(
            ControllerBase controller,
            IRequestService requestService,
            ILogger<T> logger,
            Exception e)
        {
            var isArgumentException = e.GetType() == typeof(ArgumentException) || 
                e.GetType() == typeof(ArgumentNullException);

            var message = e.Message;

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
        /// GuidRegex regex
        /// </summary>
        /// <remarks>
        /// This regex matches the GuidRegexPattern located in SudokuCollective.Core.Validation.RegexValidators
        /// </remarks> 
        [GeneratedRegex("(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)")]
        public static partial Regex GuidRegex();
    }
}
