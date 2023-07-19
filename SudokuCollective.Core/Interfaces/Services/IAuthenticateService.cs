using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.LoginModels;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IAuthenticateService : IService
    {
        Task<IResult> AuthenticateAsync(ILoginRequest request);
    }
}
