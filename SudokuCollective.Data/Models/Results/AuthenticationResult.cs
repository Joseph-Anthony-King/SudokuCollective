using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class AuthenticationResult : IAuthenticationResult
    {
        [JsonIgnore]
        ITranslatedUser IAuthenticationResult.User 
        {
            get => (ITranslatedUser)User;
            set 
            {
                User = (TranslatedUser)value;
            }
        }
        [JsonPropertyName("user")]
        public TranslatedUser User { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }

        public AuthenticationResult()
        {
            User = new TranslatedUser();
            Token = string.Empty;
        }

        public AuthenticationResult(
            ITranslatedUser user, 
            string token)
        {
            User = (TranslatedUser)user;
            Token = token;
        }

        public AuthenticationResult(
            TranslatedUser user, 
            string token)
        {
            User = user;
            Token = token;
        }
    }
}
