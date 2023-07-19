using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
[assembly: InternalsVisibleTo("SudokuCollective.Data")]
[assembly: InternalsVisibleTo("SudokuCollective.Repos")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Logs.Utilities
{
    internal static class LogsUtilities
    {
        internal static EventId GetControllerLogEventId()
        {
            return new EventId(100, "Controller Event");
        }

        internal static EventId GetControllerWarningEventId()
        {
            return new EventId(101, "Controller Event Warning");
        }

        internal static EventId GetControllerErrorEventId()
        {
            return new EventId(102, "Controller Event Error");
        }
        
        internal static EventId GetServiceLogEventId()
        {
            return new EventId(200, "Service Event");
        }

        internal static EventId GetServiceWarningEventId()
        {
            return new EventId(201, "Service Event Warning");
        }

        internal static EventId GetServiceErrorEventId()
        {
            return new EventId(202, "Service Event Error");
        }

        internal static EventId GetSMTPEventId()
        {
            return new EventId(203, "SMTP Event");
        }

        internal static EventId GetRepoLogEventId()
        {
            return new EventId(300, "Repository Event");
        }

        internal static EventId GetRepoWarningEventId()
        {
            return new EventId(301, "Repository Event Warning");
        }

        internal static EventId GetRepoErrorEventId()
        {
            return new EventId(302, "Repository Event Error");
        }

        internal static EventId GetHangfireEventId()
        {
            return new EventId(400, "Hangfire Event");
        }

        internal static EventId GetHangfireWarningEventId()
        {
            return new EventId(401, "Hangfire Event Warning");
        }

        internal static EventId GetHangfireErrorEventId()
        {
            return new EventId(402, "Hangfire Event Error");
        }
    }
}
