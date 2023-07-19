using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IEmailConfirmation : IDomainEntity
    {
        int UserId { get; set; }
        int AppId { get; set; }
        string Token { get; set; }
        string OldEmailAddress { get; set; }
        string NewEmailAddress { get; set; }
        bool? OldEmailAddressConfirmed { get; set; }
        bool IsUpdate { get; }
        DateTime DateCreated { get; set; }
    }
}
