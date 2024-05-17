using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface ICreateGamePayload : IPayload
    {
        DifficultyLevel DifficultyLevel { get; set; }
    }
}
