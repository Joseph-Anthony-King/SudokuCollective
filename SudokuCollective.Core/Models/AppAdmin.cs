using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class AppAdmin : IAppAdmin
    {
        #region Fields
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        #endregion

        #region Constructors
        public AppAdmin()
        {
            Id = 0;
            AppId = 0;
            UserId = 0;
            IsActive = true;
        }

        public AppAdmin(int appId, int userId) : this()
        {
            Id = 0;
            AppId = appId;
            UserId = userId;
        }

        [JsonConstructor]
        public AppAdmin(
            int id,
            int appId,
            int userId,
            bool isActive)
        {
            Id = id;
            AppId = appId;
            UserId = userId;
            IsActive = isActive;
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
