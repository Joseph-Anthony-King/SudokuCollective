using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface ISolutionsService : IService
    {
        Task<IResult> GetAsync(int id);
        Task<IResult> GetSolutionsAsync(IRequest request);
        Task<IResult> SolveAsync(IAnnonymousCheckRequest request);
        Task<IResult> GenerateAsync();
        IResult GenerateSolutions(IRequest request);
    }
}
