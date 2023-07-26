using System;
using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface ITranslatedUser : IDomainEntity
    {
        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string NickName { get; set; }
        string FullName { get; set; }
        string Email { get; set; }
        bool IsEmailConfirmed { get; set; }
        bool ReceivedRequestToUpdateEmail { get; set; }
        bool ReceivedRequestToUpdatePassword { get; set; }
        bool IsActive { get; set; }
        bool IsSuperUser { get; set; }
        bool IsAdmin { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        ICollection<IGame> Games { get; set; }
        void NullifyEmail();
    }
}
