using System.Collections.Generic;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IGamePayload : IPayload
    {
        List<SudokuCell> SudokuCells { get; set; }
    }
}
