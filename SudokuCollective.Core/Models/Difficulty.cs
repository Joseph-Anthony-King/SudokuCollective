﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class Difficulty : IDifficulty
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
        [Required, JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [Required, JsonPropertyName("difficultyLevel")]
        public DifficultyLevel DifficultyLevel { get; set; }
        [JsonIgnore]
        ICollection<ISudokuMatrix> IDifficulty.Matrices
        { 
            get => Matrices.ConvertAll(m => (ISudokuMatrix)m);
            set => Matrices = value.ToList().ConvertAll(m => (SudokuMatrix)m);
        }
        [JsonIgnore]
        public virtual List<SudokuMatrix> Matrices { get; set; }
        #endregion

        #region Constructors
        public Difficulty()
        {
            Id = 0;
            Name = string.Empty;
            DifficultyLevel = DifficultyLevel.NULL;
            Matrices = [];
        }

        [JsonConstructor]
        public Difficulty(int id, string name,
            string displayName, DifficultyLevel difficultyLevel)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            DifficultyLevel = difficultyLevel;
            Matrices = [];
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.DisplayName:{1}", Id, DisplayName);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            _serializerOptions);

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();
        #endregion
    }
}
