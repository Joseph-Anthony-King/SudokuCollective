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
    public class GamesRepository<TEntity> : IGamesRepository<TEntity> where TEntity : Game
    {
        #region Fields
        private readonly DatabaseContext _context;
        private readonly IRequestService _requestService;
        private readonly ILogger<GamesRepository<Game>> _logger;
        #endregion

        #region Constructor
        public GamesRepository(
            DatabaseContext context,
            IRequestService requestService,
            ILogger<GamesRepository<Game>> logger)
        {
            _context = context;
            _requestService = requestService;
            _logger = logger;
        }
        #endregion

        #region Methods    
        public async Task<IRepositoryResponse> AddAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new RepositoryResponse();

            if (entity.Id != 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
                _context.Attach(entity);

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    if (dbEntry is UserApp)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else if (dbEntry is UserRole)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else
                    {
                        // Otherwise do nothing...
                    }
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

            if (id == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
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
                    query.SudokuMatrix.SudokuCells = query
                        .SudokuMatrix
                        .SudokuCells
                        .OrderBy(c => c.Index)
                        .ToList();

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
            var query = new List<Game>();

            try
            {
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
                    foreach (var game in query)
                    {
                        game.SudokuMatrix.SudokuCells = game
                            .SudokuMatrix
                            .SudokuCells
                            .OrderBy(c => c.Index)
                            .ToList();
                    }

                    result.IsSuccess = true;
                    result.Objects = query
                        .ConvertAll(g => (IDomainEntity)g)
                        .ToList();
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

            if (id == 0 || appid == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
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
                    query.SudokuMatrix.SudokuCells = query
                        .SudokuMatrix
                        .SudokuCells
                        .Where(c => c.Id != 0)
                        .OrderBy(c => c.Index)
                        .ToList();

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

            if (appid == 0)
            {
                result.IsSuccess = false;

                return result;
            }

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
                    .Where(g => g.AppId == appid)
                    .ToListAsync();

                if (query.Count > 0)
                {
                    foreach (var game in query)
                    {
                        game.SudokuMatrix.SudokuCells = game
                            .SudokuMatrix
                            .SudokuCells
                            .Where(c => c.Id != 0)
                            .OrderBy(c => c.Index)
                            .ToList();
                    }

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

            if (userid == 0 || gameid == 0 || appid == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
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
                    query.SudokuMatrix.SudokuCells = query
                        .SudokuMatrix
                        .SudokuCells
                        .Where(c => c.Id != 0)
                        .OrderBy(c => c.Index)
                        .ToList();

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

            if (userid == 0 || appid == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
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
                        foreach (var game in query)
                        {
                            game.SudokuMatrix.SudokuCells = game
                                .SudokuMatrix
                                .SudokuCells
                                .Where(c => c.Id != 0)
                                .OrderBy(c => c.Index)
                                .ToList();
                        }

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
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new RepositoryResponse();

            if (entity.Id == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
                if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                {
                    entity.DateUpdated = DateTime.UtcNow;

                    try
                    {
                        _context.Update(entity);
                    }
                    catch
                    {
                        _context.Attach(entity);
                    }

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        if (dbEntry is UserApp)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else if (dbEntry is UserRole)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            // Otherwise do nothing...
                        }
                    }

                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Object = entity;

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

        public async Task<IRepositoryResponse> UpdateRangeAsync(List<TEntity> entities)
        {
            var result = new RepositoryResponse();

            try
            {
                var dateUpdated = DateTime.UtcNow;

                foreach (var entity in entities)
                {
                    if (entity.Id == 0)
                    {
                        result.IsSuccess = false;

                        return result;
                    }

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
                }

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    if (dbEntry is UserApp)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else if (dbEntry is UserRole)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else
                    {
                        // Otherwise do nothing...
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
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new RepositoryResponse();

            if (entity.Id == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
                if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                {
                    _context.Remove(entity);

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        if (dbEntry is UserApp)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else if (dbEntry is UserRole)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            // Otherwise do nothing...
                        }
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
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            var result = new RepositoryResponse();

            try
            {
                foreach (var entity in entities)
                {
                    if (entity.Id == 0)
                    {
                        result.IsSuccess = false;

                        return result;
                    }

                    if (await _context.Games.AnyAsync(g => g.Id == entity.Id))
                    {
                        _context.Remove(entity);
                    }
                    else
                    {
                        result.IsSuccess = false;

                        return result;
                    }
                }

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    if (dbEntry is UserApp)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else if (dbEntry is UserRole)
                    {
                        entry.State = EntityState.Modified;
                    }
                    else
                    {
                        // Otherwise do nothing...
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

            if (userid == 0 || gameid == 0 || appid == 0)
            {
                result.IsSuccess = false;

                return result;
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Id == userid) && 
                    await _context.Games.AnyAsync(g => g.Id == gameid) &&
                    await _context.Apps.AnyAsync(a => a.Id == appid))
                {
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

                    foreach (var entry in _context.ChangeTracker.Entries())
                    {
                        var dbEntry = (IDomainEntity)entry.Entity;

                        if (dbEntry is UserApp)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else if (dbEntry is UserRole)
                        {
                            entry.State = EntityState.Modified;
                        }
                        else
                        {
                            // Otherwise do nothing...
                        }
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

        public async Task<bool> HasEntityAsync(int id) => await _context.Games.AnyAsync(g => g.Id == id);
        #endregion
    }
}