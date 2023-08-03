using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class AuthenticationResult : IAuthenticationResult
    {
        [JsonIgnore]
        IUserDTO IAuthenticationResult.User 
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

        public AuthenticationResult()
        {
            User = new UserDTO();
            Token = string.Empty;
        }

        public AuthenticationResult(
            IUserDTO user, 
            string token)
        {
            User = (UserDTO)user;
            Token = token;
        }

        public AuthenticationResult(
            UserDTO user, 
            string token)
        {
            User = user;
            Token = token;
        }
    }
}
