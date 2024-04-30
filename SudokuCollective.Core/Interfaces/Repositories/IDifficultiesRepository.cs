using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.ServiceModels;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IDifficultiesRepository<TEntity> : IRepository<TEntity> where TEntity : IDifficulty
    {
        Task<IRepositoryResponse> GetByDifficultyLevelAsync(DifficultyLevel difficultyLevel);
        Task<bool> HasDifficultyLevelAsync(DifficultyLevel level);
    }
}
