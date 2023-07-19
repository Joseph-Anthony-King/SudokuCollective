using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IPasswordResetsRepository<TEntity> where TEntity : IDomainEntity
    {
        Task<IRepositoryResponse> CreateAsync(TEntity entity);
        Task<IRepositoryResponse> GetAsync(string token);
        Task<IRepositoryResponse> GetAllAsync();
        Task<IRepositoryResponse> UpdateAsync(TEntity entity);
        Task<IRepositoryResponse> DeleteAsync(TEntity entity);
        Task<bool> HasEntityAsync(int id);
        Task<bool> HasOutstandingPasswordResetAsync(int userId, int appid);
        Task<IRepositoryResponse> RetrievePasswordResetAsync(int userId, int appid);
    }
}
