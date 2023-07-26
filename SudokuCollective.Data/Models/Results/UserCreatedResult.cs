using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class UserCreatedResult : IUserCreatedResult
    {
        [JsonIgnore]
        ITranslatedUser IUserCreatedResult.User 
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
        [JsonPropertyName("emailConfirmationSent")]
        public bool EmailConfirmationSent { get; set; }

        public UserCreatedResult()
        {
            User = new TranslatedUser();
            Token = string.Empty;
            EmailConfirmationSent = false;
        }

        public UserCreatedResult(
            ITranslatedUser user, 
            string token,
            bool emailConfirmationSent)
        {
            User = (TranslatedUser)user;
            Token = token;
            EmailConfirmationSent = emailConfirmationSent;
        }

        public UserCreatedResult(
            TranslatedUser user, 
            string token,
            bool emailConfirmationSent)
        {
            User = user;
            Token = token;
            EmailConfirmationSent = emailConfirmationSent;
        }
    }
}
