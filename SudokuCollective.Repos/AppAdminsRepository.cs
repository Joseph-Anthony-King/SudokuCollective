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
    public class AppAdminsRepository<TEntity>(
        IDatabaseContext context,
        IRequestService requestService,
        ILogger<AppAdminsRepository<TEntity>> logger) : IAppAdminsRepository<TEntity> where TEntity : AppAdmin
    {
        #region Fields
        private readonly DatabaseContext _context = (DatabaseContext)context;
        private readonly IRequestService _requestService = requestService;
        private readonly ILogger<AppAdminsRepository<TEntity>> _logger = logger;
        #endregion

        #region Methods
        public async Task<IRepositoryResponse> AddAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(entity);

                ArgumentOutOfRangeException.ThrowIfNotEqual(0, entity.Id, nameof(entity));

                if (await _context.AppAdmins.AnyAsync(aa => aa.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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

                var query = await _context
                    .AppAdmins
                    .FirstOrDefaultAsync(aa => aa.Id == id);

                result.Object = query;

                if (query == null)
                {
                    result.IsSuccess = false;
                }
                else
                {
                    result.IsSuccess = true;
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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
                List<AppAdmin> query = await _context
                    .AppAdmins
                    .ToListAsync();

                if (query.Count == 0)
                {
                    result.IsSuccess = false;
                }
                else
                {
                    result.IsSuccess = true;

                    result.Objects = [.. query.ConvertAll(aa => (IDomainEntity)aa)];
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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

                var isAdmin = await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id);

                if (!isAdmin)
                {
                    result.IsSuccess = false;

                    return result;
                }

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Object = entity;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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
                    if (entity.Id == 0)
                    {
                        result.IsSuccess = false;

                        return result;
                    }

                    if (!await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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

                var isAdmin = await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id);

                if (!isAdmin)
                {
                    result.IsSuccess = false;

                    return result;
                }

                _context.AppAdmins.Remove(
                    await _context.AppAdmins
                        .FirstOrDefaultAsync(appAdmin => appAdmin.Id == entity.Id));

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Object = entity;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
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
                    if (entity.Id == 0)
                    {
                        result.IsSuccess = false;

                        return result;
                    }

                    if (await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
                    {
                        _context.AppAdmins.Remove(
                            await _context.AppAdmins
                                .FirstOrDefaultAsync(appAdmin => appAdmin.Id == entity.Id));
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
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }

        public async Task<bool> HasEntityAsync(int id) => 
            await _context.AppAdmins.AnyAsync(aa => aa.Id == id);

        public async Task<bool> HasAdminRecordAsync(int appId, int userId) => 
            await _context
                .AppAdmins
                .AnyAsync(aa => aa.AppId == appId && aa.UserId == userId);

        public async Task<IRepositoryResponse> GetAdminRecordAsync(int appId, int userId)
        {
            var result = new RepositoryResponse();

            try
            {
                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var query = await _context
                    .AppAdmins
                    .FirstOrDefaultAsync(aa => aa.AppId == appId && aa.UserId == userId);

                if (query == null)
                {
                    result.IsSuccess = false;

                    return result;
                }

                result.IsSuccess = true;
                result.Object = query;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<TEntity>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }
        #endregion
    }
}
