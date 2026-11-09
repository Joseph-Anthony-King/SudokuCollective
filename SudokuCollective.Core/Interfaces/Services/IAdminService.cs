using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    internal interface IAdminService
    {
        Task<IResult> GetSystemStatsAndValues();
    }
}
