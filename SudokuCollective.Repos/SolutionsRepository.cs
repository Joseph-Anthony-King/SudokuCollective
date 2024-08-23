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
	public class SolutionsRepository<TEntity>(
            IDatabaseContext context,
            IRequestService requestService,
            ILogger<SolutionsRepository<TEntity>> logger) : ISolutionsRepository<TEntity> where TEntity : SudokuSolution
	{
		#region Fields
		private readonly DatabaseContext _context = (DatabaseContext)context;
		private readonly IRequestService _requestService = requestService;
		private readonly ILogger<SolutionsRepository<TEntity>> _logger = logger;
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

				await _context.SaveChangesAsync();

				result.Object = entity;
				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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
					result.Objects = [.. query.ConvertAll(s => (IDomainEntity)s)];
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> AddSolutionsAsync(List<ISudokuSolution> solutions)
		{
            ArgumentNullException.ThrowIfNull(solutions);

            var result = new RepositoryResponse();

			try
			{
				_context.AddRange(solutions.ConvertAll(s => (SudokuSolution)s));

				await _context.SaveChangesAsync();

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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
				result.Objects = [.. query.ConvertAll(s => (IDomainEntity)s)];

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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

                if (await _context.SudokuSolutions.AnyAsync(r => r.Id == entity.Id))
				{
					_context.Update(entity);

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
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> UpdateRangeAsync(List<TEntity> entities)
		{
            ArgumentNullException.ThrowIfNull(entities);

            var result = new RepositoryResponse();

			try
			{
				foreach (var entity in entities)
				{
					ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

					if (await _context.SudokuSolutions.AnyAsync(d => d.Id == entity.Id))
					{
						_context.Update(entity);
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
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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

                if (await _context.SudokuSolutions.AnyAsync(d => d.Id == entity.Id))
				{
					_context.Remove(entity);

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
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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

                var roleIds = new List<int>();

				foreach (var entity in entities)
				{
					ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

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

				await _context.SaveChangesAsync();

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<SolutionsRepository<TEntity>>(
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
