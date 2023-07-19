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
    public class AppAdminsRepository<TEntity> : IAppAdminsRepository<TEntity> where TEntity : AppAdmin
    {
        #region Fields
        private readonly DatabaseContext _context;
        private readonly IRequestService _requestService;
        private readonly ILogger<AppAdminsRepository<AppAdmin>> _logger;
        #endregion

        #region Constructor
        public AppAdminsRepository(
            DatabaseContext context,
            IRequestService requestService,
            ILogger<AppAdminsRepository<AppAdmin>> logger)
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

            if (await _context.AppAdmins.AnyAsync(aa => aa.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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

                    result.Objects = query
                        .ConvertAll(aa => (IDomainEntity)aa)
                        .ToList();
                }

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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
                if (await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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

                    if (await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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
                if (await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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

                    if (await _context.AppAdmins.AnyAsync(d => d.Id == entity.Id))
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

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception e)
            {
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
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
                return ReposUtilities.ProcessException<AppAdminsRepository<AppAdmin>>(
                    _requestService, 
                    _logger, 
                    result, 
                    e);
            }
        }
        #endregion
    }
}
