using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Requests
{
    public class UpdatePasswordRequest : IUpdatePasswordRequest
    {
        private string _license = string.Empty;
        private string _newPassword = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
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

        public UpdatePasswordRequest() {}

        public UpdatePasswordRequest(
            string license,
            int userId, 
            string newPassword)
        {
            License = license;
            UserId = userId;
            NewPassword = newPassword;
        }

        public static implicit operator JsonElement(UpdatePasswordRequest v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
