using System.Collections.Generic;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    internal interface IAdminSummaryResult
    {
        int ApplicationsCount { get; set; }
        int DifficultiesCount { get; set; }
        int EmailConfirmationsCount { get; set; }
        int GamesCount { get; set; }
        int PasswordResetsCount { get; set; }
        int RolesCount { get; set; }
        int SolutionsCount { get; set; }
        int UsersCount { get; set; }
        List<IDifficulty> Difficulties { get; set; }
        List<IRole> Roles { get; set; }
        List<IEnumListItem> ReleaseEnvironments { get; set; }
        List<IEnumListItem> SortValues { get; set; }
        List<IEnumListItem> TimeFrames { get; set; }
    }
}
