using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class GalleryApp : IGalleryApp
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
        [Required, JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), JsonPropertyName("souceCodeUrl")]
        public string SourceCodeUrl { get; set; }
        [Required, JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
        [Required, JsonPropertyName("userCount")]
        public int UserCount { get; set; }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        [Required, JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }
        #endregion

        #region Constructors
        public GalleryApp()
        {
            Id = 0;
            Name = string.Empty;
            Url = string.Empty;
            SourceCodeUrl = string.Empty;
            CreatedBy = string.Empty;
            UserCount = 0;
            DateCreated = DateTime.MinValue;
            DateUpdated = DateTime.MinValue;
        }

        [JsonConstructor]
        public GalleryApp(
            int id, 
            string name, 
            string url, 
            string sourceCodeUrl, 
            string createdBy, 
            int userCount, 
            DateTime dateCreated, 
            DateTime dateUpdated)
        {
            Id = id;
            Name = name;
            Url = url;
            SourceCodeUrl = sourceCodeUrl;
            CreatedBy = createdBy;
            UserCount = userCount;
            DateCreated = dateCreated;
            DateUpdated = dateUpdated;
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.Name:{1}", Id, Name);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();

        public void NullifySourceCodeUrl()
        {
            if (string.IsNullOrEmpty(SourceCodeUrl))
            {
                SourceCodeUrl = null;
            }
        }
        #endregion
    }
}
