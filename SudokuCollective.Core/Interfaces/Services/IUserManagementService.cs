using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IUserManagementService : IService
    {
        Task<bool> IsValidUserAsync(string username, string password);
        Task<UserAuthenticationErrorType> ConfirmAuthenticationIssueAsync(string username, string password, string license);
        Task<IResult> ConfirmUserNameAsync(string email, string license);
    }
}
