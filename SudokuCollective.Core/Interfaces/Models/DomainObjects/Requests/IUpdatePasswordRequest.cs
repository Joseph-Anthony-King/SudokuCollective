namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IUpdatePasswordRequest
    {
        string License { get; set; }
        int UserId { get; set; }
        string NewPassword { get; set; }
    }
}
