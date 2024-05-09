using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Core.Models
{
    public class App : IApp
    {
        #region Fields
        private string _license = string.Empty;
        private string _localUrl = string.Empty;
        private string _stagingUrl = string.Empty;
        private string _qaUrl = string.Empty;
        private string _prodUrl = string.Empty;
        private string _sourceCodeUrl = string.Empty;
        private TimeFrame _timeFrame = TimeFrame.NULL;
        private int _accessDuration = 0;
        private readonly GuidValidatedAttribute _guidValidator = new();
        private readonly UrlValidatedAttribute _urlValidator = new();
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [IgnoreDataMember]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [GuidValidated(ErrorMessage = AttributeMessages.InvalidLicense)]
        public string License
        {
            get => _license;
            set => _license = CoreUtilities.SetNullableField(
                value,
                _guidValidator,
                AttributeMessages.InvalidLicense);
        }
        [Required, JsonPropertyName("ownerId")]
        public int OwnerId { get; set; }
        [JsonIgnore]
        public string CreatedBy { get; set; }
        [JsonPropertyName("localUrl"), UrlValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
        public string LocalUrl
        {
            get => _localUrl;
            set => _localUrl = CoreUtilities.SetNullableField(
                value,
                _urlValidator,
                AttributeMessages.InvalidUrl);
        }
        [JsonPropertyName("qaUrl"), UrlValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
        public string QaUrl
        {
            get => _qaUrl;
            set => _qaUrl = CoreUtilities.SetNullableField(
                value,
                _urlValidator,
                AttributeMessages.InvalidUrl);
        }
        [JsonPropertyName("stagingUrl"), UrlValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
        public string StagingUrl
        {
            get => _stagingUrl;
            set => _stagingUrl = CoreUtilities.SetNullableField(
                value,
                _urlValidator,
                AttributeMessages.InvalidUrl);
        }
        [JsonPropertyName("prodUrl"), UrlValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
        public string ProdUrl
        {
            get => _prodUrl;
            set => _prodUrl = CoreUtilities.SetNullableField(
                value,
                _urlValidator,
                AttributeMessages.InvalidUrl);
        }
        [JsonPropertyName("sourceCodeUrl"), UrlValidated(ErrorMessage = AttributeMessages.InvalidUrl)]
        public string SourceCodeUrl
        {
            get => _sourceCodeUrl;
            set => _sourceCodeUrl = CoreUtilities.SetNullableField(
                value,
                _urlValidator,
                AttributeMessages.InvalidUrl);
        }
        [Required, JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [Required, JsonPropertyName("environment")]
        public ReleaseEnvironment Environment { get; set; }
        [Required, JsonPropertyName("permitSuperUserAccess")]
        public bool PermitSuperUserAccess { get; set; }
        [Required, JsonPropertyName("permitCollectiveLogins")]
        public bool PermitCollectiveLogins { get; set; }
        [JsonIgnore]
        public bool UseCustomEmailConfirmationAction
        {
            get => GetUseCustomEmailConfirmationAction();
        }
        [JsonIgnore]
        public bool UseCustomPasswordResetAction
        {
            get => GetUseCustomPasswordResetAction();
        }
        [JsonPropertyName("disableCustomUrls")]
        public bool DisableCustomUrls { get; set; }
        [JsonPropertyName("customEmailConfirmationAction")]
        public string CustomEmailConfirmationAction { get; set; }
        [JsonPropertyName("customPasswordResetAction")]
        public string CustomPasswordResetAction { get; set; }
        [JsonPropertyName("useCustomSMTPServer")]
        public bool UseCustomSMTPServer { get; set; }
        [JsonIgnore]
        ISMTPServerSettings IApp.SMTPServerSettings
        {
            get => SMTPServerSettings;
            set => SMTPServerSettings = (SMTPServerSettings)value;
        }
        [JsonPropertyName("smtpServerSettings"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SMTPServerSettings SMTPServerSettings { get; set; }
        [Required, JsonPropertyName("userCount")]
        public int UserCount
        {
            get => GetUserCount();
        }
        [JsonPropertyName("timeFrame")]
        public TimeFrame TimeFrame
        {
            get => _timeFrame;
            set => SetTimeFrame(value);
        }
        [JsonPropertyName("accessDuration")]
        public int AccessDuration
        {
            get => _accessDuration;
            set => SetAccessDuration(value);
        }
        [Required, JsonPropertyName("displayInGallery")]
        public bool DisplayInGallery { get; set; }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        [Required, JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }
        [JsonIgnore]
        ICollection<IUserApp> IApp.UserApps
        {
            get => UserApps.ConvertAll(u => (IUserApp)u);
            set => UserApps = value.ToList().ConvertAll(u => (UserApp)u);
        }
        [JsonIgnore]
        public virtual List<UserApp> UserApps { get; set; }
        [JsonIgnore]
        ICollection<IUserDTO> IApp.Users
        {
            get => Users.ConvertAll(u => (IUserDTO)u);
            set => Users = value.ToList().ConvertAll(u => (UserDTO)u);
        }
        [Required, JsonPropertyName("users"), JsonConverter(typeof(IDomainEntityListConverter<List<UserDTO>>))]
        public virtual List<UserDTO> Users { get; set; }
        #endregion

        #region Constructors
        public App()
        {
            Id = 0;
            Name = string.Empty;
            OwnerId = 0;
            CreatedBy = string.Empty;
            DateCreated = DateTime.UtcNow;
            IsActive = false;
            PermitSuperUserAccess = false;
            PermitCollectiveLogins = false;
            Environment = ReleaseEnvironment.LOCAL;
            DisableCustomUrls = true;
            CustomEmailConfirmationAction = string.Empty;
            CustomPasswordResetAction = string.Empty;
            UseCustomSMTPServer = false;
            SMTPServerSettings = new SMTPServerSettings
            {
                App = this
            };
            UserApps = [];
            Users = [];
            TimeFrame = TimeFrame.DAYS;
            AccessDuration = 1;
            DisplayInGallery = false;
        }

        public App(
                string name,
                string license,
                int ownerId,
                string ownerUserName,
                string localUrl,
                string qaUrl,
                string stagingUrl,
                string prodUrl,
                string sourceCodeUrl,
                bool useCustomSMTPServer = false,
                string smtpServer = null,
                int? port = null,
                string smtpUserName = null,
                string smtpPassword = null,
                string smtpFromEmail = null) : this()
        {
            Name = name;
            License = license;
            OwnerId = ownerId;
            CreatedBy = ownerUserName;
            DateCreated = DateTime.UtcNow;
            LocalUrl = localUrl;
            QaUrl = qaUrl;
            StagingUrl = stagingUrl;
            ProdUrl = prodUrl;
            SourceCodeUrl = sourceCodeUrl;
            IsActive = true;
            UseCustomSMTPServer = useCustomSMTPServer;
            if (useCustomSMTPServer == true &&
                smtpServer != null &&
                port != null &&
                smtpUserName != null &&
                smtpPassword != null &&
                smtpFromEmail != null)
            {
                SMTPServerSettings = new SMTPServerSettings()
                {
                    SmtpServer = smtpServer,
                    Port = (int)port,
                    UserName = smtpUserName,
                    Password = smtpPassword,
                    FromEmail = smtpFromEmail,
                    AppId = Id,
                    App = this,
                };
            }
        }

        [JsonConstructor]
        public App(
            int id,
            string name,
            string license,
            int ownerId,
            string createdBy,
            string localUrl,
            string qaUrl,
            string stagingUrl,
            string prodUrl,
            string sourceCodeUrl,
            bool isActive,
            bool permitSuperUserAccess,
            bool permitCollectiveLogins,
            ReleaseEnvironment environment,
            bool disableCustomUrls,
            string customEmailConfirmationAction,
            string customPasswordResetAction,
            TimeFrame timeFrame,
            int accessDuration,
            bool displayInGallery,
            DateTime dateCreated,
            DateTime dateUpdated,
            bool useCustomSMTPServer = false
        )
        {
            Id = id;
            Name = name;
            License = license;
            OwnerId = ownerId;
            CreatedBy = createdBy;
            LocalUrl = localUrl;
            QaUrl = qaUrl;
            StagingUrl = stagingUrl;
            ProdUrl = prodUrl;
            SourceCodeUrl = sourceCodeUrl;
            IsActive = isActive;
            PermitSuperUserAccess = permitSuperUserAccess;
            PermitCollectiveLogins = permitCollectiveLogins;
            Environment = environment;
            DisableCustomUrls = disableCustomUrls;
            CustomEmailConfirmationAction = customEmailConfirmationAction;
            CustomPasswordResetAction = customPasswordResetAction;
            TimeFrame = timeFrame;
            AccessDuration = accessDuration;
            DisplayInGallery = displayInGallery;
            DateCreated = dateCreated;
            DateUpdated = dateUpdated;
            UserApps = [];
            Users = [];
            UseCustomSMTPServer = useCustomSMTPServer;
        }
        #endregion

        #region Methods
        public void ActivateApp()
        {
            IsActive = true;
        }

        public void DeactivateApp()
        {
            IsActive = false;
        }

        public string GetLicense(int id, int ownerId)
        {
            var result = string.Empty;

            if (Id == id && OwnerId == id)
            {
                result = License;
            }

            return result;
        }

        public void NullifyLicense()
        {
            _license = null;
        }

        public void NullifySMTPServerSettings()
        {
            SMTPServerSettings = null;
        }

        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.Name:{1}", Id, Name);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>()
        {
            var type = typeof(T);

            if (type == typeof(GalleryApp))
            {
                return new GalleryApp
                {
                    Id = Id,
                    Name = Name,
                    Url = Environment == ReleaseEnvironment.LOCAL ?
                        LocalUrl :
                        Environment == ReleaseEnvironment.STAGING ?
                            StagingUrl :
                            Environment == ReleaseEnvironment.QA ?
                                QaUrl :
                                Environment == ReleaseEnvironment.PROD ?
                                    ProdUrl :
                                    null,
                    CreatedBy = CreatedBy,
                    UserCount = UserCount,
                    DateCreated = DateCreated,
                    DateUpdated = DateUpdated
                };
            }
            else
            {
                return null;
            }
        }

        private bool GetUseCustomEmailConfirmationAction()
        {
            if (
                Environment == ReleaseEnvironment.LOCAL
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(LocalUrl)
                && !string.IsNullOrEmpty(CustomEmailConfirmationAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.STAGING
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(StagingUrl)
                && !string.IsNullOrEmpty(CustomEmailConfirmationAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.QA
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(QaUrl)
                && !string.IsNullOrEmpty(CustomEmailConfirmationAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.PROD
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(ProdUrl)
                && !string.IsNullOrEmpty(CustomEmailConfirmationAction)
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetUseCustomPasswordResetAction()
        {
            if (
                Environment == ReleaseEnvironment.LOCAL
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(LocalUrl)
                && !string.IsNullOrEmpty(CustomPasswordResetAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.STAGING
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(StagingUrl)
                && !string.IsNullOrEmpty(CustomPasswordResetAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.QA
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(QaUrl)
                && !string.IsNullOrEmpty(CustomPasswordResetAction)
            )
            {
                return true;
            }
            else if (
                Environment == ReleaseEnvironment.PROD
                && !DisableCustomUrls
                && !string.IsNullOrEmpty(ProdUrl)
                && !string.IsNullOrEmpty(CustomPasswordResetAction)
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetUserCount()
        {
            if (Users != null)
            {
                return Users.Count;
            }
            else
            {
                return 0;
            }
        }

        private void SetTimeFrame(TimeFrame value)
        {
            _timeFrame = value;


            if (value == TimeFrame.SECONDS && AccessDuration > 60)
            {
                AccessDuration = 60;
            }
            else if (value == TimeFrame.MINUTES && AccessDuration > 60)
            {
                AccessDuration = 60;
            }
            else if (value == TimeFrame.HOURS && AccessDuration > 23)
            {
                AccessDuration = 23;
            }
            else if (value == TimeFrame.DAYS && AccessDuration > 31)
            {
                AccessDuration = 31;
            }
            else if (value == TimeFrame.MONTHS && AccessDuration > 12)
            {
                AccessDuration = 12;
            }
            else if (value == TimeFrame.YEARS && AccessDuration > 5)
            {
                AccessDuration = 5;
            }
        }

        private void SetAccessDuration(int value)
        {
            if (TimeFrame == TimeFrame.SECONDS)
            {
                if (0 < value || value <= 59)
                {
                    _accessDuration = value;
                }
            }
            else if (TimeFrame == TimeFrame.MINUTES)
            {
                if (0 < value || value <= 59)
                {
                    _accessDuration = value;
                }
            }
            else if (TimeFrame == TimeFrame.HOURS)
            {
                if (0 < value || value <= 23)
                {
                    _accessDuration = value;
                }
            }
            else if (TimeFrame == TimeFrame.DAYS)
            {
                if (0 < value || value <= 31)
                {
                    _accessDuration = value;
                }
            }
            else if (TimeFrame == TimeFrame.MONTHS)
            {
                if (0 < value || value <= 12)
                {
                    _accessDuration = value;
                }
            }
            else if (TimeFrame == TimeFrame.YEARS)
            {
                if (0 < value || value <= 5)
                {
                    _accessDuration = value;
                }
            }
        }
        #endregion
    }
}
