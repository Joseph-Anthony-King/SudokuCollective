using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SudokuCollective.Logs.Models;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
[assembly:InternalsVisibleTo("SudokuCollective.Data")]
[assembly:InternalsVisibleTo("SudokuCollective.Repos")]
[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Logs
{
    internal static class SudokuCollectiveLogger
    {
        internal static void LogInformation<T>(
            ILogger<T> logger,
            EventId eventId,
            string message,
            Request request = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException((nameof(message)));

            if (request == null)
            {
                request = new Request();
            }

            logger.LogInformation(eventId, "Message: {0}, License: {1}, AppId: {2}, RequestorId: {3}", message, request.License, request.AppId, request.RequestorId);
        }

        internal static void LogWarning<T>(
            ILogger<T> logger,
            EventId eventId,
            string message,
            Request request = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException((nameof(message)));

            if (request == null)
            {
                request = new Request();
            }

            logger.LogWarning(eventId, "Message: {0}, License: {1}, AppId: {2}, RequestorId: {3}", message, request.License, request.AppId, request.RequestorId);
        }

        internal static void LogError<T>(
            ILogger<T> logger, 
            EventId eventId, 
            string message,
            Exception exception,
            Request request = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException((nameof(message)));

            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (request == null)
            {
                request = new Request();
            }

            logger.LogError(eventId, exception, "Message: {0}, License: {1}, AppId: {2}, RequestorId: {3}", message, request.License, request.AppId, request.RequestorId);
        }
    }
}
