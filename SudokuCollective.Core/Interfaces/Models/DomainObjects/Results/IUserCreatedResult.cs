using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IUserCreatedResult
    {
        IUserDTO User { get; set; }
        string Token { get; set; }
        DateTime TokenExpirationDate { get; set; }
        bool EmailConfirmationSent { get; set; }
    }
}
