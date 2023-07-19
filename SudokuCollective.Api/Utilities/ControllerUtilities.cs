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

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Api.Utilities
{
    internal static partial class ControllerUtilities
    {
        internal static ObjectResult ProcessException<T>(
            ControllerBase controller,
            IRequestService requestService,
            ILogger<T> logger,
            Exception e)
        {
            var isArgumentException = e.GetType() == typeof(ArgumentException) || 
                e.GetType() == typeof(ArgumentNullException);

            var result = new Result
            {
                IsSuccess = false,
                Message = isArgumentException ? 
                    ControllerMessages.StatusCode400(e.Message) : 
                    ControllerMessages.StatusCode500(e.Message)
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

        internal static ObjectResult ProcessTokenError(ControllerBase controller)
        {
            var result = new Result
            {
                IsSuccess = false,
                Message = ControllerMessages.InvalidTokenRequestMessage
            };

            return controller.StatusCode((int)HttpStatusCode.Forbidden, result);
        }

        // This regex matches the GuidRegexPattern located in SudokuCollective.Core.Validation.RegexValidators
        [GeneratedRegex("(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)")]
        internal static partial Regex GuidRegex();
    }
}
