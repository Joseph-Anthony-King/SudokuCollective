using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
namespace SudokuCollective.Data.Messages
{
    internal static class GamesMessages
    {
        internal const string GameFoundMessage = "Game was found.";
        internal const string GameNotFoundMessage = "Game was not found.";
        internal const string GamesFoundMessage = "Games were found.";
        internal const string GamesNotFoundMessage = "Games were not found.";
        internal const string GameCreatedMessage = "Game was created.";
        internal const string GameNotCreatedMessage = "Game was not created.";
        internal const string GameUpdatedMessage = "Game was updated.";
        internal const string GameNotUpdatedMessage = "Game was not updated.";
        internal const string GameDeletedMessage = "Game was deleted.";
        internal const string GameNotDeletedMessage = "Game was not deleted.";
        internal const string GameSolvedMessage = "Game was solved.";
        internal const string GameNotSolvedMessage = "Game was not solved.";
        internal const string DifficultyLevelIsRequiredMessage = "Difficulty level is required.";
        internal const string CheckRequestNotValidMessage = "Check request is not a valid message.";
        internal const string CreateGameJobScheduled = "Create game job {0} scheduled.";
    }
}
