using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class PasswordResetPayload : IPasswordResetPayload
    {
        private string _newPassword = string.Empty;
        private readonly PasswordValidatedAttribute _passwordValidator = new();

        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, PasswordValidated(ErrorMessage = AttributeMessages.InvalidPassword), JsonPropertyName("newPassword")]
        public string NewPassword
        {
            get
            {
                return _newPassword;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _passwordValidator.IsValid(value))
                {
                    _newPassword = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidPassword);
                }
            }
        }

        public PasswordResetPayload()
        {
            UserId = 0;
        }

        public PasswordResetPayload(int userId, string newPassword)
        {
            UserId = userId;
            NewPassword = newPassword;
        }

        public static implicit operator JsonElement(PasswordResetPayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
