using System;
using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IEmailConfirmation : IDomainEntity
    {
        string Token { get; set; }
        EmailConfirmationType ConfirmationType { get; set; }
        bool IsUpdate { get; }
        int UserId { get; set; }
        int AppId { get; set; }
        string OldEmailAddress { get; set; }
        string NewEmailAddress { get; set; }
        bool? OldEmailAddressConfirmed { get; set; }
        DateTime ExpirationDate { get; set; }
    }
}
