using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Values
{
    public interface IEnumListItem
    {
        string Label { get; set; }
        int Value { get; set; }
        List<string> AppliesTo { get; set; }
    }
}
