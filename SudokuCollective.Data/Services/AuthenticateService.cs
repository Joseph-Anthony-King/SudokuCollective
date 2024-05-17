using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.LoginModels;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using Request = SudokuCollective.Logs.Models.Request;

namespace SudokuCollective.Data.Services
{
    public class AuthenticateService(
        IUsersRepository<User> usersRepository,
        IRolesRepository<Role> rolesRepository,
        IAppsRepository<App> appsRepository,
        IAppAdminsRepository<AppAdmin> appsAdminRepository,
        IUserManagementService userManagementService,
        IRequestService requestService,
        ITokenManagement tokenManagement,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<AuthenticateService> logger) : IAuthenticateService
    {
        private readonly IUsersRepository<User> _usersRepository = usersRepository;
        private readonly IRolesRepository<Role> _rolesRepository = rolesRepository;
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly IAppAdminsRepository<AppAdmin> _appAdminsRepository = appsAdminRepository;
        private readonly IUserManagementService _userManagementService = userManagementService;
        private readonly IRequestService _requestService = requestService;
        private readonly ITokenManagement _tokenManagement = tokenManagement;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<AuthenticateService> _logger = logger;

        public async Task<IResult> AuthenticateAsync(ILoginRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var result = new Result();

                var validateUserTask = await _userManagementService.IsValidUserAsync(request.UserName, request.Password);

                if (!validateUserTask)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }

                var userResponse = await _cacheService.GetByUserNameWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserByUsernameCacheKey, request.UserName, request.License),
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    request.UserName,
                    request.License,
                    result);

                var user = (User)((RepositoryResponse)userResponse.Item1).Object;

                result = (Result)userResponse.Item2;

                var appResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, request.License),
                    _cachingStrategy.Medium,
                    request.License);

                App app;

                if (((RepositoryResponse)appResponse.Item1).IsSuccess)
                {
                    app = (App)((RepositoryResponse)appResponse.Item1).Object;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                if (!app.IsActive)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppDeactivatedMessage;

                    return result;
                }

                if (!app.PermitCollectiveLogins && !app.UserApps.Any(ua => ua.UserId == user.Id))
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.UserIsNotARegisteredUserOfThisAppMessage;

                    return result;
                }

                var appAdmins = (await _appAdminsRepository.GetAllAsync()).Objects.ConvertAll(aa => (AppAdmin)aa);

                if (!user.IsSuperUser)
                {
                    if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                    {
                        if (!appAdmins.Any(aa => aa.AppId == app.Id && aa.UserId == user.Id && aa.IsActive))
                        {
                            var adminRole = user
                                .Roles
                                .FirstOrDefault(ur => ur.Role.RoleLevel == RoleLevel.ADMIN);

                            user.Roles.Remove(adminRole);
                        }
                    }
                }
                else
                {
                    if (!app.PermitSuperUserAccess)
                    {
                        if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.SUPERUSER))
                        {
                            var superUserRole = user
                                .Roles
                                .FirstOrDefault(ur => ur.Role.RoleLevel == RoleLevel.SUPERUSER);

                            user.Roles.Remove(superUserRole);
                        }

                        if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                        {
                            var adminRole = user
                                .Roles
                                .FirstOrDefault(ur => ur.Role.RoleLevel == RoleLevel.ADMIN);

                            user.Roles.Remove(adminRole);
                        }
                    }
                }

                user.Games = user.Games.Where(g => g.Id == app.Id).ToList();

                var userDTO = (UserDTO)user.Cast<UserDTO>();

                var authenticationResult = new AuthenticationResult
                {
                    User = userDTO
                };

                var claim = new List<Claim> {

                    new(ClaimTypes.Name, request.UserName),
                    new(ClaimTypes.Name, user.Id.ToString()),
                    new(ClaimTypes.Name, app.Id.ToString()),
                };

                foreach (var role in user.Roles)
                {
                    var r = (Role)(await _rolesRepository.GetAsync(role.Role.Id)).Object;

                    claim.Add(new Claim(ClaimTypes.Role, r.RoleLevel.ToString()));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));

                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                DateTime expirationDate;

                if (app.TimeFrame == TimeFrame.SECONDS)
                {
                    expirationDate = DateTime.UtcNow.AddSeconds(app.AccessDuration);
                }
                else if (app.TimeFrame == TimeFrame.MINUTES)
                {
                    expirationDate = DateTime.UtcNow.AddMinutes(app.AccessDuration);
                }
                else if (app.TimeFrame == TimeFrame.HOURS)
                {
                    expirationDate = DateTime.UtcNow.AddHours(app.AccessDuration);
                }
                else if (app.TimeFrame == TimeFrame.DAYS)
                {
                    expirationDate = DateTime.UtcNow.AddDays(app.AccessDuration);
                }
                else if (app.TimeFrame == TimeFrame.MONTHS)
                {
                    expirationDate = DateTime.UtcNow.AddMonths(app.AccessDuration);
                }
                else
                {
                    expirationDate = DateTime.UtcNow.AddYears(app.AccessDuration);
                }

                var jwtToken = new JwtSecurityToken(
                        _tokenManagement.Issuer,
                        _tokenManagement.Audience,
                        [.. claim],
                        notBefore: DateTime.UtcNow,
                        expires: expirationDate,
                        signingCredentials: credentials
                    );

                authenticationResult.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                authenticationResult.TokenExpirationDate = expirationDate;

                result.IsSuccess = true;
                result.Message = UsersMessages.UserFoundMessage;
                result.Payload.Add(authenticationResult);

                return result;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<AuthenticateService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    e.Message,
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }
    }
}
