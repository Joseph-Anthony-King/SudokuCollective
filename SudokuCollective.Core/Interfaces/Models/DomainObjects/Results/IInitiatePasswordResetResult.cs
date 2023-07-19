using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IInitiatePasswordResetResult : IUserResult
    {
        IApp App { get; set; }
    }
}
