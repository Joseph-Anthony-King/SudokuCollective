namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests
{
    public interface ISignupRequest
    {
        string License { get; set; }
        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string NickName { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}
