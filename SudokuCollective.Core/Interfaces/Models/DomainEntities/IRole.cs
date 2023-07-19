using System.Collections.Generic;
using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IRole : IDomainEntity
    {
        string Name { get; set; }
        RoleLevel RoleLevel { get; set; }
        ICollection<IUserRole> Users { get; set; }
    }
}
