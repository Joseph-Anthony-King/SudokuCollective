using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class UserResult : IUserResult
    {
        [JsonIgnore]
        IUserDTO IUserResult.User
        {
            get
            {
                return User;
            }
            set
            {
                User = (UserDTO)value;
            }
        }
        [JsonPropertyName("user")]
        public UserDTO User { get; set; }
        [JsonPropertyName("confirmationEmailSuccessfullySent")]
        public bool? ConfirmationEmailSuccessfullySent { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }

        public UserResult()
        {
            User = new UserDTO();
            ConfirmationEmailSuccessfullySent = null;
            Token = string.Empty;
        }

        public UserResult(IUserDTO user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = (UserDTO)user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }

        public UserResult(UserDTO user, bool? confirmationEmailSuccessfullySent, string token)
        {
            User = user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }
    }
}
