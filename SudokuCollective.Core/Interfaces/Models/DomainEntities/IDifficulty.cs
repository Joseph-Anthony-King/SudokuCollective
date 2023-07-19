using System.Collections.Generic;
using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IDifficulty : IDomainEntity
    {
        string Name { get; set; }
        string DisplayName { get; set; }
        DifficultyLevel DifficultyLevel { get; set; }
        ICollection<ISudokuMatrix> Matrices { get; set; }
    }
}
