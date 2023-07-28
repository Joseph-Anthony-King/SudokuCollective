using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos.Utilities;
using SudokuCollective.Core.Interfaces.Services;

namespace SudokuCollective.Repos
{
	public class SolutionsRepository<TEntity> : ISolutionsRepository<TEntity> where TEntity : SudokuSolution
	{
		#region Fields
		private readonly DatabaseContext _context;
		private readonly IRequestService _requestService;
		private readonly ILogger<SolutionsRepository<SudokuSolution>> _logger;
		#endregion

		#region Constructor
		public SolutionsRepository(
				DatabaseContext context,
				IRequestService requestService,
				ILogger<SolutionsRepository<SudokuSolution>> logger)
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

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
				{
					var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

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

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

				await _context.SaveChangesAsync();

				result.Object = entity;
				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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
				var query = await _context
						.SudokuSolutions
						.FirstOrDefaultAsync(s => s.Id == id);

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
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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
				List<SudokuSolution> query = await _context
						.SudokuSolutions
						.ToListAsync();

				if (query.Count == 0)
				{
					result.IsSuccess = false;
				}
				else
				{
					result.IsSuccess = true;
					result.Objects = query
							.ConvertAll(s => (IDomainEntity)s)
							.ToList();
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}


		public async Task<IRepositoryResponse> AddSolutionsAsync(List<ISudokuSolution> solutions)
		{
			if (solutions == null) throw new ArgumentNullException(nameof(solutions));

			var result = new RepositoryResponse();

			try
			{
				_context.AddRange(solutions.ConvertAll(s => (SudokuSolution)s));

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
				{
					var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

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

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

				await _context.SaveChangesAsync();

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetSolvedSolutionsAsync()
		{
			var result = new RepositoryResponse();

			try
			{
				var query = await _context
						.SudokuSolutions
						.Where(s => s.DateSolved > DateTime.MinValue)
						.ToListAsync();

				result.IsSuccess = true;
				result.Objects = query
						.ConvertAll(s => (IDomainEntity)s)
						.ToList();

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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
				if (await _context.SudokuSolutions.AnyAsync(r => r.Id == entity.Id))
				{
					try
					{
						_context.Update(entity);
					}
					catch
					{
						_context.Attach(entity);
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
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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

					if (await _context.SudokuSolutions.AnyAsync(d => d.Id == entity.Id))
					{
						_context.Attach(entity);
					}
					else
					{
						result.IsSuccess = false;

						return result;
					}
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

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

				await _context.SaveChangesAsync();

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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
				if (await _context.SudokuSolutions.AnyAsync(d => d.Id == entity.Id))
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

                        if (dbEntry is UserApp)
						{
							entry.State = EntityState.Modified;
						}
						else if (dbEntry is UserRole userRole)
						{
							if (userRole.RoleId == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
						}
						else
						{
                            // Otherwise do nothing...
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
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
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
				var roleIds = new List<int>();

				foreach (var entity in entities)
				{
					if (entity.Id == 0)
					{
						result.IsSuccess = false;

						return result;
					}

					if (await _context.SudokuSolutions.AnyAsync(d => d.Id == entity.Id))
					{
						_context.Remove(entity);
						roleIds.Add(entity.Id);
					}
					else
					{
						result.IsSuccess = false;

						return result;
					}
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

                    if (dbEntry is UserApp)
					{
						entry.State = EntityState.Modified;
					}
					else if (dbEntry is UserRole userRole)
					{
						if (roleIds.Contains(userRole.RoleId))
						{
							entry.State = EntityState.Deleted;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else
					{
                        // Otherwise do nothing...
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
				return ReposUtilities.ProcessException<SolutionsRepository<SudokuSolution>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> HasEntityAsync(int id) =>
				await _context.SudokuSolutions.AnyAsync(d => d.Id == id);
		#endregion
	}
}
