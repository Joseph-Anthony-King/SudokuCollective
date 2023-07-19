using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IAnnonymousGameResult
    {
        List<List<int>> SudokuMatrix { get; set; }
    }
}
