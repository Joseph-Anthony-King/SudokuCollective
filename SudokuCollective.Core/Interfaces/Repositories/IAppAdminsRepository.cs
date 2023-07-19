using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IAppAdminsRepository<TEntity> : IRepository<TEntity> where TEntity : IAppAdmin
    {
        Task<bool> HasAdminRecordAsync(int appId, int userId);
        Task<IRepositoryResponse> GetAdminRecordAsync(int appId, int userId);
    }
}
