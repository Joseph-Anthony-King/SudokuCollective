namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IGamesPayload : IPayload
    {
        int UserId { get; set; }
    }
}
