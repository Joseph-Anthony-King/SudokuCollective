using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
namespace SudokuCollective.Data.Messages
{
    internal static class GamesMessages
    {
        internal const string GameFoundMessage = "Game found";
        internal const string GameNotFoundMessage = "Game not found";
        internal const string GamesFoundMessage = "Games found";
        internal const string GamesNotFoundMessage = "Games not found";
        internal const string GameCreatedMessage = "Game created";
        internal const string GameNotCreatedMessage = "Game not created";
        internal const string GameUpdatedMessage = "Game updated";
        internal const string GameNotUpdatedMessage = "Game not updated";
        internal const string GameDeletedMessage = "Game deleted";
        internal const string GameNotDeletedMessage = "Game not deleted";
        internal const string GameSolvedMessage = "Game solved";
        internal const string GameNotSolvedMessage = "Game not solved";
        internal const string DifficultyLevelIsRequiredMessage = "Difficulty level is required";
        internal const string CheckRequestNotValidMessage = "Check request not valid message";
    }
}
