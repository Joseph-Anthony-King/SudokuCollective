using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Results
{
    public interface IAuthenticationResult
    {
        IUserDTO User { get; set; }
        string Token { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}
