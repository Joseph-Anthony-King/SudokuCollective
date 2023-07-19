using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Data.Models.Payloads
{
    public class AppPayload : IAppPayload
    {
        private string _localUrl = string.Empty;
        private string _stagingUrl = string.Empty;
        private string _qaUrl = string.Empty;
        private string _prodUrl = string.Empty;
        private string _sourceCodeUrl = string.Empty;
        private readonly UrlValidatedAttribute _urlValidator = new();

        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [Required, JsonPropertyName("localUrl"), GuidValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
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
        [Required, JsonPropertyName("stagingUrl"), GuidValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
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
        [Required, JsonPropertyName("qaUrl"), GuidValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
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
        [Required, JsonPropertyName("prodUrl"), GuidValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
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
        [Required, JsonPropertyName("sourceCodeUrl"), GuidValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
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
        [Required, JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [Required, JsonPropertyName("environment")]
        public ReleaseEnvironment Environment { get; set; }
        [Required, JsonPropertyName("permitSuperUserAccess")]
        public bool PermitSuperUserAccess { get; set; }
        [Required, JsonPropertyName("permitCollectiveLogins")]
        public bool PermitCollectiveLogins { get; set; }
        [Required, JsonPropertyName("disableCustomUrls")]
        public bool DisableCustomUrls { get; set; }
        [Required, JsonPropertyName("customEmailConfirmationAction")]
        public string CustomEmailConfirmationAction { get; set; }
        [Required, JsonPropertyName("customPasswordResetAction")]
        public string CustomPasswordResetAction { get; set; }
        [Required, JsonPropertyName("useCustomSMTPServer")]
        public bool UseCustomSMTPServer { get; set; }
        [JsonIgnore]
        ISMTPServerSettings IAppPayload.SMTPServerSettings
        {
            get => SMTPServerSettings;
            set => SMTPServerSettings = (SMTPServerSettings)value;
        }
        [JsonPropertyName("smtpServerSettings")]
        public SMTPServerSettings SMTPServerSettings { get; set; }
        [Required, JsonPropertyName("timeFrame")]
        public TimeFrame TimeFrame { get; set; }
        [Required, JsonPropertyName("accessDuration")]
        public int AccessDuration { get; set; }
        public bool DisplayInGallery { get; set; }

        public AppPayload()
        {
            Name = string.Empty;
            IsActive = false;
            Environment = ReleaseEnvironment.NULL;
            PermitSuperUserAccess = false;
            PermitCollectiveLogins = false;
            DisableCustomUrls = false;
            CustomEmailConfirmationAction = string.Empty;
            CustomPasswordResetAction = string.Empty;
            UseCustomSMTPServer = false;
            TimeFrame = TimeFrame.NULL;
            AccessDuration = 0;
            DisplayInGallery = false;

            SMTPServerSettings = new SMTPServerSettings();
        }

        public AppPayload(
            string name, 
            string localUrl, 
            string stagingUrl, 
            string qaUrl, 
            string prodUrl, 
            string sourceCodeUrl,
            bool isActive, 
            int environment, 
            bool permitSuperUserAccess, 
            bool permitCollectiveLogins, 
            bool disableCustomUrls, 
            string customEmailConfirmationAction, 
            string customPasswordResetAction, 
            bool useCustomSMTPServer,
            int timeFrame, 
            int accessDuration,
            bool displayinGallery,
            SMTPServerSettings smtpServerSettings = null)
        {
            Name = name;
            LocalUrl = localUrl;
            StagingUrl = stagingUrl;
            QaUrl = qaUrl;
            ProdUrl = prodUrl;
            SourceCodeUrl = sourceCodeUrl;
            IsActive = isActive;
            Environment = (ReleaseEnvironment)environment;
            PermitSuperUserAccess = permitSuperUserAccess;
            PermitCollectiveLogins = permitCollectiveLogins;
            DisableCustomUrls = disableCustomUrls;
            CustomEmailConfirmationAction = customEmailConfirmationAction;
            CustomPasswordResetAction = customPasswordResetAction;
            UseCustomSMTPServer = useCustomSMTPServer;
            TimeFrame = (TimeFrame)timeFrame;
            AccessDuration = accessDuration;
            DisplayInGallery = displayinGallery;

            if (smtpServerSettings != null)
            {
                SMTPServerSettings = smtpServerSettings;
            }
            else
            {
                SMTPServerSettings = new SMTPServerSettings();
            }
        }

        public AppPayload(
            string name, 
            string localUrl, 
            string stagingUrl, 
            string qaUrl, 
            string prodUrl, 
            string sourceCodeUrl,
            bool isActive, 
            ReleaseEnvironment environment, 
            bool permitSuperUserAccess, 
            bool permitCollectiveLogins, 
            bool disableCustomUrls, 
            string customEmailConfirmationAction, 
            string customPasswordResetAction, 
            bool useCustomSMTPServer,
            TimeFrame timeFrame, 
            int accessDuration,
            bool displayinGallery,
            SMTPServerSettings smtpServerSettings = null)
        {
            Name = name;
            LocalUrl = localUrl;
            StagingUrl = stagingUrl;
            QaUrl = qaUrl;
            ProdUrl = prodUrl;
            SourceCodeUrl = sourceCodeUrl;
            IsActive = isActive;
            Environment = environment;
            PermitSuperUserAccess = permitSuperUserAccess;
            PermitCollectiveLogins = permitCollectiveLogins;
            DisableCustomUrls = disableCustomUrls;
            CustomEmailConfirmationAction = customEmailConfirmationAction;
            CustomPasswordResetAction = customPasswordResetAction;
            UseCustomSMTPServer = useCustomSMTPServer;
            TimeFrame = timeFrame;
            AccessDuration = accessDuration;
            DisplayInGallery = displayinGallery;

            if (smtpServerSettings != null)
            {
                SMTPServerSettings = smtpServerSettings;
            }
            else
            {
                SMTPServerSettings = new SMTPServerSettings();
            }
        }

        public static implicit operator JsonElement(AppPayload v) => JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
