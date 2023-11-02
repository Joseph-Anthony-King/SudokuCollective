using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class LicensePayload : ILicensePayload
    {
        private string _localUrl = string.Empty;
        private string _qaUrl = string.Empty;
        private string _stagingUrl = string.Empty;
        private string _prodUrl = string.Empty;
        private string _sourceCodeUrl = string.Empty;
        private readonly UrlValidatedAttribute _urlValidator = new();
        
        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [Required, JsonPropertyName("ownerId")]
        public int OwnerId { get; set; }
        [JsonPropertyName("localUrl")]
        public string LocalUrl
        {
            get
            {
                return _localUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _urlValidator.IsValid(value))
                {
                    _localUrl = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // do nothing...
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUrl);
                }
            }
        }
        [JsonPropertyName("qaUrl")]
        public string QaUrl
        {
            get
            {
                return _qaUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _urlValidator.IsValid(value))
                {
                    _qaUrl = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // do nothing...
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUrl);
                }
            }
        }
        [JsonPropertyName("stagingUrl")]
        public string StagingUrl
        {
            get
            {
                return _stagingUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _urlValidator.IsValid(value))
                {
                    _stagingUrl = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // do nothing...
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUrl);
                }
            }
        }
        [JsonPropertyName("prodUrl")]
        public string ProdUrl
        {
            get
            {
                return _prodUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _urlValidator.IsValid(value))
                {
                    _prodUrl = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // do nothing...
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUrl);
                }
            }
        }
        [JsonPropertyName("sourceCodeUrl")]
        public string SourceCodeUrl
        {
            get
            {
                return _sourceCodeUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && _urlValidator.IsValid(value))
                {
                    _sourceCodeUrl = value;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // do nothing...
                }
                else
                {
                    throw new ArgumentException(AttributeMessages.InvalidUrl);
                }
            }
        }

        public LicensePayload()
        {
            Name = string.Empty;
            OwnerId = 0;
            LocalUrl = string.Empty;
            StagingUrl = string.Empty;
            QaUrl = string.Empty;
            ProdUrl = string.Empty;
            SourceCodeUrl = string.Empty;
        }

        public LicensePayload(
            string name,
            int ownerId,
            string localUrl,
            string stagingUrl,
            string qaUrl,
            string prodUrl,
            string sourceCodeUrl)
        {
            Name = name;
            OwnerId = ownerId;
            LocalUrl = localUrl;
            StagingUrl = stagingUrl;
            QaUrl = qaUrl;
            ProdUrl = prodUrl;
            SourceCodeUrl = sourceCodeUrl;
        }

        public static implicit operator JsonElement(LicensePayload v) => JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
