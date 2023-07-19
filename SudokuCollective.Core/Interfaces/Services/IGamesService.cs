using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IGamesService : IService
    {
        Task<IResult> CreateAsync(IRequest request);
        Task<IResult> UpdateAsync(int id, IRequest request);
        Task<IResult> DeleteAsync(int id);
        Task<IResult> GetGameAsync(int id, int appId);
        Task<IResult> GetGamesAsync(IRequest request);
        Task<IResult> GetMyGameAsync(int id, IRequest request);
        Task<IResult> GetMyGamesAsync(IRequest request);
        Task<IResult> UpdateMyGameAsync(int id, IRequest request);
        Task<IResult> DeleteMyGameAsync(int id, IRequest request);
        Task<IResult> CheckAsync(int id, IRequest request);
        Task<IResult> CreateAnnonymousAsync(DifficultyLevel difficultyLevel);
        IResult CheckAnnonymous(List<int> intList);
    }
}
