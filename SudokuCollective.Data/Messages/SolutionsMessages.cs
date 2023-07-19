using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal static class SolutionsMessages
    {
        internal const string SolutionFoundMessage = "Solution found";
        internal const string SolutionNotFoundMessage = "Solution not found";
        internal const string SolutionsFoundMessage = "Solutions found";
        internal const string SolutionsNotFoundMessage = "Solutions not found";
        internal const string SolutionCreatedMessage = "Solution created";
        internal const string SolutionNotCreatedMessage = "Solution not created";
        internal const string SolutionUpdatedMessage = "Solution updated";
        internal const string SolutionNotUpdatedMessage = "Solution not updated";
        internal const string SolutionsAddedMessage = "Solutions added";
        internal const string SolutionsNotAddedMessage = "Solutions not added";
        internal const string SudokuSolutionFoundMessage = "Sudoku solution found";
        internal const string SudokuSolutionNotFoundMessage = "Sudoku solution not found";
        internal const string SolutionGeneratedMessage = "Solution generated";
        internal const string SolutionNotGeneratedMessage = "Solution not generated";
        internal static string LimitExceedsSolutionsLimitMessage(string limit) => string.Format("The amount of solutions requested, {0}, Exceeds the service's 1,000 limit", limit);
    }
}
