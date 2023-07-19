namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface ICreateGamePayload : IPayload
    {
        int DifficultyId { get; set; }
    }
}
