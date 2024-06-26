using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos.Utilities;

namespace SudokuCollective.Repos
{
    public class GamesRepository<TEntity>(
        DatabaseContext context,
        IRequestService requestService,
        ILogger<GamesRepository<Game>> logger) : IGamesRepository<TEntity> where TEntity : Game
    {
        #region Fields
        private readonly DatabaseContext _context = context;
        private readonly IRequestService _requestService = requestService;
        private readonly ILogger<GamesRepository<Game>> _logger = logger;
        #endregion

        #region Methods    
        public async Task<IRepositoryResponse> AddAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entity);

                ArgumentOutOfRangeException.ThrowIfNotEqual(0, entity.Id, nameof(entity.Id));

                _context.Attach(entity);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

                    if (dbEntry is Game)
                    {
                        if (dbEntry.Id == entity.Id)
                        {
                            entry.State = EntityState.Added;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuMatrix matrix)
                    {
                        if (matrix.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Added;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuCell cell)
                    {
                        if (cell.SudokuMatrix.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Added;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuSolution solution)
                    {
                        if (solution.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Added;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else
                    {
                        if (dbEntry.Id == 0)
                        {
                            entry.State = EntityState.Added;
                        }
                        else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Object = entity;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetAsync(int id)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var query = new Game();

                query = await _context
                    .Games
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.Difficulty)
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.SudokuCells)
                    .Include(g => g.SudokuSolution)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (query != null)
                {
                    result.IsSuccess = true;
                    result.Object = query;
                }
                else
                {
                    result.IsSuccess = false;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetAllAsync()
        {
            var result = new RepositoryResponse();

            try
            {
                var query = new List<Game>();

                query = await _context
                    .Games
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.Difficulty)
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.SudokuCells)
                    .Include(g => g.SudokuSolution)
                    .ToListAsync();

                if (query.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Objects = [.. query.ConvertAll(g => (IDomainEntity)g)];
                }
                else
                {
                    result.IsSuccess = false;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }
        
        public async Task<IRepositoryResponse> GetAppGameAsync(int id, int appid)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                var query = new Game();

                query = await _context
                    .Games
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.Difficulty)
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.SudokuCells)
                    .Include(g => g.SudokuSolution)
                    .FirstOrDefaultAsync(g => g.Id == id && g.AppId == appid);

                if (query != null)
                {
                    result.IsSuccess = true;
                    result.Object = query;
                }
                else
                {
                    result.IsSuccess = false;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetAppGamesAsync(int appid)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                var query = new List<Game>();

                query = await _context
                    .Games
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.Difficulty)
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.SudokuCells)
                    .Include(g => g.SudokuSolution)
                    .Where(g => g.AppId == appid)
                    .ToListAsync();

                if (query.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Objects = query.ConvertAll(g => (IDomainEntity)g);
                }
                else
                {
                    result.IsSuccess = false;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetMyGameAsync(int userid, int gameid, int appid)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(userid, nameof(userid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userid, nameof(userid));

                ArgumentNullException.ThrowIfNull(gameid, nameof(gameid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gameid, nameof(gameid));

                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                var query = new Game();

                query = await _context
                .Games
                .Include(g => g.SudokuMatrix)
                .ThenInclude(g => g.Difficulty)
                .Include(g => g.SudokuMatrix)
                .ThenInclude(g => g.SudokuCells)
                .Include(g => g.SudokuSolution)
                .Where(g => g.AppId == appid)
                .FirstOrDefaultAsync(predicate:
                    g => g.Id == gameid
                    && g.AppId == appid
                    && g.UserId == userid);

                if (query != null)
                {
                    result.IsSuccess = true;
                    result.Object = query;
                }
                else
                {
                    result.IsSuccess = false;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetMyGamesAsync(int userid, int appid)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(userid, nameof(userid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userid, nameof(userid));

                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                if (await _context.Users.AnyAsync(u => u.Id == userid) && await _context.Apps.AnyAsync(a => a.Id == appid))
                {
                    var query = new List<Game>();

                    query = await _context
                        .Games
                        .Include(g => g.SudokuMatrix)
                        .ThenInclude(g => g.Difficulty)
                        .Include(g => g.SudokuMatrix)
                        .ThenInclude(g => g.SudokuCells)
                        .Include(g => g.SudokuSolution)
                        .Where(g => g.AppId == appid && g.UserId == userid)
                        .ToListAsync();

                    if (query.Count > 0)
                    {
                        result.IsSuccess = true;
                        result.Objects = query.ConvertAll(g => (IDomainEntity)g);
                    }
                    else
                    {
                        result.IsSuccess = false;
                    }

                    return result;
                }
                else
                {
                    result.IsSuccess = false;

                    return result;
                }
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> UpdateAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entity);

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

                var gameExists = await _context.Games.AnyAsync(g => g.Id == entity.Id);

                if (!gameExists)
                {
                    result.IsSuccess = false;

                    return result;
                }

                entity.DateUpdated = DateTime.UtcNow;
                    
                _context.Update(entity);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

                    if (dbEntry is Game)
                    {
                        if (dbEntry.Id == entity.Id)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuMatrix matrix)
                    {
                        if (matrix.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuCell cell)
                    {
                        if (cell.SudokuMatrix.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuSolution solution)
                    {
                        if (solution.Game.Id == entity.Id)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else
                    {
                        if (dbEntry.Id == 0)
                        {
                            entry.State = EntityState.Added;
                        }
                        else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Object = entity;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> UpdateRangeAsync(List<TEntity> entities)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entities, nameof(entities));

                var dateUpdated = DateTime.UtcNow;

                foreach (var entity in entities)
                {
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

                    if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                    {
                        entity.DateUpdated = dateUpdated;

                        _context.Attach(entity);
                    }
                    else
                    {
                        result.IsSuccess = false;

                        return result;
                    }

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is Game)
                        {
                            if (dbEntry.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                        else if (dbEntry is SudokuMatrix matrix)
                        {
                            if (matrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                        else if (dbEntry is SudokuCell cell)
                        {
                            if (cell.SudokuMatrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                        else if (dbEntry is SudokuSolution solution)
                        {
                            if (solution.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            if (dbEntry.Id == 0)
                            {
                                entry.State = EntityState.Added;
                            }
                            else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                            {
                                entry.State = EntityState.Detached;
                            }
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> DeleteAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entity);

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

                if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                {
                    _context.Remove(entity);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is Game)
                        {
                            if (dbEntry.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                            else
                            {
                                entry.State = EntityState.Unchanged;
                            }
                        }
                        else if (dbEntry is SudokuMatrix matrix)
                        {
                            if (matrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                            else
                            {
                                entry.State = EntityState.Unchanged;
                            }
                        }
                        else if (dbEntry is SudokuCell cell)
                        {
                            if (cell.SudokuMatrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                            else
                            {
                                entry.State = EntityState.Unchanged;
                            }
                        }
                        else if (dbEntry is SudokuSolution solution)
                        {
                            if (solution.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                            else
                            {
                                entry.State = EntityState.Unchanged;
                            }
                        }
                        else
                        {
                            if (dbEntry.Id == 0)
                            {
                                entry.State = EntityState.Added;
                            }
                            else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                            {
                                entry.State = EntityState.Detached;
                            }
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;

                    return result;
                }
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> DeleteRangeAsync(List<TEntity> entities)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entities);

                foreach (var entity in entities)
                {
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

                    if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                    {
                        _context.Remove(entity);
                    }
                    else
                    {
                        result.IsSuccess = false;

                        return result;
                    }

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is Game)
                        {
                            if (dbEntry.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                        }
                        else if (dbEntry is SudokuMatrix matrix)
                        {
                            if (matrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                        }
                        else if (dbEntry is SudokuCell cell)
                        {
                            if (cell.SudokuMatrix.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Deleted;
                            }
                        }
                        else if (dbEntry is SudokuSolution solution)
                        {
                            if (solution.Game.Id == entity.Id)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            if (dbEntry.Id == 0)
                            {
                                entry.State = EntityState.Added;
                            }
                            else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                            {
                                entry.State = EntityState.Detached;
                            }
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }
                }

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> DeleteMyGameAsync(int userid, int gameid, int appid)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(userid, nameof(userid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userid, nameof(userid));

                ArgumentNullException.ThrowIfNull(gameid, nameof(gameid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gameid, nameof(gameid));

                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                var gameExists = await _context.Users.AnyAsync(u => u.Id == userid) &&
                    await _context.Games.AnyAsync(g => g.Id == gameid) &&
                    await _context.Apps.AnyAsync(a => a.Id == appid);

                if (!gameExists)
                {
                    result.IsSuccess = false;

                    return result;
                }

                var query = new Game();

                query = await _context
                    .Games
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.Difficulty)
                    .Include(g => g.SudokuMatrix)
                    .ThenInclude(g => g.SudokuCells)
                    .FirstOrDefaultAsync(predicate:
                    g => g.Id == gameid
                    && g.AppId == appid
                    && g.UserId == userid);

                _context.Remove(query);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

                    if (dbEntry is Game)
                    {
                        if (dbEntry.Id == gameid)
                        {
                            entry.State = EntityState.Deleted;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuMatrix matrix)
                    {
                        if (matrix.Game.Id == gameid)
                        {
                            entry.State = EntityState.Deleted;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuCell cell)
                    {
                        if (cell.SudokuMatrix.Game.Id == gameid)
                        {
                            entry.State = EntityState.Deleted;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else if (dbEntry is SudokuSolution solution)
                    {
                        if (solution.Game.Id == gameid)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            entry.State = EntityState.Unchanged;
                        }
                    }
                    else
                    {
                        if (dbEntry.Id == 0)
                        {
                            entry.State = EntityState.Added;
                        }
                        else if (entry.State != EntityState.Deleted || entry.State != EntityState.Modified || entry.State != EntityState.Added)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<GamesRepository<Game>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<bool> HasEntityAsync(int id) => await _context.Games.AnyAsync(g => g.Id == id);
        #endregion
    }
}