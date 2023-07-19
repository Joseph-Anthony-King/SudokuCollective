using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IUsersRepository<TEntity> : IRepository<TEntity> where TEntity : IUser
    {
        Task<IRepositoryResponse> GetByUserNameAsync(string username);
        Task<IRepositoryResponse> GetByEmailAsync(string email);
        Task<IRepositoryResponse> AddRoleAsync(int userId, int roleId);
        Task<IRepositoryResponse> AddRolesAsync(int userId, List<int> roleIds);
        Task<IRepositoryResponse> RemoveRoleAsync(int userId, int roleId);
        Task<IRepositoryResponse> RemoveRolesAsync(int userId, List<int> roleIds);
        Task<IRepositoryResponse> ConfirmEmailAsync(IEmailConfirmation emailConfirmation);
        Task<IRepositoryResponse> UpdateEmailAsync(IEmailConfirmation emailConfirmation);
        Task<IRepositoryResponse> GetMyAppsAsync(int id);
        Task<string> GetAppLicenseAsync(int appId);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<bool> PromoteToAdminAsync(int id);
        Task<bool> IsUserRegisteredAsync(int id);
        Task<bool> IsUserNameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUpdatedUserNameUniqueAsync(int userId, string username);
        Task<bool> IsUpdatedEmailUniqueAsync(int userId, string email);
    }
}
