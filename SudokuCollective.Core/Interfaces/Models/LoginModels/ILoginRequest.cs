namespace SudokuCollective.Core.Interfaces.Models.LoginModels
{
    public interface ILoginRequest
    {
        string UserName { get; set; }
        string Password { get; set; }
        string License { get; set; }
    }
}
