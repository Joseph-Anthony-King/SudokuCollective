using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos.Utilities;
using SudokuCollective.Core.Interfaces.Services;

namespace SudokuCollective.Repos
{
    public class RolesRepository<TEntity>(
        IDatabaseContext context,
        IRequestService requestService,
        ILogger<RolesRepository<TEntity>> logger) : IRolesRepository<TEntity> where TEntity : Role
    {
        #region Fields
        private readonly DatabaseContext _context = (DatabaseContext)context;
        private readonly IRequestService _requestService = requestService;
        private readonly ILogger<RolesRepository<TEntity>> _logger = logger;
        #endregion

        #region Methods
        public async Task<IRepositoryResponse> AddAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entity);

                ArgumentOutOfRangeException.ThrowIfNotEqual(0, entity.Id, nameof(entity.Id));

                if (await _context.Roles.AnyAsync(r => r.RoleLevel == entity.RoleLevel))
                {
                    result.IsSuccess = false;

                    return result;
                }

                _context.Attach(entity);

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Object = entity;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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

                var query = new Role();

                query = await _context
                    .Roles
                    .FirstOrDefaultAsync(r => r.Id == id);

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
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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
                var query = new List<Role>();

                query = await _context
                    .Roles
                    .Where(r => r.RoleLevel != RoleLevel.NULL)
                    .OrderBy(r => r.RoleLevel)
                    .ToListAsync();

                if (query.Count == 0)
                {
                    result.IsSuccess = false;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Objects = [.. query.ConvertAll(r => (IDomainEntity)r)];
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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

                if (await _context.Roles.AnyAsync(r => r.Id == entity.Id))
                {
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
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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
                ArgumentNullException.ThrowIfNull(entities);

                foreach (var entity in entities)
                {
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

                    if (!await _context.Roles.AnyAsync(d => d.Id == entity.Id))
                    {
                        result.IsSuccess = false;

                        return result;
                    }
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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

                if (await _context.Roles.AnyAsync(d => d.Id == entity.Id))
                {
                    _context.Roles.Remove(
                        await _context.Roles
                            .FirstOrDefaultAsync(role => role.Id == entity.Id));

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
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
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

                    if (await _context.Roles.AnyAsync(d => d.Id == entity.Id))
                    {
                        _context.Remove(
                            await _context.Roles
                                .FirstOrDefaultAsync(role => role.Id == entity.Id));
                    }
                    else
                    {
                        result.IsSuccess = false;

                        return result;
                    }
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<RolesRepository<TEntity>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e); 
            }
        }

        public async Task<bool> HasEntityAsync(int id) => 
            await _context.Roles.AnyAsync(r => r.Id == id);

        public async Task<bool> HasRoleLevelAsync(RoleLevel level) => 
            await _context.Roles.AnyAsync(r => r.RoleLevel == level);

        public async Task<bool> IsListValidAsync(List<int> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var result = true;

            foreach (var id in ids)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var isIdValid = await _context.Roles.AnyAsync(r => r.Id == id);

                if (!isIdValid)
                {
                    result = false;
                }
            }

            return result;
        }
        #endregion
    }
}
