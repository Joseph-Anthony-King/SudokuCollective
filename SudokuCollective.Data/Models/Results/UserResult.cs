using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class UserResult : IUserResult
    {
        [JsonIgnore]
        ITranslatedUser IUserResult.User
        {
            get
            {
                return User;
            }
            set
            {
                User = (TranslatedUser)value;
            }
        }
        [JsonPropertyName("user")]
        public TranslatedUser User { get; set; }
        [JsonPropertyName("confirmationEmailSuccessfullySent")]
        public bool? ConfirmationEmailSuccessfullySent { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }

        public UserResult()
        {
            User = new TranslatedUser();
            ConfirmationEmailSuccessfullySent = null;
            Token = string.Empty;
        }

        public UserResult(ITranslatedUser user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = (TranslatedUser)user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }

        public UserResult(TranslatedUser user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }
    }
}
