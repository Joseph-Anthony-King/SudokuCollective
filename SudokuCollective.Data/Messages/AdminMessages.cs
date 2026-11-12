namespace SudokuCollective.Data.Messages
{
    internal static class AdminMessages
    {
        internal const string AdminSummaryFoundMessage = "Admin summary was found.";
        internal const string AdminSummaryNotFoundMessage = "Admin summary not was found.";
        internal const string MustBeSuperUserMessage = "Requestor must be a super user to complete this request.";
        internal const string CacheWasFlushedMessage = "Cache was flushed successfully.";
        internal const string CacheWasNotFlushedMessage = "Cache was not flushed successfully.";
    }
}
