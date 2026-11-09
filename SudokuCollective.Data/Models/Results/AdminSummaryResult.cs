using System.Runtime.CompilerServices;
using System.Collections.Generic;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
namespace SudokuCollective.Data.Models.Results
{
    internal class AdminSummaryResult : IAdminSummaryResult
    {
        public int ApplicationsCount { get; set; } = 0;
        public int DifficultiesCount { get; set; } = 0;
        public int EmailConfirmationsCount { get; set; } = 0;
        public int GamesCount { get; set; } = 0;
        public int PasswordResetsCount { get; set; } = 0;
        public int RolesCount { get; set; } = 0;
        public int SolutionsCount { get; set; } = 0;
        public int UsersCount { get; set; } = 0;
        public List<IDifficulty> Difficulties { get; set; } = [];
        public List<IRole> Roles { get; set; } = [];
        public List<IEnumListItem> ReleaseEnvironments { get; set; } = [];
        public List<IEnumListItem> SortValues { get; set; } = [];
        public List<IEnumListItem> TimeFrames { get; set; } = [];
    }
}
