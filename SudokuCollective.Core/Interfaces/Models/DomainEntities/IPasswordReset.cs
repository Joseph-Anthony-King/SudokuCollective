using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IPasswordReset : IDomainEntity
    {
        int UserId { get; set; }
        int AppId { get; set; }
        string Token { get; set; }
        DateTime DateCreated { get; set; }
    }
}
