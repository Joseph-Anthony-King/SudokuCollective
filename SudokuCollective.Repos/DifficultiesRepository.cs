using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos.Utilities;

namespace SudokuCollective.Repos
{
    public class DifficultiesRepository<TEntity> : IDifficultiesRepository<TEntity> where TEntity : Difficulty
    {
        #region Fields
        private readonly DatabaseContext _context;
        private readonly IRequestService _requestService;
        private readonly ILogger<DifficultiesRepository<Difficulty>> _logger;
        #endregion

        #region Constructor
        public DifficultiesRepository(
            DatabaseContext context,
            IRequestService requestService,
            ILogger<DifficultiesRepository<Difficulty>> logger)
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

            try
            {
                if (await _context.Difficulties.AnyAsync(d => d.DifficultyLevel == entity.DifficultyLevel))
                {
                    result.IsSuccess = false;

                    return result;
                }

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
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
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
                var query = new Difficulty();

                query = await _context
                    .Difficulties
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (query == null)
                {
                    result.IsSuccess = false;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Object = query;
                }


                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> GetAllAsync()
        {
            var result = new RepositoryResponse();
            var query = new List<Difficulty>();

            try
            {
                query = await _context
                    .Difficulties
                    .Where(d =>
                        d.DifficultyLevel != DifficultyLevel.NULL
                        && d.DifficultyLevel != DifficultyLevel.TEST)
                    .OrderBy(d => d.DifficultyLevel)
                    .ToListAsync();

                if (query.Count == 0)
                {
                    result.IsSuccess = false;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Objects = query
                        .ConvertAll(d => (IDomainEntity)d)
                        .ToList();
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
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

            try
            {
                if (await _context.Difficulties.AnyAsync(d => d.Id == entity.Id))
                {
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
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<IRepositoryResponse> UpdateRangeAsync(List<TEntity> entities)
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

                    if (await _context.Difficulties.AnyAsync(d => d.Id == entity.Id))
                    {
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
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
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

            try
            {
                if (await _context.Difficulties.AnyAsync(d => d.Id == entity.Id))
                {
                    _context.Remove(entity);

                    if (entity.Matrices.Count == 0)
                    {
                        var games = await _context
                            .Games
                            .Include(g => g.SudokuMatrix)
                                .ThenInclude(m => m.SudokuCells)
                            .ToListAsync();

                        foreach (var game in games)
                        {
                            if (game.SudokuMatrix.DifficultyId == entity.Id)
                            {
                                _context.Remove(game);
                            }
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
                        else if (dbEntry is SudokuSolution)
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
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
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

                    if (await _context.Difficulties.AnyAsync(d => d.Id == entity.Id))
                    {
                        _context.Remove(entity);

                        if (entity.Matrices.Count == 0)
                        {
                            var games = await _context
                                .Games
                                .Include(g => g.SudokuMatrix)
                                    .ThenInclude(m => m.SudokuCells)
                                .ToListAsync();

                            foreach (var game in games)
                            {
                                if (game.SudokuMatrix.DifficultyId == entity.Id)
                                {
                                    _context.Remove(game);
                                }
                            }
                        }
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
                    else if (dbEntry is SudokuSolution)
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
                return ReposUtilities.ProcessException<DifficultiesRepository<Difficulty>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<bool> HasEntityAsync(int id) => 
            await _context.Difficulties.AnyAsync(d => d.Id == id);

        public async Task<bool> HasDifficultyLevelAsync(DifficultyLevel level) => 
            await _context.Difficulties.AnyAsync(d => d.DifficultyLevel == level);
        #endregion
    }
}
