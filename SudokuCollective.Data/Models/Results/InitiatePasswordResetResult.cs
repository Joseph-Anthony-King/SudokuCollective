using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Data.Models.Results
{
    public class InitiatePasswordResetResult : IInitiatePasswordResetResult
    {
        [JsonIgnore]
        IApp IInitiatePasswordResetResult.App
        {
            get
            {
                return App;
            }
            set
            {
                App = (App)value;
            }
        }
        public App App { get; set; }
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

        public InitiatePasswordResetResult()
        {
            App = new App();
            User = new UserDTO();
            ConfirmationEmailSuccessfullySent = null;
            Token = string.Empty;
        }

        public InitiatePasswordResetResult(
            IApp app, 
            IUserDTO user, 
            bool? confirmationEmailSuccessfullySent, 
            string token)
        {
            App = (App)app;
            User = (UserDTO)user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }

        public InitiatePasswordResetResult(
            App app, 
            UserDTO user, 
            bool? confirmationEmailSuccessfullySent, 
            string token)
        {
            App = app;
            User = user;
            ConfirmationEmailSuccessfullySent = confirmationEmailSuccessfullySent;
            Token = token;
        }
    }
}
