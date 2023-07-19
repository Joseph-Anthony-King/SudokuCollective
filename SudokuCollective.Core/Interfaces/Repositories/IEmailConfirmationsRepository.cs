using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IEmailConfirmationsRepository<TEntity> where TEntity : IEmailConfirmation
    {
        Task<IRepositoryResponse> CreateAsync(TEntity entity);
        Task<IRepositoryResponse> GetAsync(string token);
        Task<IRepositoryResponse> GetAllAsync();
        Task<IRepositoryResponse> UpdateAsync(TEntity entity);
        Task<IRepositoryResponse> DeleteAsync(TEntity entity);
        Task<bool> HasEntityAsync(int id);
        Task<bool> HasOutstandingEmailConfirmationAsync(int userId, int appid);
        Task<IRepositoryResponse> RetrieveEmailConfirmationAsync(int userId, int appid);
    }
}
