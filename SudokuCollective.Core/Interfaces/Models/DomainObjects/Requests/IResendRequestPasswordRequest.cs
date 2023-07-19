namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface IResendRequestPasswordRequest
    {
        int UserId { get; set; }
        int AppId { get; set; }
    }
}
