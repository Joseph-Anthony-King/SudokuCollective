using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos.Utilities;

namespace SudokuCollective.Repos
{
	public class PasswordResetsRepository<TEntity> : IPasswordResetsRepository<TEntity> where TEntity : PasswordReset
	{
		#region Fields
		private readonly DatabaseContext _context;
		private readonly IRequestService _requestService;
		private readonly ILogger<PasswordResetsRepository<PasswordReset>> _logger;
		#endregion

		#region Constructor
		public PasswordResetsRepository(
				DatabaseContext context,
				IRequestService requestService,
				ILogger<PasswordResetsRepository<PasswordReset>> logger)
		{
			_context = context;
			_requestService = requestService;
			_logger = logger;
		}
		#endregion

		#region Methods
		public async Task<IRepositoryResponse> CreateAsync(TEntity entity)
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
				if (await _context.PasswordResets
					.AnyAsync(pu => pu.Token.ToLower().Equals(entity.Token.ToLower())))
				{
					result.IsSuccess = false;

					return result;
				}

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

                    if (dbEntry is PasswordReset)
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
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetAsync(string token)
		{
			if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

			var result = new RepositoryResponse();

			try
			{
				var query = await _context
					.PasswordResets
					.ToListAsync();

				var passwordReset = query.FirstOrDefault(pr => pr.Token.ToLower().Equals(token.ToLower()));

				if (passwordReset == null)
				{
					result.IsSuccess = false;
				}
				else
				{
					result.IsSuccess = true;
					result.Object = passwordReset;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
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
				List<PasswordReset> query = await _context
					.PasswordResets
					.OrderBy(ec => ec.Id)
					.ToListAsync();

				if (query.Count == 0)
				{
					result.IsSuccess = false;
				}
				else
				{
					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(pu => (IDomainEntity)pu)];
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
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
				if (await _context.PasswordResets.AnyAsync(a => a.Id == entity.Id))
				{
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

                        if (dbEntry is PasswordReset)
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
				else
				{
					result.IsSuccess = false;

					return result;
				}
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
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
				if (await _context.PasswordResets.AnyAsync(pu => pu.Id == entity.Id))
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

                        if (dbEntry is PasswordReset)
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
				else
				{
					result.IsSuccess = false;

					return result;
				}
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> HasEntityAsync(int id) =>
			await _context.PasswordResets.AnyAsync(ec => ec.Id == id);

		public async Task<bool> HasOutstandingPasswordResetAsync(int userId, int appid) =>
			await _context
				.PasswordResets
				.AnyAsync(pw => pw.UserId == userId && pw.AppId == appid);

		public async Task<IRepositoryResponse> RetrievePasswordResetAsync(int userId, int appid)
		{
			var result = new RepositoryResponse();

			if (userId == 0 || appid == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var query = await _context
					.PasswordResets
					.FirstOrDefaultAsync(pw =>
						pw.UserId == userId &&
						pw.AppId == appid);

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
				return ReposUtilities.ProcessException<PasswordResetsRepository<PasswordReset>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}
		#endregion
	}
}
