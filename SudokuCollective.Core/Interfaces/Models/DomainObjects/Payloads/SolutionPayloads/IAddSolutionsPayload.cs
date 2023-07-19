namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IAddSolutionsPayload : IPayload
    {
        int Limit { get; set; }
    }
}
