namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IRequestPasswordResetPayload : IPayload
    {
        string License { get; set; }
        string Email { get; set; }
    }
}
