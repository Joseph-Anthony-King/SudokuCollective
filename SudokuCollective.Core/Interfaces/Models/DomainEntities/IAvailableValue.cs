namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IAvailableValue
    {
        int Value { get; set; }
        bool Available { get; set; }
    }
}
