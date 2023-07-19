namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IRequestPasswordResetRequest
    {
        string License { get; set; }
        string Email { get; set; }
    }
}
