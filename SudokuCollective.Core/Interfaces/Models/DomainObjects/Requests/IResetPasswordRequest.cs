namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IResetPasswordPayload
    {
        string Token { get; set; }
        string NewPassword { get; set; }
    }
}
