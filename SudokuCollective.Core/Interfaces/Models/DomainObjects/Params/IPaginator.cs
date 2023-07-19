using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Params
{
    public interface IPaginator
    {
        int Page { get; set; }
        int ItemsPerPage { get; set; }
        SortValue SortBy { get; set; }
        bool OrderByDescending { get; set; }
        bool IncludeCompletedGames { get; set; }
        bool IsNull();
    }
}
