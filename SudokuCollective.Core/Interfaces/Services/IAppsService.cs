using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IAppsService : IService
    {
        Task<IResult> CreateAync(IRequest request);
        Task<IResult> GetAsync(int id, int userId);
        Task<IResult> UpdateAsync(int id, IRequest request);
        Task<IResult> DeleteOrResetAsync(int id, bool isReset = false);
        Task<IResult> GetAppsAsync(IRequest request);
        Task<IResult> GetMyAppsAsync(int ownerId, IPaginator paginator);
        Task<IResult> GetMyRegisteredAppsAsync(int userId, IPaginator paginator);
        Task<IResult> GetByLicenseAsync(string license, int userId);
        Task<ILicenseResult> GetLicenseAsync(int id, int requestorId);
        Task<IResult> GetAppUsersAsync(int id, int requestorId, IPaginator paginator, bool appUsers = true);
        Task<IResult> AddAppUserAsync(int appId, int userId);
        Task<IResult> RemoveAppUserAsync(int appId, int userId);
        Task<IResult> ActivateAsync(int id);
        Task<IResult> DeactivateAsync(int id);
        Task<IResult> ActivateAdminPrivilegesAsync(int appId, int userId);
        Task<IResult> DeactivateAdminPrivilegesAsync(int appId, int userId);
        Task<bool> IsUserOwnerOThisfAppAsync(IHttpContextAccessor httpContextAccessor, string license, int userId, string requestorLicense, int requestorAppId, int requestorId);
        Task<bool> IsRequestValidOnThisTokenAsync(IHttpContextAccessor httpContextAccessor, string license, int appId, int userId);
        Task<IResult> GetGalleryAppsAsync(IPaginator paginator = null);
    }
}
