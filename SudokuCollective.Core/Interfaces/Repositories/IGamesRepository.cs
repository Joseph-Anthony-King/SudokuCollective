using System.Threading.Tasks;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Repositories
{
    public interface IGamesRepository<TEntity> : IRepository<TEntity> where TEntity : IGame
    {
        Task<IRepositoryResponse> GetAppGameAsync(int id, int appid);
        Task<IRepositoryResponse> GetAppGamesAsync(int appid);
        Task<IRepositoryResponse> GetMyGameAsync(int userid, int gameid, int appid);
        Task<IRepositoryResponse> GetMyGamesAsync(int userid, int appid);
        Task<IRepositoryResponse> DeleteMyGameAsync(int userid, int gameid, int appid);
    }
}
