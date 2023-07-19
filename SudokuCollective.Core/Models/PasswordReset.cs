using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Core.Models
{
    public class PasswordReset : IPasswordReset
    {
        #region Fields
        private string _token = string.Empty;
        private readonly GuidValidatedAttribute _guidValidator = new();
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [Required, JsonPropertyName("token"), GuidValidated(ErrorMessage = AttributeMessages.InvalidToken)]
        public string Token
        {
            get => _token;
            set => _token = CoreUtilities.SetField(
                value, 
                _guidValidator, 
                AttributeMessages.InvalidToken);
        }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        #endregion

        #region Constructors
        public PasswordReset()
        {
            Id = 0;
            UserId = 0;
            AppId = 0;
            DateCreated = DateTime.MinValue;
        }

        public PasswordReset(int userId, int appId) : this()
        {
            UserId = userId;
            AppId = appId;
            Token = Guid.NewGuid().ToString();
            DateCreated = DateTime.UtcNow;
        }

        [JsonConstructor]
        public PasswordReset(int id, int userId, int appId, string token, DateTime dateCreated)
        {
            Id = id;
            UserId = userId;
            AppId = appId;
            Token = token;
            DateCreated = dateCreated;
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.AppId:{1}.UserId:{2}", Id, AppId, UserId);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion
    }
}
