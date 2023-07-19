namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IUpdateDifficultyPayload : IPayload
    {
        int Id { get; set; }
        string Name { get; set; }
        string DisplayName { get; set; }
    }
}
