using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SudokuCollective.Api")]
namespace SudokuCollective.Data.Messages
{
    internal static class ControllerMessages
    {
        internal const string HelloWorld = "Hello World from Sudoku Collective!";
        internal const string ExpiredTokenMessage = "Status Code 401: The authorization token has expired, please sign in again";
        internal const string InvalidTokenMessage = "Status Code 403: Invalid request on this authorization token";
        internal const string NotOwnerMessage = "Status Code 403: You are not the owner of this app";
        internal const string IdIncorrectMessage = "Status Code 400: Id is incorrect";
        internal const string IdCannotBeZeroMessage = "Id cannot be zero";
        internal const string UserIdCannotBeZeroMessage = "User Id cannot be zero";

        internal static string StatusCode200(string serviceMessage) => string.Format("Status Code 200: {0}", serviceMessage);

        internal static string StatusCode201(string serviceMessage) => string.Format("Status Code 201: {0}", serviceMessage);

        internal static string StatusCode400(string serviceMessage) => string.Format("Status Code 400: {0}", serviceMessage);

        internal static string StatusCode404(string serviceMessage) => string.Format("Status Code 404: {0}", serviceMessage);

        internal static string StatusCode500(string serviceMessage) => string.Format("Status Code 500: {0}", serviceMessage);

        internal static string Echo(string param) => string.Format("You Submitted: {0}", param);
    }
}
