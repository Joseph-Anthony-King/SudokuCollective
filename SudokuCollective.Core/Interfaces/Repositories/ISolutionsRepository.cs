using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface ISolutionsRepository<TEntity> : IRepository<TEntity> where TEntity : ISudokuSolution
    {
        Task<IRepositoryResponse> AddSolutionsAsync(List<ISudokuSolution> solutions);
        Task<IRepositoryResponse> GetSolvedSolutionsAsync();
    }
}
