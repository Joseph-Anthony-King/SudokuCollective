namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IResetPasswordPayload
    {
        string NewPassword { get; set; }
    }
}
