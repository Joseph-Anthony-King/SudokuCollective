using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IRolesRepository<TEntity> : IRepository<TEntity> where TEntity : IRole
    {
        Task<bool> HasRoleLevelAsync(RoleLevel level);
        Task<bool> IsListValidAsync(List<int> ids);
    }
}
