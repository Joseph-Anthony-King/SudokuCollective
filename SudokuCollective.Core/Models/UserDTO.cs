using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Utilities;

namespace SudokuCollective.Core.Models
{
    public class UserDTO : IUserDTO
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
        [Required, JsonPropertyName("userName")]
        public string UserName { get; set; }
        [Required, JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [Required, JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [Required, JsonPropertyName("nickName")]
        public string NickName { get; set; }
        [Required, JsonPropertyName("fullName")]
        public string FullName { get; set; }
        [Required, JsonPropertyName("email")]
        public string Email { get; set; }
        [Required, JsonPropertyName("isEmailConfirmed")]
        public bool IsEmailConfirmed { get; set; }
        [Required, JsonPropertyName("receivedRequestToUpdateEmail")]
        public bool ReceivedRequestToUpdateEmail { get; set; }
        [Required, JsonPropertyName("receivedRequestToUpdatePassword")]
        public bool ReceivedRequestToUpdatePassword { get; set; }
        [Required, JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [Required, JsonPropertyName("isSuperUser")]
        public bool IsSuperUser { get; set; }
        [Required, JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        [Required, JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }
        [JsonIgnore]
        ICollection<IGame> IUserDTO.Games
        {
            get => Games.ConvertAll(g => (IGame)g);
            set => Games = value.ToList().ConvertAll(g => (Game)g);
        }
        [Required, JsonPropertyName("games"), JsonConverter(typeof(IDomainEntityListConverter<List<Game>>))]
        public virtual List<Game> Games { get; set; }
        #endregion

        #region Constructors
        public UserDTO()
        {
            var createdDate = DateTime.UtcNow;

            Id = 0;
            UserName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            NickName = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            IsEmailConfirmed = false;
            ReceivedRequestToUpdateEmail = false;
            ReceivedRequestToUpdatePassword = false;
            IsActive = false;
            IsSuperUser = false;
            IsAdmin = false;
            DateCreated = createdDate;
            DateUpdated = createdDate;
            Games = [];
        }
        #endregion

        #region Methods
        public void NullifyEmail()
        {
            Email = null;
        }
        
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.UserName:{1}", Id, UserName);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
    #endregion
  }
}
