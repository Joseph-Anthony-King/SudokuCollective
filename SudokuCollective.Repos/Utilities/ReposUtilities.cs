using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Repos.Utilities
{
    internal static class ReposUtilities
    {
        internal static IRepositoryResponse ProcessException<T>(
            IRequestService requestService, 
            ILogger<T> logger, 
            IRepositoryResponse result, 
            Exception e,
            string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format(LoggerMessages.ErrorThrownMessage, e.Message);
            }

            result.IsSuccess = false;
            result.Exception = e;

            SudokuCollectiveLogger.LogError<T>(
                logger,
                LogsUtilities.GetRepoErrorEventId(), 
                message,
                e,
                (SudokuCollective.Logs.Models.Request)requestService.Get());

            return result;
        }
    }
}
