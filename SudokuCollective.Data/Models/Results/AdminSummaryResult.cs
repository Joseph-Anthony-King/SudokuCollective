using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Values;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
namespace SudokuCollective.Data.Models.Results
{
    internal class AdminSummaryResult : IAdminSummaryResult
    {
        [Required, JsonPropertyName("applicationsCount")]
        public int ApplicationsCount { get; set; } = 0;
        [Required, JsonPropertyName("difficultiesCount")]
        public int DifficultiesCount { get; set; } = 0;
        [Required, JsonPropertyName("emailConfirmationsCount")]
        public int EmailConfirmationsCount { get; set; } = 0;
        [Required, JsonPropertyName("gamesCount")]
        public int GamesCount { get; set; } = 0;
        [Required, JsonPropertyName("passwordResetsCount")]
        public int PasswordResetsCount { get; set; } = 0;
        [Required, JsonPropertyName("rolesCount")]
        public int RolesCount { get; set; } = 0;
        [Required, JsonPropertyName("solutionsCount")]
        public int SolutionsCount { get; set; } = 0;
        [Required, JsonPropertyName("usersCount")]
        public int UsersCount { get; set; } = 0;
        [JsonIgnore]
        List<IDifficulty> IAdminSummaryResult.Difficulties
        {
            get => Difficulties.ConvertAll(d => (IDifficulty)d);
            set => Difficulties = value.ConvertAll(d => (Difficulty)d);
        }
        [Required, JsonPropertyName("difficulties")]
        public List<Difficulty> Difficulties { get; set; } = [];
        [JsonIgnore]
        List<IRole> IAdminSummaryResult.Roles
        {
            get => Roles.ConvertAll(r => (IRole)r);
            set => Roles = value.ConvertAll(r => (Role)r);
        }
        [Required, JsonPropertyName("roles")]
        public List<Role> Roles { get; set; } = [];
        [JsonIgnore]
        List<IEnumListItem> IAdminSummaryResult.ReleaseEnvironments
        {
            get => ReleaseEnvironments.ConvertAll(i => (IEnumListItem)i);
            set => ReleaseEnvironments = value.ConvertAll(i => (EnumListItem)i);
        }
        [Required, JsonPropertyName("releaseEnvironments")]
        public List<EnumListItem> ReleaseEnvironments { get; set; } = [];
        [JsonIgnore]
        List<IEnumListItem> IAdminSummaryResult.SortValues
        {
            get => SortValues.ConvertAll(i => (IEnumListItem)i);
            set => SortValues = value.ConvertAll(i => (EnumListItem)i);
        }
        [Required, JsonPropertyName("sortValues")]
        public List<EnumListItem> SortValues { get; set; } = [];
        [JsonIgnore]
        List<IEnumListItem> IAdminSummaryResult.TimeFrames
        {
            get => TimeFrames.ConvertAll(i => (IEnumListItem)i);
            set => TimeFrames = value.ConvertAll(i => (EnumListItem)i);
        }
        [Required, JsonPropertyName("timeFrames")]
        public List<EnumListItem> TimeFrames { get; set; } = [];
    }
}
