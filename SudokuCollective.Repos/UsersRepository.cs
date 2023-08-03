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
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SudokuCollective.Repos
{
	public class UsersRepository<TEntity> : IUsersRepository<TEntity> where TEntity : User
	{
		#region Fields
		private readonly DatabaseContext _context;
		private readonly IRequestService _requestService;
		private readonly ILogger<UsersRepository<User>> _logger;
		#endregion

		#region Constructors
		public UsersRepository(
				DatabaseContext context,
				IRequestService requestService,
				ILogger<UsersRepository<User>> logger)
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
				_context.Entry(entity).State = EntityState.Added;

				foreach (var userApp in entity.Apps)
				{
					_context.Entry(userApp).State = EntityState.Added;
				}

				foreach (var userRole in entity.Roles)
				{
					_context.Entry(userRole).State = EntityState.Added;
				}


				var role = await _context
						.Roles
						.FirstOrDefaultAsync(r => r.RoleLevel == RoleLevel.USER);

				var userRoles = new List<UserRole>
								{
										new UserRole
										{
												UserId = entity.Id,
												User = entity,
												RoleId = role.Id,
												Role = role
										}
								};

				if (entity.Apps.FirstOrDefault().AppId == 1)
				{
					var adminRole = await _context
							.Roles
							.FirstOrDefaultAsync(r => r.RoleLevel == RoleLevel.ADMIN);

					userRoles.Add(
							new UserRole
							{
								UserId = entity.Id,
								User = entity,
								RoleId = adminRole.Id,
								Role = adminRole
							});
				}

				entity.Roles = userRoles
						.OrderBy(ur => ur.RoleId)
						.ToList();

				_context.AddRange(entity.Roles);

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
						if (userApp.UserId == entity.Id)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else if (dbEntry is UserRole userRole)
					{
						if (userRole.Role == null)
						{
							userRole.Role = await _context
									.Roles
									.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
						}

						if (entity.Roles.Any(ur => ur.Id == userRole.Id))
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
						}
					}
					else if (dbEntry is AppAdmin userAdmin)
					{
						if (userAdmin.AppId == entity.Apps.FirstOrDefault().AppId
								&& userAdmin.UserId == entity.Id)
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
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByUserNameAsync(string username)
		{
			if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

			var result = new RepositoryResponse();

			try
			{
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
						.FirstOrDefaultAsync(
								u => u.UserName.ToLower().Equals(username.ToLower()));

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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetByEmailAsync(string email)
		{
			if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

			var result = new RepositoryResponse();

			try
			{
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

				var query = users
						.FirstOrDefault(
								u => u.Email.ToLower().Equals(email.ToLower()));

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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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
					result.Objects = query
							.ConvertAll(u => (IDomainEntity)u)
							.ToList();
				}

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> GetMyAppsAsync(int id)
		{
			var result = new RepositoryResponse();

			if (id == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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

					if (dbEntry is User)
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
					else if (dbEntry is UserApp)
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
					else if (dbEntry is UserRole)
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
					else if (dbEntry is Role)
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
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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

					if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
					{
						entity.DateUpdated = dateUpdated;

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
				result.Objects = entities.ConvertAll(a => (IDomainEntity)a);

				return result;
			}
			catch (Exception e)
			{
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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

			if (entity.Id == 0 || entity.Id == 1 || entity.IsSuperUser)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
				{
					var games = await _context
							.Games
							.Where(g => g.UserId == entity.Id)
							.ToListAsync();

					var apps = await _context
							.Apps
							.Where(a => a.OwnerId == entity.Id)
							.ToListAsync();

					_context.RemoveRange(games);
					_context.RemoveRange(apps);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is User user)
						{
							if (user.Id == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
						}
						else if (dbEntry is UserApp userApp)
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
						else if (dbEntry is UserRole userRole)
						{
							if (userRole.UserId == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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

					if (await _context.Users.AnyAsync(u => u.Id == entity.Id))
					{
						_context.Remove(entity);

						var games = await _context
								.Games
								.Where(g => g.UserId == entity.Id)
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
						else if (dbEntry is UserRole userRole)
						{
							if (userRole.UserId == entity.Id)
							{
								entry.State = EntityState.Deleted;
							}
							else
							{
								entry.State = EntityState.Modified;
							}
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
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
			if (id == 0)
			{
				return false;
			}

			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

				if (user != null)
				{
					user.ActivateUser();

					_context.Attach(user);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry.Id == 0)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

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

		public async Task<bool> DeactivateAsync(int id)
		{
			if (id == 0)
			{
				return false;
			}

			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

				if (user != null)
				{
					user.DeactiveUser();

					_context.Attach(user);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry.Id == 0)
						{
							entry.State = EntityState.Added;
						}
						else
						{
							entry.State = EntityState.Modified;
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

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

			if (userId == 0 || roleId == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				if (await _context.Users.AnyAsync(u => u.Id == userId) && await _context.Roles.AnyAsync(r => r.Id == roleId) &&
						await _context.Users.AnyAsync(u => u.Id == userId && !u.Roles.Any(ur => ur.RoleId == roleId)))
				{
					var user = await _context
							.Users
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

					_context.Attach(userRole);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is UserRole ur)
						{
							if (ur.Id == userRole.Id)
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
							entry.State = EntityState.Modified;
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> AddRolesAsync(int userId, List<int> roleIds)
		{
			if (roleIds == null) throw new ArgumentNullException(nameof(roleIds));

			var result = new RepositoryResponse();

			if (userId == 0)
			{
				result.IsSuccess = false;
				return result;
			}

			try
			{
				var user = await _context
						.Users
						.FirstOrDefaultAsync(u => u.Id == userId);

				if (user != null)
				{
					var newUserRoleIds = new List<int>();

					foreach (var roleId in roleIds)
					{
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

							_context.Attach(userRole);

							result.Objects.Add(userRole);

							newUserRoleIds.Add((int)userRole.Id);
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

                        if (dbEntry is UserRole ur)
						{
							if (newUserRoleIds.Contains((int)ur.Id))
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
							entry.State = EntityState.Modified;
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveRoleAsync(int userId, int roleId)
		{
			var result = new RepositoryResponse();

			if (userId == 0 || roleId == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				var userRole = await _context
						.UsersRoles
						.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

				if (userRole != null)
                {
                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is UserRole ur)
						{
							if (ur.Id == userRole.Id)
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
							entry.State = EntityState.Modified;
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> RemoveRolesAsync(int userId, List<int> roleIds)
		{
			if (roleIds == null) throw new ArgumentNullException(nameof(roleIds));

			var result = new RepositoryResponse();

			if (userId == 0)
			{
				result.IsSuccess = false;

				return result;
			}

			try
			{
				if (await _context.Users.AnyAsync(u => u.Id == userId))
				{
					foreach (var roleId in roleIds)
					{
						if (await _context
								.UsersRoles
								.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId))
						{
							// USerRole exists so we continue...
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

                        if (dbEntry is UserRole userRole)
						{
							if (userRole.UserId == userId
									&& roleIds.Contains(userRole.RoleId))
							{
								entry.State = EntityState.Deleted;

								result.Objects.Add(userRole);
							}
							else
							{
								entry.State = EntityState.Modified;
							}
						}
						else
						{
							entry.State = EntityState.Modified;
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> PromoteToAdminAsync(int id)
		{
			if (id == 0)
			{
				return false;
			}

			try
			{
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

					_context.Attach(userRole);

                    var trackedEntities = new List<string>();

                    foreach (var entry in _context.ChangeTracker.Entries())
					{
						var dbEntry = (IDomainEntity)entry.Entity;

                        // If the entity is already being tracked for the update... break
                        if (trackedEntities.Contains(dbEntry.ToString()))
                        {
                            break;
                        }

                        if (dbEntry is UserRole ur)
						{
							if (ur.Id == userRole.Id)
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
							entry.State = EntityState.Modified;
                        }

                        // Note that this entry is tracked for the update
                        trackedEntities.Add(dbEntry.ToString());
                    }

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
			if (emailConfirmation == null) throw new ArgumentNullException(nameof(emailConfirmation));

			var result = new RepositoryResponse();

			try
			{
				if (await _context
						.EmailConfirmations
						.AnyAsync(ec => ec.Id == emailConfirmation.Id))
				{
					var user = await _context
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
							.FirstOrDefaultAsync(u => u.Id == emailConfirmation.UserId);

					if (user != null)
					{
						user.DateUpdated = DateTime.UtcNow;
						user.IsEmailConfirmed = true;

						_context.Attach(user);

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

							if (dbEntry is UserRole)
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<IRepositoryResponse> UpdateEmailAsync(IEmailConfirmation emailConfirmation)
		{
			if (emailConfirmation == null) throw new ArgumentNullException(nameof(emailConfirmation));

			var result = new RepositoryResponse();

			try
			{
				if (await _context
						.EmailConfirmations
						.AnyAsync(ec => ec.Id == emailConfirmation.Id))
				{
					var user = await _context
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
							.FirstOrDefaultAsync(u => u.Id == emailConfirmation.UserId);

					if (user != null)
					{
						user.Email = emailConfirmation.NewEmailAddress;
						user.ReceivedRequestToUpdateEmail = false;
						user.DateUpdated = DateTime.UtcNow;

						_context.Attach(user);

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

							if (dbEntry is UserRole)
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
				return ReposUtilities.ProcessException<UsersRepository<User>>(
						_requestService,
						_logger,
						result,
						e);
			}
		}

		public async Task<bool> IsEmailUniqueAsync(string email)
		{
			if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

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
			if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

			if (userId == 0)
			{
				return false;
			}

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
			if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

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
			if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

			if (userId == 0)
			{
				return false;
			}

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
