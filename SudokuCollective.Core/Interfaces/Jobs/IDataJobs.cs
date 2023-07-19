using System.Collections.Generic;
using System.Threading.Tasks;

namespace SudokuCollective.Core.Interfaces.Jobs
{
    public interface IDataJobs
    {
        Task AddSolutionJobAsync(List<int> intList);
        Task GenerateSolutionsJobAsync(int limit);
    }
}
