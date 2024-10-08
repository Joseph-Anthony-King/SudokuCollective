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
    public class AppsRepository<TEntity>(
		IDatabaseContext context,
		IRequestService requestService,
		ILogger<AppsRepository<TEntity>> logger) : IAppsRepository<TEntity> where TEntity : App
	{
		#region Fields
		private readonly DatabaseContext _context = (DatabaseContext)context;
		private readonly IRequestService _requestService = requestService;
		private readonly ILogger<AppsRepository<TEntity>> _logger = logger;
        #endregion

        #region Methods
        public async Task<IRepositoryResponse> AddAsync(TEntity entity)
        {
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(entity);

				ArgumentOutOfRangeException.ThrowIfNotEqual(0, entity.Id, nameof(entity.Id));

                // Add connection between the app and the user
                var userApp = new UserApp
				{
					UserId = entity.OwnerId,
					AppId = entity.Id
				};

				entity.UserApps.Add(userApp);

                _context.Attach(entity);

				await _context.SaveChangesAsync();

				// Ensure that the owner has admin priviledges, if not they will be promoted
				var addAdminRole = true;
                var user = await _context
                    .Users
                    .FirstOrDefaultAsync(u => u.Id == entity.OwnerId);

                foreach (var userRole in user.Roles)
				{
					userRole.Role = await _context
						.Roles
						.FirstOrDefaultAsync(roleDbSet => roleDbSet.Id == userRole.RoleId);

					if (userRole.Role.RoleLevel == RoleLevel.ADMIN)
					{
						addAdminRole = false;
					}
				}

				// Promote user to admin if user is not already
				if (addAdminRole)
				{
					var adminRole = await _context
						.Roles
						.FirstOrDefaultAsync(r => r.RoleLevel == RoleLevel.ADMIN);

					var newUserAdminRole = new UserRole
					{
						UserId = user.Id,
						RoleId = adminRole.Id
					};

                    _context.UsersRoles.Add(newUserAdminRole);

                    var appAdmin = new AppAdmin
					{
						AppId = entity.Id,
						UserId = user.Id
					};

					_context.AppAdmins.Add(appAdmin);
                }

                await _context.SaveChangesAsync();

                result.Object = entity;
				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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
					.Apps
					.Include(a => a.SMTPServerSettings)
					.FirstOrDefaultAsync(a => a.Id == id);

				if (query != null)
                {
                    query.UserApps = await _context.UsersApps
                        .Where(ua => ua.AppId == query.Id)
                        .Include(ua => ua.User)
                        .ThenInclude(u => u.Roles)
                        .ThenInclude(r => r.Role)
                        .ToListAsync();

                    foreach (var userApp in query.UserApps)
					{
						userApp.User = await _context.Users
							.Where(u => u.Id == userApp.UserId)
							.FirstOrDefaultAsync();

						foreach (var userRole in userApp.User.Roles)
						{
							userRole.Role = await _context
								.Roles
								.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
						}

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

						query.Users.Add((UserDTO)userApp.User.Cast<UserDTO>());
					}

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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByLicenseAsync(string license)
		{
			var result = new RepositoryResponse();

			try
            {
				ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

				var apps = await _context
					.Apps
					.Include(a => a.SMTPServerSettings)
					.ToListAsync();

				var query = apps
					.Where(a => a.License.Equals(license, StringComparison.CurrentCultureIgnoreCase))
					.FirstOrDefault();

				if (query != null)
                {
                    query.UserApps = await _context.UsersApps
                        .Where(ua => ua.AppId == query.Id)
						.Include(ua => ua.User)
						.ThenInclude(u => u.Roles)
						.ThenInclude(r => r.Role)
                        .ToListAsync();

                    foreach (var userApp in query.UserApps)
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

						query.Users.Add((UserDTO)userApp.User.Cast<UserDTO>());
					}

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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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
				var query  = await _context
					.Apps
					.Include(a => a.UserApps)
					.ThenInclude(ua => ua.User)
					.ThenInclude(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(a =>a.SMTPServerSettings)
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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyAppsAsync(int ownerId)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(ownerId, nameof(ownerId));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ownerId, nameof(ownerId));

				var query = await _context
					.Apps
					.Where(a => a.OwnerId == ownerId)
					.Include(a => a.UserApps)
					.ThenInclude(ua => ua.User)
					.ThenInclude(u => u.Roles)
					.ThenInclude(ur => ur.Role)
					.Include(a => a.SMTPServerSettings)
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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyRegisteredAppsAsync(int userId)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(userId, nameof(userId));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

				var query = await _context
					.Apps
                    .Where(a => a.UserApps.Any(ua => ua.UserId == userId) && a.OwnerId != userId)
                    .Include(a => a.UserApps)
                    .ThenInclude(ua => ua.User)
                    .ThenInclude(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                    .Include(a => a.SMTPServerSettings)
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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetAppUsersAsync(int id)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(id, nameof(id));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var hasApp = await _context.Apps.AnyAsync(a => a.Id == id);

                if (!hasApp)
                {
                    result.IsSuccess = false;

                    return result;
                }

                var query = new List<User>();

				query = await _context
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
					.Where(u => u.Apps.Any(ua => ua.AppId == id))
					.OrderBy(u => u.Id)
					.ToListAsync();

				if (query.Count != 0)
				{
					foreach (var user in query)
					{
						// Filter games by app
						user.Games = [];

						user.Games = await _context
							.Games
							.Where(g => g.AppId == id && g.UserId == user.Id)
							.ToListAsync();

                        // Filter roles by app
                        var appAdmins = await _context
							.AppAdmins
							.Where(aa => aa.AppId == id && aa.UserId == user.Id)
							.ToListAsync();

						var filteredRoles = new List<UserRole>();

						foreach (var ur in user.Roles)
						{
							if (ur.Role.RoleLevel != RoleLevel.ADMIN)
							{
								filteredRoles.Add(ur);
							}
							else
							{
								if (appAdmins.Any(aa => aa.AppId == id && aa.UserId == user.Id && aa.IsActive))
								{
									filteredRoles.Add(ur);
								}
							}
						}

						user.Roles = filteredRoles;
					}

					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(u => (IDomainEntity)u)];
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetNonAppUsersAsync(int id)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(id, nameof(id));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

				var hasApp = await _context.Apps.AnyAsync(a => a.Id == id);

				if (!hasApp)
                {
                    result.IsSuccess = false;

					return result;
                }

				var query = new List<User>();

				query = await _context
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
					.Where(u => !u.Apps.Any(ua => ua.AppId == id))
					.OrderBy(u => u.Id)
                    .ToListAsync();

                if (query.Count != 0)
				{
					foreach (var user in query)
					{
						// Filter games by app
						user.Games = [];

						user.Games = await _context
							.Games
							.Where(g => g.AppId == id && g.UserId != user.Id)
                            .ToListAsync();
                    }

					result.IsSuccess = true;
					result.Objects = [.. query.ConvertAll(u => (IDomainEntity)u)];
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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

				if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
				{
					entity.DateUpdated = DateTime.UtcNow;

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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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

					if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
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

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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

                if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
				{
					_context.Games.RemoveRange(
						await _context
							.Games
							.Include(g => g.SudokuMatrix)
							.ThenInclude(g => g.Difficulty)
							.Include(g => g.SudokuMatrix)
							.ThenInclude(m => m.SudokuCells)
							.Where(g => g.AppId == entity.Id)
							.ToListAsync());

					_context.Remove(
						await _context
							.Apps.FirstOrDefaultAsync(app => app.Id == entity.Id));

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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
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

                    if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
					{
						_context.Games.RemoveRange(
							await _context
								.Games
								.Include(g => g.SudokuMatrix)
								.ThenInclude(g => g.Difficulty)
								.Include(g => g.SudokuMatrix)
								.ThenInclude(m => m.SudokuCells)
								.Where(g => g.AppId == entity.Id)
								.ToListAsync());

                        _context.Remove(
                            await _context
                                .Apps.FirstOrDefaultAsync(app => app.Id == entity.Id));
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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> ResetAsync(TEntity entity)
		{
            var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(entity);

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entity.Id, nameof(entity.Id));

				_context.Games.RemoveRange(
					await _context
						.Games
						.Include(g => g.SudokuMatrix)
						.ThenInclude(g => g.Difficulty)
						.Include(g => g.SudokuMatrix)
						.ThenInclude(m => m.SudokuCells)
						.Where(g => g.AppId == entity.Id)
						.ToListAsync());

                await _context.SaveChangesAsync();

				result.IsSuccess = true;
				result.Object = await _context
						.Apps
						.FirstOrDefaultAsync(a => a.Id == entity.Id);

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> AddAppUserAsync(int userId, string license)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(userId, nameof(userId));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

				ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

				var user = (User)await _context
					.Users
					.FirstOrDefaultAsync(u => u.Id == userId);

				/* Since licenses are encrypted we have to pull all apps
				 * first and then search by license */
				var apps = await _context
					.Apps
					.ToListAsync();

				var app = apps.FirstOrDefault(a => a.License.ToLower().Equals(license.ToLower()));

				if (user == null || app == null)
				{
					result.IsSuccess = false;

					return result;
				}

				var userApp = new UserApp
				{
					User = user,
					UserId = user.Id,
					App = app,
					AppId = app.Id
				};

				_context.UsersApps.Add(userApp);

                await _context.SaveChangesAsync();

                result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveAppUserAsync(int userId, string license)
		{
			var result = new RepositoryResponse();

			try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                /* Since licenses are encrypted we have to pull all apps
				 * first and then search by license */
                var apps = await _context
					.Apps
					.ToListAsync();

				var app = apps.FirstOrDefault(a => a.License.ToLower().Equals(license.ToLower()));

				var user = await _context
					.Users
					.Include(u => u.Apps)
					.FirstOrDefaultAsync(
							u => u.Id == userId &&
							u.Apps.Any(ua => ua.AppId == app.Id));

				if (user == null || app == null)
				{
					result.IsSuccess = false;

					return result;
				}

				if (app.OwnerId == user.Id)
				{
					result.IsSuccess = false;

					return result;
				}

				user.Games = [];

				user.Games = await _context
					.Games
					.Include(g => g.SudokuMatrix)
					.ThenInclude(g => g.Difficulty)
					.Include(g => g.SudokuMatrix)
					.ThenInclude(m => m.SudokuCells)
					.Where(g => g.AppId == app.Id)
					.ToListAsync();

				foreach (var game in user.Games)
				{
					if (game.AppId == app.Id)
					{
						_context.Games.Remove(game);
					}
                }

				var userApp = app.UserApps
					.Where(ua => ua.UserId == userId)
					.FirstOrDefault();

				_context.UsersApps.Remove(userApp);

                await _context.SaveChangesAsync();

                result.IsSuccess = true;
				result.Object = user;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> ActivateAsync(int id)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(id, nameof(id));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

				var app = await _context.Apps.FindAsync(id);

				if (app != null)
				{
					if (!app.IsActive)
					{
						app.ActivateApp();

                        await _context.SaveChangesAsync();
                    }

					result.Object = app;
					result.IsSuccess = true;
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> DeactivateAsync(int id)
		{
			var result = new RepositoryResponse();

			try
			{
				ArgumentNullException.ThrowIfNull(id, nameof(id));

				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

				var app = await _context.Apps.FindAsync(id);

				if (app != null)
				{
					if (app.IsActive)
					{
						app.DeactivateApp();

                        await _context.SaveChangesAsync();
                    }

					result.Object = app;
					result.IsSuccess = true;
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> HasEntityAsync(int id) =>
			await _context.Apps.AnyAsync(a => a.Id == id);

		public async Task<bool> IsAppLicenseValidAsync(string license)
		{
			ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

			/* Since licenses are encrypted we have to pull all apps
			 * first and then search by license */
			var apps = await _context
					.Apps
					.ToListAsync();

			return apps.Any(app => app.License.ToLower().Equals(license.ToLower()));
		}

		public async Task<bool> IsUserRegisteredToAppAsync(
			int id,
			string license,
			int userId)
		{
			ArgumentNullException.ThrowIfNull(id, nameof(id));

			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

			ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

            /* Since licenses are encrypted we have to pull all apps
			 * first and then search by license */
            var apps = await _context
				.Apps
				.ToListAsync();

			return apps.Any(
				a => a.UserApps.Any(ua => ua.UserId == userId)
				&& a.Id == id
				&& a.License.ToLower().Equals(license.ToLower()));
		}

		public async Task<bool> IsUserOwnerOThisfAppAsync(
			int id,
			string license,
			int userId)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

            ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

            /* Since licenses are encrypted we have to pull all apps
			 * first and then search by license */
            var apps = await _context
				.Apps
				.ToListAsync();

			return apps.Any(
				a => a.License.ToLower().Equals(license.ToLower())
				&& a.OwnerId == userId
				&& a.Id == id);
		}

		public async Task<string> GetLicenseAsync(int id) => await _context
			.Apps
			.Where(a => a.Id == id)
			.Select(a => a.License)
			.FirstOrDefaultAsync();

		public async Task<IRepositoryResponse> GetGalleryAppsAsync()
		{
			var result = new RepositoryResponse();

			try
			{
				var query = new List<App>();

				query = await _context
					.Apps
					.Where(app => app.IsActive && app.DisplayInGallery && app.Environment == ReleaseEnvironment.PROD)
					.Include(app => app.UserApps)
					.ToListAsync();

				if (query.Count != 0)
				{
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
				return ReposUtilities.ProcessException<AppsRepository<TEntity>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}
		#endregion
	}
}
