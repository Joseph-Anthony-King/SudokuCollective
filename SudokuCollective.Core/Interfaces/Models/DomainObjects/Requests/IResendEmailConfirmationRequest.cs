namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IResendEmailConfirmationRequest
    {
        string License { get; set; }
        int RequestorId { get; set; }
        int AppId { get; set; }
    }
}
