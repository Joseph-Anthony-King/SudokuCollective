using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IValuesService
    {
        Task<IResult> GetAsync(IPaginator paginator = null);
        IResult GetReleaseEnvironments();
        IResult GetSortValues();
        IResult GetTimeFrames();
    }
}
