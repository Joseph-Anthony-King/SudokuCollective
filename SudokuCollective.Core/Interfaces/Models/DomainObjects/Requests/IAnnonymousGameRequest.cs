using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IAnnonymousGameRequest
    {
        DifficultyLevel DifficultyLevel { get; set; }
        int AppId { get; set; }
    }
}
