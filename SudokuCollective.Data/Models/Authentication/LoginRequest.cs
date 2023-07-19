using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.LoginModels;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Authentication
{
    public class LoginRequest : ILoginRequest
    {
        private string _license = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
        private readonly UserNameValidatedAttribute _userNameValidatedAttribute = new();
        private readonly PasswordValidatedAttribute _passwordValidator = new();

        [Required, GuidValidated(ErrorMessage = AttributeMessages.InvalidLicense), JsonPropertyName("license")]
        public string License
        {
            get
            {
                return _license;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _guidValidator.IsValid(value))
                {
                    _license = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidLicense);
                }
            }
        }

        [Required, UserNameValidated(ErrorMessage = AttributeMessages.InvalidUserName), JsonPropertyName("userName")]
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _userNameValidatedAttribute.IsValid(value))
                {
                    _userName = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUserName);
                }
            }
        }

        [Required, PasswordValidated(ErrorMessage = AttributeMessages.InvalidPassword), JsonPropertyName("password")]
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _passwordValidator.IsValid(value))
                {
                    _password = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidPassword);
                }
            }
        }

        public LoginRequest() { }

        public LoginRequest(string license, string userName, string password)
        {
            License = license;
            UserName = userName;
            Password = password;
        }
    }
}
