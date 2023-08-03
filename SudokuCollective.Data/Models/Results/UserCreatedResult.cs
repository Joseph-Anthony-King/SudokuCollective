using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class UserCreatedResult : IUserCreatedResult
    {
        [JsonIgnore]
        IUserDTO IUserCreatedResult.User 
        {
            get => (IUserDTO)User;
            set 
            {
                User = (UserDTO)value;
            }
        }
        [JsonPropertyName("user")]
        public UserDTO User { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("emailConfirmationSent")]
        public bool EmailConfirmationSent { get; set; }

        public UserCreatedResult()
        {
            User = new UserDTO();
            Token = string.Empty;
            EmailConfirmationSent = false;
        }

        public UserCreatedResult(
            IUserDTO user, 
            string token,
            bool emailConfirmationSent)
        {
            User = (UserDTO)user;
            Token = token;
            EmailConfirmationSent = emailConfirmationSent;
        }

        public UserCreatedResult(
            UserDTO user, 
            string token,
            bool emailConfirmationSent)
        {
            User = user;
            Token = token;
            EmailConfirmationSent = emailConfirmationSent;
        }
    }
}
