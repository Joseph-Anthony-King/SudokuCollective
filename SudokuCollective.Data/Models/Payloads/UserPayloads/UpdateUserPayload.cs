using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class UpdateUserPayload : IUpdateUserPayload
    {
        private string _userName = string.Empty;
        private string _email = string.Empty;
        private readonly UserNameValidatedAttribute _usernameValidatedAttribute = new();
        private readonly EmailValidatedAttribute _emailValidator = new();

        [Required, UserNameValidated(ErrorMessage = AttributeMessages.InvalidUserName), JsonPropertyName("userName")]
        public string UserName 
        {
            get
            {
                return _userName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                if (_usernameValidatedAttribute.IsValid(value))
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
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                if (_emailValidator.IsValid(value))
                {
                    _email = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidEmail);
                }
            }
        }

        public UpdateUserPayload()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            NickName = string.Empty;
        }

        public UpdateUserPayload(
            string userName, 
            string firstName, 
            string lastName, 
            string nickName, 
            string email)
        {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            NickName = nickName;
            Email = email;
        }

        public static implicit operator JsonElement(UpdateUserPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
