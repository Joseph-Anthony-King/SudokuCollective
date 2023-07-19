using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Requests
{
    public class RequestPasswordResetRequest : IRequestPasswordResetRequest
    {
        private string _license = string.Empty;
        private string _email = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
        private readonly EmailValidatedAttribute _emailRegexAttribute = new();

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
        [Required, EmailValidated(ErrorMessage = AttributeMessages.InvalidEmail), JsonPropertyName("email")]
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _emailRegexAttribute.IsValid(value))
                {
                    _email = value;
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidEmail);
                }
            }
        }

        public RequestPasswordResetRequest() { }

        public RequestPasswordResetRequest(string license, string email)
        {
            License = license;
            Email = email;
        }

        public static implicit operator JsonElement(RequestPasswordResetRequest v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
