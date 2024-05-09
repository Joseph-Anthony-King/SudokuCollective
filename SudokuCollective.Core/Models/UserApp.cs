using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class UserApp : IUserApp
    {
        #region Fields
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set;}
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [JsonIgnore]
        IUser IUserApp.User
        {
            get => User;
            set => User = (User)value;
        }
        [JsonIgnore]
        public virtual User User { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [JsonIgnore]
        IApp IUserApp.App
        {
            get => App;
            set => App = (App)value;
        }
        [JsonIgnore]
        public virtual App App { get; set; }
        #endregion

        #region Constructors
        public UserApp()
        {
            Id = 0;
            UserId = 0;
            User = null;
            AppId = 0;
            App = null;
        }

        public UserApp(int userId, int appId) : this()
        {
            UserId = userId;
            AppId = appId;
        }

        [JsonConstructor]
        public UserApp(
            int id,
            int userId,
            int appId)
        {
            Id = id;
            UserId = userId;
            AppId = appId;
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.AppId:{1}.UserId:{2}", Id, AppId, UserId);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion
    }
}
