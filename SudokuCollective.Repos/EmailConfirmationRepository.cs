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
	public class EmailConfirmationsRepository<TEntity>(
            DatabaseContext context,
            IRequestService requestService,
            ILogger<EmailConfirmationsRepository<EmailConfirmation>> logger) : IEmailConfirmationsRepository<TEntity> where TEntity : EmailConfirmation
	{
		#region Fields
		private readonly DatabaseContext _context = context;
		private readonly IRequestService _requestService = requestService;
		private readonly ILogger<EmailConfirmationsRepository<EmailConfirmation>> _logger = logger;
        #endregion

        #region Methods
        public async Task<IRepositoryResponse> CreateAsync(TEntity entity)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(entity);

				ArgumentOutOfRangeException.ThrowIfNotEqual(0, entity.Id, nameof(entity.Id));

				if (await _context.EmailConfirmations
								.AnyAsync(pu => pu.Token.ToLower().Equals(entity.Token.ToLower())))
				{
					result.IsSuccess = false;

					return result;
				}

				_context.EmailConfirmations.Add(entity);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
				{
					var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }
					
					if (dbEntry is EmailConfirmation confirmation)
                    {
                        if (confirmation.Id == entity.Id)
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
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetAsync(string token)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

				var query = await _context
					.EmailConfirmations
					.FirstOrDefaultAsync(ec => ec.Token.ToLower().Equals(token.ToLower()));

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
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
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
				List<EmailConfirmation> query = await _context
					.EmailConfirmations
					.OrderBy(ec => ec.Id)
					.ToListAsync();

				if (query.Count == 0)
				{
					result.IsSuccess = false;
				}
				else
				{
					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(ec => (IDomainEntity)ec)];
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
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

                List<EmailConfirmation> tokenNotUniqueList = await _context.EmailConfirmations
					.Where(ec => ec.Token.ToLower().Equals(entity.Token.ToLower()) && ec.Id != entity.Id)
					.ToListAsync();

				if (await _context.EmailConfirmations
						.AnyAsync(ec => ec.Id == entity.Id) && tokenNotUniqueList.Count == 0)
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

                        if (dbEntry is EmailConfirmation confirmation)
                        {
                            if (confirmation.Id == entity.Id)
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
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
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

                if (await _context.EmailConfirmations.AnyAsync(ec => ec.Id == entity.Id))
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

                        if (dbEntry is EmailConfirmation confirmation)
                        {
                            if (confirmation.Id == entity.Id)
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
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> HasEntityAsync(int id) =>
			await _context.EmailConfirmations.AnyAsync(ec => ec.Id == id);

		public async Task<bool> HasOutstandingEmailConfirmationAsync(int userId, int appid) =>
			await _context.EmailConfirmations.AnyAsync(ec => ec.UserId == userId && ec.AppId == appid);

		public async Task<IRepositoryResponse> RetrieveEmailConfirmationAsync(int userId, int appid)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(userId, nameof(userId));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(appid, nameof(appid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appid, nameof(appid));

                var query = await _context
					.EmailConfirmations
					.FirstOrDefaultAsync(ec =>
							ec.UserId == userId &&
							ec.AppId == appid);

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
				return ReposUtilities.ProcessException<EmailConfirmationsRepository<EmailConfirmation>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}
		#endregion
	}
}
