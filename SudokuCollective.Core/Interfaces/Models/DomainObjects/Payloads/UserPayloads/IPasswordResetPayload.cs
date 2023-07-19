namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IPasswordResetPayload : IPayload
    {
        int UserId { get; set; }
        string NewPassword { get; set; }
    }
}
