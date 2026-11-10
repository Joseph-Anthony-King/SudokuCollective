using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IAdminService
    {
        Task<IResult> GetSystemStatsAndValues(int requestorId);
    }
}
