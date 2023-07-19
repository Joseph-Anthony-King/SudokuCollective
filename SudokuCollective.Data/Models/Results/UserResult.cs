using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class UserResult : IUserResult
    {
        [JsonIgnore]
        IUser IUserResult.User
        {
            get
            {
                return User;
            }
            set
            {
                User = (User)value;
            }
        }
        [JsonPropertyName("user")]
        public User User { get; set; }
        [JsonPropertyName("confirmationEmailSuccessfullySent")]
        public bool? ConfirmationEmailSuccessfullySent { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }

        public UserResult()
        {
            User = new User();
            ConfirmationEmailSuccessfullySent = null;
            Token = string.Empty;
        }

        public UserResult(IUser user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = (User)user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }

        public UserResult(User user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }
    }
}
