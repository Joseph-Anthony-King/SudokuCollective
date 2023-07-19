using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IRepository<TEntity> where TEntity : IDomainEntity
    {
        Task<IRepositoryResponse> AddAsync(TEntity entity);
        Task<IRepositoryResponse> GetAsync(int id);
        Task<IRepositoryResponse> GetAllAsync();
        Task<IRepositoryResponse> UpdateAsync(TEntity entity);
        Task<IRepositoryResponse> UpdateRangeAsync(List<TEntity> entities);
        Task<IRepositoryResponse> DeleteAsync(TEntity entity);
        Task<IRepositoryResponse> DeleteRangeAsync(List<TEntity> entities);
        Task<bool> HasEntityAsync(int id);
    }
}
