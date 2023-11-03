using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IPasswordReset : IDomainEntity
    {
        string Token { get; set; }
        int UserId { get; set; }
        int AppId { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}
