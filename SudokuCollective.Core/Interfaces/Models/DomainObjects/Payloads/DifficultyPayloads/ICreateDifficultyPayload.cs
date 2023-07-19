using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface ICreateDifficultyPayload : IPayload
    {
        string Name { get; set; }
        string DisplayName { get; set; }
        DifficultyLevel DifficultyLevel { get; set; }
    }
}
