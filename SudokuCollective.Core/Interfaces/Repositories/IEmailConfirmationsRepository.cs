using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IEmailConfirmationsRepository<TEntity> : IRepository<TEntity> where TEntity : IEmailConfirmation
    {
        Task<IRepositoryResponse> CreateAsync(TEntity entity);
        Task<IRepositoryResponse> GetAsync(string token);
        Task<bool> HasOutstandingEmailConfirmationAsync(int userId, int appid);
        Task<IRepositoryResponse> RetrieveEmailConfirmationAsync(int userId, int appid);
    }
}
