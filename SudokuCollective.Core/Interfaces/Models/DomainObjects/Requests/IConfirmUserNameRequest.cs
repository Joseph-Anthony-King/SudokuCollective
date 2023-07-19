namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IConfirmUserNameRequest
    {
        string License { get; set; }
        string Email { get; set; }
    }
}
