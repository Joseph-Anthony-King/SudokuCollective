using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Requests
{
    public class ResetPasswordRequest : IResetPasswordPayload
    {
        private string _newPassword = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
        private readonly PasswordValidatedAttribute _passwordValidator = new();

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

        public ResetPasswordRequest() {}

        public ResetPasswordRequest(string newPassword)
        {
            NewPassword = newPassword;
        }

        public static implicit operator JsonElement(ResetPasswordRequest v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
