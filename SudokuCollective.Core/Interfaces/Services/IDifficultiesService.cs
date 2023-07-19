using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IDifficultiesService : IService
    {
        Task<IResult> CreateAsync(IRequest request);
        Task<IResult> GetAsync(int id);
        Task<IResult> UpdateAsync(int id, IRequest request);
        Task<IResult> DeleteAsync(int id);
        Task<IResult> GetDifficultiesAsync();
    }
}
