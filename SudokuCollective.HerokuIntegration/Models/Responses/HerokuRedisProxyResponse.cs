using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
namespace SudokuCollective.HerokuIntegration.Models.Responses
{
    internal class HerokuRedisProxyResponse
    {
        internal bool IsSuccessful { get; set; }
        internal string Message { get; set; }

        internal HerokuRedisProxyResponse()
        {
            IsSuccessful = false;
            Message = string.Empty;
        }
    }
}
