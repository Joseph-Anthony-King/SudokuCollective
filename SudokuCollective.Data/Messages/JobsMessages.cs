using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal static class JobsMessages
    {
        internal const string JobIdIsInvalid = "Job id {0} is invalid.";
        internal const string JobIsCompletedWithStatus = "Job {0} is completed with status {1}.";
        internal const string JobIsNotCompletedWithStatus = "Job {0} is not completed with status {1}.";
        internal const string JobRetrievalFailedDueToJobState = "Job retrieval for job {0} failed due to job state {1}.";
    }
}
