using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IAuthenticationResult
    {
        ITranslatedUser User { get; set; }
        string Token { get; set; }
    }
}
