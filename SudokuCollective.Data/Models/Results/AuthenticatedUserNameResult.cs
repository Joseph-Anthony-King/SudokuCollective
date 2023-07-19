using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;

namespace SudokuCollective.Data.Models.Results
{
    public class AuthenticatedUserNameResult : IAuthenticatedUserNameResult
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        public AuthenticatedUserNameResult()
        {
            UserName = string.Empty;
        }

        public AuthenticatedUserNameResult(string userName)
        {
            UserName = userName;
        }
    }
}
