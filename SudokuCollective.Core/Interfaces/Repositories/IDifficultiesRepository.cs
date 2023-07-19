using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IDifficultiesRepository<TEntity> : IRepository<TEntity> where TEntity : IDifficulty
    {
        Task<bool> HasDifficultyLevelAsync(DifficultyLevel level);
    }
}
