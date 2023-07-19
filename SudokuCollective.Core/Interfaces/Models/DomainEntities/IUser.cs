using System;
using System.Collections.Generic;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IUser : IDomainEntity
    {
        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string NickName { get; set; }
        string FullName { get; }
        string Email { get; set; }
        bool IsEmailConfirmed { get; set; }
        bool ReceivedRequestToUpdateEmail { get; set; }
        string Password { get; set; }
        bool ReceivedRequestToUpdatePassword { get; set; }
        bool IsActive { get; }
        bool IsSuperUser { get; }
        bool IsAdmin { get; }
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        ICollection<IGame> Games { get; set; }
        ICollection<IUserRole> Roles { get; set; }
        ICollection<IUserApp> Apps { get; set; }
        void ActivateUser();
        void DeactiveUser();
        void UpdateRoles();
        void NullifyEmail();
        void NullifyPassword();
    }
}
