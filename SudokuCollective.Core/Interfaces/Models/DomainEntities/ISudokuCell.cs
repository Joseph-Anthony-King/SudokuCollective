using System;
using System.Collections.Generic;
using SudokuCollective.Core.Structs;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface ISudokuCell : IDomainEntity
    {
        int Index { get; set; }
        int Column { get; set; }
        int Region { get; set; }
        int Row { get; set; }
        int Value { get; set; }
        int DisplayedValue { get; set; }
        bool Hidden { get; set; }
        int SudokuMatrixId { get; set; }
        ISudokuMatrix SudokuMatrix { get; set; }
        ICollection<IAvailableValue> AvailableValues { get; set; }
        void UpdateAvailableValues(int i);
        int ToInt32() => DisplayedValue;
        void OnSuccessfulSudokuCellUpdate(SudokuCellEventArgs e);
        event EventHandler<SudokuCellEventArgs> SudokuCellEvent;
    }
}
