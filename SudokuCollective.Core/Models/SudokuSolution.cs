using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Models
{
    public class SudokuSolution : ISudokuSolution
    {
        #region Properites
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("solutionList")]
        public virtual List<int> SolutionList { get; set; }
        [Required, JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }
        [Required, JsonPropertyName("dateSolved")]
        public DateTime DateSolved { get; set; }
        [JsonIgnore]
        public List<int> FirstRow { get => GetValues(0, 9); }
        [JsonIgnore]
        public List<int> SecondRow { get => GetValues(9, 9); }
        [JsonIgnore]
        public List<int> ThirdRow { get => GetValues(18, 9); }
        [JsonIgnore]
        public List<int> FourthRow { get => GetValues(27, 9); }
        [JsonIgnore]
        public List<int> FifthRow { get => GetValues(36, 9); }
        [JsonIgnore]
        public List<int> SixthRow { get => GetValues(45, 9); }
        [JsonIgnore]
        public List<int> SeventhRow { get => GetValues(54, 9); }
        [JsonIgnore]
        public List<int> EighthRow { get => GetValues(63, 9); }
        [JsonIgnore]
        public List<int> NinthRow { get => GetValues(72, 9); }
        [JsonIgnore]
        IGame ISudokuSolution.Game
        {
            get => Game;
            set => Game = (Game)value;
        }
        [JsonIgnore]
        public virtual Game Game { get; set; }
        #endregion

        #region Constructors
        public SudokuSolution()
        {
            var createdDate = DateTime.UtcNow;

            Id = 0;
            DateCreated = createdDate;
            DateSolved = DateTime.MinValue;
            SolutionList = [];

            for (var i = 0; i < 81; i++)
            {
                SolutionList.Add(0);
            }
        }

        public SudokuSolution(List<int> intList) : this()
        {
            var solvedDate = DateTime.UtcNow;
            DateSolved = solvedDate;

            SolutionList = [];
            SolutionList = intList;
        }

        [JsonConstructor]
        public SudokuSolution(
            int id,
            List<int> solutionList,
            DateTime dateCreated, 
            DateTime dateSolved)
        {
            Id = id;
            SolutionList = solutionList;
            DateCreated = dateCreated;
            DateSolved = dateSolved;
        }
        #endregion

        #region Methods
        private List<int> GetValues(int skipValue, int takeValue)
        {
            return SolutionList.Skip(skipValue).Take(takeValue).ToList();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (var solutionListInt in SolutionList)
            {
                result.Append(solutionListInt);
            }

            return result.ToString();
        }

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
