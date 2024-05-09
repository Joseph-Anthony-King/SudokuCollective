using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class Role : IRole
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
        [Required, JsonPropertyName("name")]
        public string Name { get; set; }
        [Required, JsonPropertyName("roleLevel")]
        public RoleLevel RoleLevel { get; set; }
        [JsonIgnore]
        ICollection<IUserRole> IRole.Users
        {
            get => Users.ConvertAll(ur => (IUserRole)ur);
            set => Users = value.ToList().ConvertAll(ur => (UserRole)ur);
        }
        [JsonIgnore]
        public virtual List<UserRole> Users { get; set; }
        #endregion

        #region Constructors
        public Role()
        {
            Id = 0;
            Name = string.Empty;
            RoleLevel = RoleLevel.NULL;
            Users = [];
        }

        [JsonConstructor]
        public Role(int id, string name, RoleLevel roleLevel)
        {
            Id = id;
            Name = name;
            RoleLevel = roleLevel;
            Users = [];
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.Name:{1}", Id, Name);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion
    }
}
