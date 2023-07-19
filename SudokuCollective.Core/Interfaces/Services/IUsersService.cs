using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IUsersService : IService
    {
        Task<IResult> CreateAsync(
            ISignupRequest request,
            string baseUrl,
            string emailTemplatePath);
        Task<IResult> GetAsync(
            int id,
            string license,
            IRequest request = null);
        Task<IResult> GetUsersAsync(
            int requestorId,
            string license,
            IPaginator paginator);
        Task<IResult> UpdateAsync(
            int id,
            IRequest request,
            string baseUrl,
            string emailTemplatePath);
        Task<IResult> DeleteAsync(int id, string license);
        Task<IResult> RequestPasswordResetAsync(
            IRequestPasswordResetRequest request,
            string baseUrl,
            string emailTemplatePath);
        Task<IResult> InitiatePasswordResetAsync(
            string token,
            string license);
        Task<IResult> GetUserByPasswordTokenAsync(string token);
        Task<ILicenseResult> GetAppLicenseByPasswordTokenAsync(string token);
        Task<IResult> UpdatePasswordAsync(IUpdatePasswordRequest request);
        Task<IResult> AddUserRolesAsync(
            int userid,
            IRequest request,
            string license);
        Task<IResult> RemoveUserRolesAsync(
            int userid,
            IRequest request,
            string license);
        Task<IResult> ActivateAsync(int id);
        Task<IResult> DeactivateAsync(int id);
        Task<IResult> ConfirmEmailAsync(
            string token,
            string baseUrl,
            string emailTemplatePath);
        Task<IResult> ResendPasswordResetAsync(
            int userId,
            int appId,
            string baseUrl,
            string emailTemplatePath);
        Task<IResult> ResendEmailConfirmationAsync(
            int userId,
            int appId,
            string baseUrl,
            string emailTemplatePath,
            string license);
        Task<IResult> CancelEmailConfirmationRequestAsync(int id, int appId);
        Task<IResult> CancelPasswordResetRequestAsync(int id, int appId);
        Task<IResult> CancelAllEmailRequestsAsync(int id, int appId);
    }
}
