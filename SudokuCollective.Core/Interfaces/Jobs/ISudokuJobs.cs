using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Jobs
{
    public interface ISudokuJobs
    {
        Task<IResult> CreateGameJobAsync(DifficultyLevel difficultyLevel);
    }
}
