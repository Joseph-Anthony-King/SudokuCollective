using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
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
	public class UsersRepository<TEntity>(
                IDatabaseContext context,
                IRequestService requestService,
                ILogger<UsersRepository<TEntity>> logger) : IUsersRepository<TEntity> where TEntity : User
	{
		#region Fields
		private readonly DatabaseContext _context = (DatabaseContext)context;
		private readonly IRequestService _requestService = requestService;
		private readonly ILogger<UsersRepository<TEntity>> _logger = logger;
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

                foreach (var userApp in entity.Apps)
                {
                    _context.UsersApps.Add(userApp);
                }

                foreach (var userRole in entity.Roles)
				{
					_context.UsersRoles.Add(userRole);
				}

				await _context.SaveChangesAsync();

				result.IsSuccess = true;
				result.Object = entity;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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
					.Users
					.Include(u => u.Apps)
					.Include(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuSolution)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuMatrix)
					.ThenInclude(m => m.SudokuCells)
					.FirstOrDefaultAsync(u => u.Id == id);

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByUserNameAsync(string username)
		{
			var result = new RepositoryResponse();

			try
			{
                ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));

				var query = await _context
					.Users
					.Include(u => u.Apps)
					.Include(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuSolution)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuMatrix)
					.ThenInclude(m => m.SudokuCells)
					.FirstOrDefaultAsync(u => u.UserName.ToLower().Equals(username.ToLower()));

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByEmailAsync(string email)
		{
			var result = new RepositoryResponse();

			try
			{
                ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));

				/* Since emails are encrypted we have to pull all users
				 * first and then search by email */
				var users = await _context
					.Users
					.Include(u => u.Apps)
					.Include(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuSolution)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuMatrix)
					.ThenInclude(m => m.SudokuCells)
					.ToListAsync();

				if (users == null)
				{
					result.IsSuccess = false;
				}

				var query = users.FirstOrDefault(u => u.Email.ToLower().Equals(email.ToLower()));

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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
				List<User> query = await _context
					.Users
					.Include(u => u.Apps)
					.ThenInclude(ua => ua.App)
					.Include(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuSolution)
					.Include(u => u.Games)
					.ThenInclude(g => g.SudokuMatrix)
					.ThenInclude(m => m.SudokuCells)
					.OrderBy(u => u.Id)
					.ToListAsync();

				if (query.Count == 0)
				{
					result.IsSuccess = false;
				}
				else
				{
					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(u => (IDomainEntity)u)];
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyAppsAsync(int id)
		{
			var result = new RepositoryResponse();

			try
			{
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

				var query = new List<App>();

				query = await _context
					.Apps
					.Where(a => a.OwnerId == id)
					.Include(a => a.UserApps)
					.ThenInclude(ua => ua.User)
					.ThenInclude(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.OrderBy(a => a.Id)
					.ToListAsync();

				if (query.Count != 0)
				{
					// Filter games by app
					foreach (var app in query)
					{
						foreach (var userApp in app.UserApps)
						{
							userApp.User.Games = [];

							userApp.User.Games = await _context
								.Games
								.Include(g => g.SudokuMatrix)
								.ThenInclude(g => g.Difficulty)
								.Include(g => g.SudokuMatrix)
								.ThenInclude(m => m.SudokuCells)
								.Include(g => g.SudokuSolution)
								.Where(g => g.AppId == userApp.AppId && g.UserId == userApp.UserId)
								.ToListAsync();

							app.Users.Add((UserDTO)userApp.User.Cast<UserDTO>());
						}
					}

					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(a => (IDomainEntity)a)];
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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

                entity.DateUpdated = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				result.IsSuccess = true;
				result.Object = entity;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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

                var dateUpdated = DateTime.UtcNow;

				foreach (var entity in entities)
				{
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

					if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
					{
						entity.DateUpdated = dateUpdated;
					}
					else
					{
						result.IsSuccess = false;

						return result;
                    }
                }

				await _context.SaveChangesAsync();

				result.IsSuccess = true;
				result.Objects = entities.ConvertAll(a => (IDomainEntity)a);

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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

                if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
				{
					_context.Games.RemoveRange(
						await _context
							.Games
							.Include(g => g.SudokuMatrix)
							.ThenInclude(m => m.SudokuCells)
							.Include(g => g.SudokuSolution)
							.Where(g => g.UserId == entity.Id)
							.ToListAsync());
					_context.Apps.RemoveRange(
						await _context
							.Apps
							.Where(a => a.OwnerId == entity.Id)
							.ToListAsync());
                    _context.Users.Remove(
						await _context
							.Users
							.FirstOrDefaultAsync(user => user.Id == entity.Id));

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
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

					if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
					{
                        _context.RemoveRange(
							await _context
								.Games
								.Where(g => g.UserId == entity.Id)
								.ToListAsync());
						_context.RemoveRange(
							await _context
								.Apps
								.Where(a => a.OwnerId == entity.Id)
								.ToListAsync());
                        _context.Users.Remove(
                            await _context
                                .Users
                                .FirstOrDefaultAsync(user => user.Id == entity.Id));
                    }
                    else
					{
						result.IsSuccess = false;

						return result;
                    }

					await _context.SaveChangesAsync();
				}

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> HasEntityAsync(int id) =>
			await _context.Users.AnyAsync(u => u.Id == id);

        public async Task<bool> ActivateAsync(int id)
		{
			try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var user = await _context.Users
					.Include(u => u.Roles)
					.Include(u => u.Apps)
					.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return false;
                }

				user.ActivateUser();

				await _context.SaveChangesAsync();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> DeactivateAsync(int id)
		{
			try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var user = await _context.Users
                    .Include(u => u.Roles)
                    .Include(u => u.Apps)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return false;
                }

				user.DeactiveUser();

				await _context.SaveChangesAsync();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> IsUserRegisteredAsync(int id)
		{
			if (id == 0)
			{
				return false;
			}
			else
            {
                return await _context.Users.AnyAsync(u => u.Id == id);
            }
		}

		public async Task<IRepositoryResponse> AddRoleAsync(int userId, int roleId)
		{
			var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId, nameof(roleId));

                if (await _context.Users.AnyAsync(u => u.Id == userId) && await _context.Roles.AnyAsync(r => r.Id == roleId) &&
                        await _context.Users.AnyAsync(u => u.Id == userId && !u.Roles.Any(ur => ur.RoleId == roleId)))
                {
					var user = await _context
						.Users
						.Include(u => u.Apps) 
						.ThenInclude(a => a.App)
						.Include(u => u.Roles)
                        .ThenInclude(r => r.Role)
                        .FirstOrDefaultAsync(u => u.Id == userId);

					var role = await _context
						.Roles
						.FirstOrDefaultAsync(r => r.Id == roleId);

					var userRole = new UserRole
					{
						User = user,
						UserId = user.Id,
						Role = role,
						RoleId = role.Id
					};

                    _context.UsersRoles.Add(userRole);

                    await _context.SaveChangesAsync();

					result.IsSuccess = true;
					result.Object = userRole;

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> AddRolesAsync(int userId, List<int> roleIds)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(roleIds);

                var user = await _context
					.Users
					.FirstOrDefaultAsync(u => u.Id == userId);

				if (user != null)
				{
					var newUserRoleIds = new List<int>();

					foreach (var roleId in roleIds)
                    {
                        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId, nameof(roleId));

                        if (await _context.Roles.AnyAsync(r => r.Id == roleId) && !user.Roles.Any(ur => ur.RoleId == roleId))
                        {
                            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

                            var userRole = new UserRole
							{
								UserId = user.Id,
								User = user,
								RoleId = role.Id,
								Role = role
							};

							_context.UsersRoles.Add(userRole);

							result.Objects.Add(userRole);

							newUserRoleIds.Add((int)userRole.Id);
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
				else
				{
					result.IsSuccess = false;

					return result;
				}
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveRoleAsync(int userId, int roleId)
		{
			var result = new RepositoryResponse();

			try
			{
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId, nameof(roleId));

                var userRole = await _context
					.UsersRoles
					.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

				if (userRole != null)
                {
					_context.UsersRoles.Remove(userRole);

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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveRolesAsync(int userId, List<int> roleIds)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(roleIds);

                if (await _context.Users.AnyAsync(u => u.Id == userId))
                {
					foreach (var roleId in roleIds)
					{
                        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId, nameof(roleId));

						if (await _context
							.UsersRoles
							.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId) == false)
						{
							result.IsSuccess = false;

							return result;
                        }

						var userRole = await _context
							.UsersRoles
							.FirstOrDefaultAsync(ur => ur.RoleId == roleId && ur.UserId == userId);

						_context.UsersRoles.Remove(userRole);
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
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> PromoteToAdminAsync(int id)
		{
			try
			{
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

				var user = await _context
					.Users
					.FirstOrDefaultAsync(u => u.Id == id);

				if (user != null)
				{
					var role = await _context
						.Roles
						.FirstOrDefaultAsync(r => r.RoleLevel == RoleLevel.ADMIN);

					var userRole = new UserRole
					{
						UserId = user.Id,
						User = user,
						RoleId = role.Id,
						Role = role
					};

					_context.UsersRoles.Add(userRole);

					await _context.SaveChangesAsync();

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<string> GetAppLicenseAsync(int appId) =>
			await _context
				.Apps
				.Where(a => a.Id == appId)
				.Select(a => a.License)
				.FirstOrDefaultAsync();

		public async Task<IRepositoryResponse> ConfirmEmailAsync(IEmailConfirmation emailConfirmation)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(emailConfirmation);

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(emailConfirmation.Id, nameof(emailConfirmation.Id));

                if (await _context
					.EmailConfirmations
					.AnyAsync(ec => ec.Id == emailConfirmation.Id))
				{
					var user = await _context
						.Users
						.Include(u => u.Apps)
						.ThenInclude(ua => ua.App)
						.ThenInclude(a => a.SMTPServerSettings)
						.Include(u => u.Roles)
						.ThenInclude(ur => ur.Role)
						.Include(u => u.Games)
						.ThenInclude(g => g.SudokuSolution)
						.Include(u => u.Games)
						.ThenInclude(g => g.SudokuMatrix)
						.ThenInclude(m => m.SudokuCells)
						.FirstOrDefaultAsync(u => u.Id == emailConfirmation.UserId);

					if (user != null)
					{
						user.DateUpdated = DateTime.UtcNow;
						user.IsEmailConfirmed = true;

                        await _context.SaveChangesAsync();

						result.IsSuccess = true;
						result.Object = user;

						return result;
					}
					else
					{
						result.IsSuccess = false;

						return result;
					}
				}
				else
				{
					result.IsSuccess = false;

					return result;
				}
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> UpdateEmailAsync(IEmailConfirmation emailConfirmation)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(emailConfirmation);

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(emailConfirmation.Id, nameof(emailConfirmation.Id));

                if (await _context
						.EmailConfirmations
						.AnyAsync(ec => ec.Id == emailConfirmation.Id))
				{
					var user = await _context
						.Users
						.Include(u => u.Apps)
						.ThenInclude(ua => ua.App)
						.ThenInclude(a => a.SMTPServerSettings)
						.Include(u => u.Roles)
						.ThenInclude(ur => ur.Role)
						.Include(u => u.Games)
						.ThenInclude(g => g.SudokuSolution)
						.Include(u => u.Games)
						.ThenInclude(g => g.SudokuMatrix)
						.ThenInclude(m => m.SudokuCells)
						.FirstOrDefaultAsync(u => u.Id == emailConfirmation.UserId);

					if (user != null)
					{
						if (!(bool)emailConfirmation.OldEmailAddressConfirmed)
						{
							emailConfirmation.OldEmailAddressConfirmed = true;
                            user.Email = emailConfirmation.NewEmailAddress;
                        }
						else
                        {
                            user.ReceivedRequestToUpdateEmail = false;
                        }

						user.DateUpdated = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

						result.IsSuccess = true;
						result.Object = user;

						return result;
					}
					else
					{
						result.IsSuccess = false;

						return result;
					}
				}
				else
				{
					result.IsSuccess = false;

					return result;
				}
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> IsEmailUniqueAsync(string email)
		{
            ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));

			List<string> emails = await _context.Users.Select(u => u.Email).ToListAsync();

			if (emails.Count > 0)
			{
				var result = true;

				foreach (var e in emails)
				{
					if (e.ToLower().Equals(email.ToLower()))
					{
						result = false;
					}
				}

				return result;
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> IsUpdatedEmailUniqueAsync(int userId, string email)
		{
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            ArgumentOutOfRangeException.ThrowIfNegative(userId, nameof(userId));

            ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));

			List<string> emails = await _context
					.Users
					.Where(u => u.Id != userId)
					.Select(u => u.Email)
					.ToListAsync();

			if (emails.Count > 0)
			{
				var result = true;

				foreach (var e in emails)
				{
					if (e.ToLower().Equals(email.ToLower()))
					{
						result = false;
					}
				}

				return result;
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> IsUserNameUniqueAsync(string username)
		{
            ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));

			List<string> names = await _context.Users.Select(u => u.UserName).ToListAsync();

			if (names.Count > 0)
			{
				var result = true;

				foreach (var name in names)
				{
					if (name.ToLower().Equals(username.ToLower()))
					{
						result = false;
					}
				}

				return result;
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> IsUpdatedUserNameUniqueAsync(int userId, string username)
        {
            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            ArgumentOutOfRangeException.ThrowIfNegative(userId, nameof(userId));

            ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));

            List<string> names = await _context
				.Users
				.Where(u => u.Id != userId)
				.Select(u => u.UserName)
				.ToListAsync();

			if (names.Count > 0)
			{
				var result = true;

				foreach (var name in names)
				{
					if (name.ToLower().Equals(username.ToLower()))
					{
						result = false;
					}
				}

				return result;
			}
			else
			{
				return false;
			}
		}
		#endregion
	}
}
