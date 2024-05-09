using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class SMTPServerSettings : ISMTPServerSettings
    {
        #region Fields
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        #region Properties
        [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        [JsonPropertyName("smtpServer")]
        public string SmtpServer { get; set; }
        [JsonPropertyName("port")]
        public int Port { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; }
        [JsonPropertyName("Password"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Password { get; set; }
        [JsonPropertyName("fromEmail")]
        public string FromEmail { get; set; }
        [JsonPropertyName("appId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int AppId { get; set; }
        [JsonIgnore]
        IApp ISMTPServerSettings.App
        {
            get => App;
            set => App = (App)value;
        }
        [JsonIgnore]
        public App App { get; set; }
        #endregion

        #region Constructors
        public SMTPServerSettings()
        {
            Id = 0;
            SmtpServer = string.Empty;
            Port = 0;
            UserName = string.Empty;
            Password = string.Empty;
            FromEmail = string.Empty;
            AppId = 0;
        }

        [JsonConstructor]
        public SMTPServerSettings(
            int id,
            string smtpServer,
            int port,
            string userName,
            string password,
            string fromEmail,
            int appId,
            App app = null
        )
        {
            Id = id;
            SmtpServer = smtpServer;
            Port = port;
            UserName = userName;
            Password = password;
            FromEmail = fromEmail;
            AppId = appId;
            if (app != null)
            {
                App = app;
            }
            else
            {
                App = new App();
            }
        }
        #endregion

        #region Methods
        public bool AreSettingsValid()
        {
            var result = true;

            if (Id == 0 || 
                string.IsNullOrEmpty(SmtpServer) || 
                Port == 0 || 
                string.IsNullOrEmpty(UserName) || 
                string.IsNullOrEmpty(Password) || 
                string.IsNullOrEmpty(FromEmail) || 
                AppId == 0)
            {
                result = false;
            }

            return result;
        }

        public void Sanitize()
        {
            Id = 0;
            Password = null;
            AppId = 0;
        }

        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.AppId:{1}", Id, AppId);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion
    }
}
