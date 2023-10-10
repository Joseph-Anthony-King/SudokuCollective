using System;
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
        [JsonPropertyName("tokenExpirationDate")]
        public DateTime TokenExpirationDate { get; set; }

        public AuthenticationResult()
        {
            User = new UserDTO();
            Token = string.Empty;
            TokenExpirationDate = DateTime.MinValue;
        }

        public AuthenticationResult(
            IUserDTO user, 
            string token,
            DateTime tokenExpirationDate)
        {
            User = (UserDTO)user;
            Token = token;
            TokenExpirationDate = tokenExpirationDate;
        }

        public AuthenticationResult(
            UserDTO user, 
            string token,
            DateTime tokenExpirationDate)
        {
            User = user;
            Token = token;
            TokenExpirationDate = tokenExpirationDate;
        }
    }
}
