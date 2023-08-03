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
	public class AppsRepository<TEntity> : IAppsRepository<TEntity> where TEntity : App
	{
		#region Fields
		private readonly DatabaseContext _context;
		private readonly IRequestService _requestService;
		private readonly ILogger<AppsRepository<App>> _logger;
		#endregion

		#region Constructor
		public AppsRepository(
				DatabaseContext context,
				IRequestService requestService,
				ILogger<AppsRepository<App>> logger)
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

				var user = await _context
						.Users
						.FirstOrDefaultAsync(u => u.Id == entity.OwnerId);

				// Add connection between the app and the user
				var userApp = new UserApp
				{
					User = user,
					UserId = user.Id,
					App = entity,
					AppId = entity.Id
				};

				entity.UserApps.Add(userApp);

				_context.Attach(userApp);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

                    if (dbEntry is App app)
					{
						if (app.Id == entity.Id)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else if (dbEntry is UserApp ua)
					{
						if (ua.Id == userApp.Id)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else if (dbEntry is SMTPServerSettings s)
					{
						if (s.AppId == entity.Id)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else
					{
						if (dbEntry.Id == 0)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
                    }

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

				await _context.SaveChangesAsync();

				// Ensure that the owner has admin priviledges, if not they will be promoted
				var addAdminRole = true;
				var newUserAdminRole = new UserRole();

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

					newUserAdminRole = new UserRole
					{
						User = user,
						UserId = user.Id,
						Role = adminRole,
						RoleId = adminRole.Id
					};

					var appAdmin = new AppAdmin
					{
						AppId = entity.Id,
						UserId = user.Id
					};

					_context.Attach(newUserAdminRole);

					_context.Attach(appAdmin);

					foreach (var entry in _context.ChangeTracker.Entries())
					{
						if (entry.Entity is UserApp ua)
						{
							if (ua.Id == newUserAdminRole.Id)
							{
								entry.State = EntityState.Added;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
						}
						else
						{
							var dbEntry = (IDomainEntity)entry.Entity;

							if (dbEntry.Id == 0)
							{
								entry.State = EntityState.Added;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
						}
					}

					await _context.SaveChangesAsync();
				}

				result.Object = entity;
				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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
				var query = new App();

				query = await _context
						.Apps
						.FirstOrDefaultAsync(a => a.Id == id);

				query.SMTPServerSettings = await _context
						.SMTPServerSettings
						.FirstOrDefaultAsync(s => s.AppId == query.Id);

				if (query != null)
				{
					query.UserApps = await _context.UsersApps
						.Where(ua => ua.AppId == query.Id)
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

						userApp.User.Games = new List<Game>();

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByLicenseAsync(string license)
		{
			var result = new RepositoryResponse();

			if (string.IsNullOrEmpty(license))
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var query = new App();

				/* Since licenses are encrypted we have to pull all apps
				 * first and then search by license */
				var apps = await _context
						.Apps
						.ToListAsync();

				query = apps.FirstOrDefault(
								a => a.License.ToLower().Equals(license.ToLower()));

				query.SMTPServerSettings = await _context
						.SMTPServerSettings
						.FirstOrDefaultAsync(s => s.AppId == query.Id);

				if (query != null)
				{
					query.UserApps = await _context.UsersApps
							.Include(ua => ua.User)
									.ThenInclude(u => u.Roles)
							.Where(ua => ua.AppId == query.Id)
							.ToListAsync();

					foreach (var userApp in query.UserApps)
					{
						foreach (var userRole in userApp.User.Roles)
						{
							userRole.Role = await _context
									.Roles
									.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
						}

						userApp.User.Games = new List<Game>();

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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
				var query = new List<App>();

				query = await _context
						.Apps
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
						app.SMTPServerSettings = await _context
								.SMTPServerSettings
								.FirstOrDefaultAsync(s => s.AppId == app.Id);

						foreach (var userApp in app.UserApps)
						{
							userApp.User.Games = new List<Game>();

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
					result.Objects = query
							.ConvertAll(a => (IDomainEntity)a)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyAppsAsync(int ownerId)
		{
			var result = new RepositoryResponse();

			if (ownerId == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var query = new List<App>();

				query = await _context
						.Apps
						.Where(a => a.OwnerId == ownerId)
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
						app.SMTPServerSettings = await _context
								.SMTPServerSettings
								.FirstOrDefaultAsync(s => s.AppId == app.Id);

						foreach (var userApp in app.UserApps)
						{
							userApp.User.Games = new List<Game>();

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
					result.Objects = query
							.ConvertAll(a => (IDomainEntity)a)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyRegisteredAppsAsync(int userId)
		{
			var result = new RepositoryResponse();

			if (userId == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var query = new List<App>();

				query = await _context.Users
						.Where(u => u.Id == userId)
						.SelectMany(u => u.Apps.Where(ua => ua.App.OwnerId != userId))
						.Select(ua => ua.App)
						.ToListAsync();

				if (query.Count != 0)
				{
					// Filter games by app
					foreach (var app in query)
					{
						app.SMTPServerSettings = await _context
								.SMTPServerSettings
								.FirstOrDefaultAsync(s => s.AppId == app.Id);

						foreach (var userApp in app.UserApps)
						{
							userApp.User.Games = new List<Game>();

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
					result.Objects = query
							.ConvertAll(a => (IDomainEntity)a)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetAppUsersAsync(int id)
		{
			var result = new RepositoryResponse();

			if (id == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
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
						user.Games = new List<Game>();

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
					result.Objects = query
							.ConvertAll(u => (IDomainEntity)u)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetNonAppUsersAsync(int id)
		{
			var result = new RepositoryResponse();

			if (id == 0 || !await HasEntityAsync(id))
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
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
						user.Games = new List<Game>();

						user.Games = await _context
								.Games
								.Where(g => g.AppId == id && g.UserId != user.Id)
								.ToListAsync();
					}

					result.IsSuccess = true;
					result.Objects = query
							.ConvertAll(u => (IDomainEntity)u)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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
				if (entity.Id == 0)
				{
					result.IsSuccess = false;

					return result;
				}

				if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
				{
					entity.DateUpdated = DateTime.UtcNow;

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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

				var dateUpdated = DateTime.UtcNow;

				foreach (var entity in entities)
				{
					if (entity.Id == 0)
					{
						result.IsSuccess = false;

						return result;
					}

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

				_context.Apps.UpdateRange(entities);

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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
				if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
				{
					var games = await _context
							.Games
							.Include(g => g.SudokuMatrix)
									.ThenInclude(g => g.Difficulty)
							.Include(g => g.SudokuMatrix)
									.ThenInclude(m => m.SudokuCells)
							.Where(g => g.AppId == entity.Id)
							.ToListAsync();

					_context.RemoveRange(games);

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

                        if (dbEntry is UserApp userApp)
						{
							if (userApp.AppId == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
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

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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

					if (await _context.Apps.AnyAsync(a => a.Id == entity.Id))
					{
						_context.Remove(entity);

						var games = await _context
								.Games
								.Include(g => g.SudokuMatrix)
										.ThenInclude(g => g.Difficulty)
								.Include(g => g.SudokuMatrix)
										.ThenInclude(m => m.SudokuCells)
								.Where(g => g.AppId == entity.Id)
								.ToListAsync();

						_context.RemoveRange(games);
					}
					else
					{
						result.IsSuccess = false;

						return result;
					}
                }

                var trackedEntities = new List<string>();

                foreach (var entity in entities)
				{
					foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is UserApp userApp)
						{
							if (userApp.AppId == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
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

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

					await _context.SaveChangesAsync();
				}

				result.IsSuccess = true;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> ResetAsync(TEntity entity)
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
				List<Game> games = await _context
						.Games
						.Include(g => g.SudokuMatrix)
								.ThenInclude(g => g.Difficulty)
						.Include(g => g.SudokuMatrix)
								.ThenInclude(m => m.SudokuCells)
						.Where(g => g.AppId == entity.Id)
						.ToListAsync();

				if (games.Count > 0)
				{
					_context.RemoveRange(games);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is UserApp userApp)
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

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

					await _context.SaveChangesAsync();
				}

				result.IsSuccess = true;
				result.Object = await _context
						.Apps
						.FirstOrDefaultAsync(a => a.Id == entity.Id);

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> AddAppUserAsync(int userId, string license)
		{
			var result = new RepositoryResponse();

			if (userId == 0 || string.IsNullOrEmpty(license))
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var user = await _context
						.Users
						.FirstOrDefaultAsync(u => u.Id == userId);

				/* Since licenses are encrypted we have to pull all apps
				 * first and then search by license */
				var apps = await _context
						.Apps
						.ToListAsync();

				var app = apps
						.FirstOrDefault(
								a => a.License.ToLower().Equals(license.ToLower()));

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

				_context.Attach(userApp);

                var trackedEntities = new List<string>();

                foreach (var entry in _context.ChangeTracker.Entries())
				{
					var dbEntry = (IDomainEntity)entry.Entity;

                    // If the entity is already being tracked for the update... break
                    if (trackedEntities.Contains(dbEntry.ToString()))
                    {
                        break;
                    }

                    if (dbEntry is UserApp ua)
					{
						if (ua.Id == userApp.Id)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveAppUserAsync(int userId, string license)
		{
			var result = new RepositoryResponse();

			if (userId == 0 || string.IsNullOrEmpty(license))
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				/* Since licenses are encrypted we have to pull all apps
				 * first and then search by license */
				var apps = await _context
						.Apps
						.ToListAsync();

				var app = apps
						.FirstOrDefault(
								a => a.License.ToLower().Equals(license.ToLower()));

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

				user.Games = new List<Game>();

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
						_context.Remove(game);
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

                    if (dbEntry is UserApp userApp)
					{
						if (userApp.UserId == user.Id && userApp.AppId == app.Id)
						{
							entry.State = EntityState.Deleted;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else if (dbEntry is UserRole)
					{
						entry.State = EntityState.Modified;
					}
					else
					{

                    }

                    // Note that this entry is tracked for the update
                    trackedEntities.Add(dbEntry.ToString());
                }

				await _context.SaveChangesAsync();

				result.IsSuccess = true;
				result.Object = user;

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> ActivateAsync(int id)
		{
			var result = new RepositoryResponse();

			if (id == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var app = await _context.Apps.FindAsync(id);

				if (app != null)
				{
					if (!app.IsActive)
					{
						app.ActivateApp();

						_context.Attach(app);

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> DeactivateAsync(int id)
		{
			var result = new RepositoryResponse();

			if (id == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var app = await _context.Apps.FindAsync(id);

				if (app != null)
				{
					if (app.IsActive)
					{
						app.DeactivateApp();

						_context.Attach(app);

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
				return ReposUtilities.ProcessException<AppsRepository<App>>(
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
			/* Since licenses are encrypted we have to pull all apps
			 * first and then search by license */
			var apps = await _context
					.Apps
					.ToListAsync();

			return apps
					.Any(
							a => a.UserApps.Any(ua => ua.UserId == userId)
							&& a.Id == id
							&& a.License.ToLower().Equals(license.ToLower()));
		}

		public async Task<bool> IsUserOwnerOThisfAppAsync(
				int id,
				string license,
				int userId)
		{
			/* Since licenses are encrypted we have to pull all apps
			 * first and then search by license */
			var apps = await _context
					.Apps
					.ToListAsync();

			return apps
					.Any(
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
					result.Objects = query
							.ConvertAll(a => (IDomainEntity)a)
							.ToList();
				}
				else
				{
					result.IsSuccess = false;
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<AppsRepository<App>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}
		#endregion
	}
}
