using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Requests
{
    public class SignupRequest : ISignupRequest
    {
        private string _license = string.Empty;
        private string _userName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
        private readonly UserNameValidatedAttribute _userNameValidator = new();
        private readonly EmailValidatedAttribute _emailValidator = new();
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
                if (!string.IsNullOrEmpty(value) && _userNameValidator.IsValid(value))
                {
                    _userName = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUserName);
                }
            }
        }
        [Required, JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [Required, JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("nickName")]
        public string NickName { get; set; }
        [Required, EmailValidated(ErrorMessage = AttributeMessages.InvalidEmail), JsonPropertyName("email")]
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _emailValidator.IsValid(value))
                {
                    _email = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidEmail);
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

        public SignupRequest()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            NickName = string.Empty;
        }

        public SignupRequest(
            string license,
            string userName, 
            string firstName, 
            string lastName, 
            string nickName, 
            string email, 
            string password)
        {
            License = license;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            NickName = nickName;
            Email = email;
            Password = password;
        }

        public static implicit operator JsonElement(SignupRequest v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
