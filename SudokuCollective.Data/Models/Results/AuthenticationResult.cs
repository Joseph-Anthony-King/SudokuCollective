using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class AuthenticationResult : IAuthenticationResult
    {
        [JsonIgnore]
        IAuthenticatedUser IAuthenticationResult.User 
        {
            get => (IAuthenticatedUser)User;
            set 
            {
                User = (AuthenticatedUser)value;
            }
        }
        [JsonPropertyName("user")]
        public AuthenticatedUser User { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }

        public AuthenticationResult()
        {
            User = new AuthenticatedUser();
            Token = string.Empty;
        }

        public AuthenticationResult(
            IAuthenticatedUser user, 
            string token)
        {
            User = (AuthenticatedUser)user;
            Token = token;
        }

        public AuthenticationResult(
            AuthenticatedUser user, 
            string token)
        {
            User = user;
            Token = token;
        }
    }
}
