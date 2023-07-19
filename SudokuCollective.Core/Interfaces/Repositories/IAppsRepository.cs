using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IAppsRepository<TEntity> : IRepository<TEntity> where TEntity : IApp
    {
        Task<IRepositoryResponse> GetByLicenseAsync(string license);
        Task<IRepositoryResponse> GetAppUsersAsync(int id);
        Task<IRepositoryResponse> GetNonAppUsersAsync(int id);
        Task<IRepositoryResponse> GetMyAppsAsync(int ownerId);
        Task<IRepositoryResponse> GetMyRegisteredAppsAsync(int userId);
        Task<IRepositoryResponse> ResetAsync(TEntity entity);
        Task<IRepositoryResponse> AddAppUserAsync(int userId, string license);
        Task<IRepositoryResponse> RemoveAppUserAsync(int userId, string license);
        Task<IRepositoryResponse> ActivateAsync(int id);
        Task<IRepositoryResponse> DeactivateAsync(int id);
        Task<bool> IsAppLicenseValidAsync(string license);
        Task<bool> IsUserRegisteredToAppAsync(int id, string license, int userId);
        Task<bool> IsUserOwnerOThisfAppAsync(int id, string license, int userId);
        Task<string> GetLicenseAsync(int id);
        Task<IRepositoryResponse> GetGalleryAppsAsync();
    }
}
