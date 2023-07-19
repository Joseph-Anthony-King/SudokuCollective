using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IUserResult
    {
        IUser User { get; set; }
        bool? ConfirmationEmailSuccessfullySent { get; set; }
        string Token { get; set; }
    }
}
