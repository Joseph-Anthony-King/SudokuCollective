using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Messages
{
    internal static class SolutionsMessages
    {
        internal const string SolutionFoundMessage = "Solution was found.";
        internal const string SolutionNotFoundMessage = "Solution was not found.";
        internal const string SolutionsFoundMessage = "Solutions were found.";
        internal const string SolutionsNotFoundMessage = "Solutions were not found.";
        internal const string SolutionCreatedMessage = "Solution was created.";
        internal const string SolutionNotCreatedMessage = "Solution was not created.";
        internal const string SolutionUpdatedMessage = "Solution was updated.";
        internal const string SolutionNotUpdatedMessage = "Solution was not updated.";
        internal const string SolutionsAddedMessage = "Solutions were added.";
        internal const string SolutionsNotAddedMessage = "Solutions were not added.";
        internal const string SudokuSolutionFoundMessage = "Sudoku solution was found.";
        internal const string SudokuSolutionNotFoundMessage = "Sudoku solution was not found.";
        internal const string SolutionGeneratedMessage = "Solution was generated.";
        internal const string SolutionNotGeneratedMessage = "Solution was not generated.";
        internal static string LimitExceedsSolutionsLimitMessage(string limit) => string.Format("The amount of solutions requested, {0}, exceeds the service's 1,000 limit.", limit);
    }
}
