using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Extensions;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Utilities;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using IResult = SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IResult;
using Request = SudokuCollective.Logs.Models.Request;

namespace SudokuCollective.Data.Services
{
    public class AppsService(
        IAppsRepository<App> appRepository,
        IUsersRepository<User> userRepository,
        IAppAdminsRepository<AppAdmin> appAdminsRepository,
        IRolesRepository<Role> rolesRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<AppsService> logger) : IAppsService
    {
        #region Fields
        private readonly IAppsRepository<App> _appsRepository = appRepository;
        private readonly IUsersRepository<User> _usersRepository = userRepository;
        private readonly IAppAdminsRepository<AppAdmin> _appAdminsRepository = appAdminsRepository;
        private readonly IRolesRepository<Role> _rolesRepository = rolesRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<AppsService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> CreateAync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                LicensePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(LicensePayload), out IPayload conversionResult))
                {
                    payload = (LicensePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                // Ensure the intended owner exists by pull the records from the repository
                var userResponse = await _usersRepository.GetAsync(payload.OwnerId);

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserDoesNotExistMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                var generatingGuid = true;
                var license = new Guid();

                /* Ensure the license is unique by pulling all apps from the repository
                    * and checking that the new license is unique */
                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppsCacheKey),
                    _cachingStrategy.Medium);

                var checkAppsResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                foreach (var a in checkAppsResponse.Objects.ConvertAll(a => (App)a))
                {
                    a.License = (await _cacheService.GetLicenseWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppLicenseCacheKey, a.Id),
                        _cachingStrategy.Heavy,
                        _cacheKeys,
                        a.Id)).Item1;
                }

                do
                {
                    license = Guid.NewGuid();

                    if (!checkAppsResponse
                        .Objects
                        .ConvertAll(a => (App)a)
                        .Any(a => a.License.Equals(license.ToString())))
                    {
                        generatingGuid = false;
                    }
                    else
                    {
                        generatingGuid = true;
                    }

                } while (generatingGuid);

                var app = new App(
                    payload.Name,
                    license.ToString(),
                    payload.OwnerId,
                    user.UserName,
                    payload.LocalUrl,
                    payload.TestUrl,
                    payload.StagingUrl,
                    payload.ProdUrl,
                    payload.SourceCodeUrl);

                var appResponse = await _cacheService.AddWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys.GetAppCacheKey,
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    app);

                #region addResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotCreatedMessage;

                    return result;
                }
                #endregion

                if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                {
                    var appAdmin = new AppAdmin(app.Id, user.Id);

                    _ = await _appAdminsRepository.AddAsync(appAdmin);
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppCreatedMessage;
                result.Payload.Add((App)appResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetAsync(
            int id,
            int userId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var appCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, id),
                    _cachingStrategy.Medium,
                    id,
                    result);

                var appResponse = (RepositoryResponse)appCacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message =  appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)appCacheServiceResponse.Item2;

                var app = (App)appResponse.Object;

                var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);
                    
                var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                if (!user.IsSuperUser && app.OwnerId != user.Id)
                {
                    app.NullifyLicense();
                }

                if (app.OwnerId != user.Id)
                {
                    app.NullifySMTPServerSettings();
                }
                else if (app.OwnerId == user.Id && !app.UseCustomSMTPServer)
                {
                    app.NullifySMTPServerSettings();
                }
                else
                {
                    app.SMTPServerSettings.Sanitize();
                }

                if (!user.IsSuperUser)
                {
                    foreach(var u in app.Users)
                    {
                        if (u.Id != user.Id)
                        {
                            u.NullifyEmail();
                        }
                    }
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppFoundMessage;
                result.Payload.Add(app);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetByLicenseAsync(
            string license,
            int userId)
        {
            var result = new Result();

            try
            {
                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                    _cachingStrategy.Medium,
                    license,
                    result);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var app = (IApp)appResponse.Object;

                var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);
                    
                var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message =  userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                if (!user.IsSuperUser && app.OwnerId != user.Id)
                {
                    app.NullifyLicense();
                }

                if (app.OwnerId != user.Id)
                {
                    app.NullifySMTPServerSettings();
                }
                else if (app.OwnerId == user.Id && !app.UseCustomSMTPServer)
                {
                    app.NullifySMTPServerSettings();
                }
                else
                {
                    app.SMTPServerSettings.Sanitize();
                }

                if (!user.IsSuperUser)
                {
                    foreach (var u in app.Users)
                    {
                        if (u.Id != user.Id)
                        {
                            u.NullifyEmail();
                        }
                    }
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppFoundMessage;
                result.Payload.Add(app);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetAppsAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.RequestorId, nameof(request.RequestorId));

                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppsCacheKey),
                    _cachingStrategy.Medium,
                    result);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppsNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, request.RequestorId, request.License),
                    _cachingStrategy.Medium,
                    request.RequestorId);

                var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                if (DataUtilities.IsPageValid(request.Paginator, appResponse.Objects))
                {
                    result = PaginatorUtilities.PaginateApps(
                        request.Paginator,
                        appResponse, 
                        result);

                    if (result.Message.Equals(
                        ServicesMesages.SortValueNotImplementedMessage))
                    {
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;

                    return result;
                }

                foreach (var app in result.Payload.ConvertAll(a => (App)a))
                {
                    if (!user.IsSuperUser && app.OwnerId != user.Id)
                    {
                        app.NullifyLicense();
                    }

                    if (app.OwnerId != user.Id)
                    {
                        app.NullifySMTPServerSettings();
                    }
                    else if (app.OwnerId == user.Id && !app.UseCustomSMTPServer)
                    {
                        app.NullifySMTPServerSettings();
                    }
                    else
                    {
                        app.SMTPServerSettings.Sanitize();
                    }

                    if (!user.IsSuperUser)
                    {
                        foreach (var u in app.Users)
                        {
                            if (u.Id != user.Id)
                            {
                                u.NullifyEmail();
                            }
                        }
                    }
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppsFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetMyAppsAsync(int ownerId, IPaginator paginator)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(ownerId, nameof(ownerId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ownerId, nameof(ownerId));

                ArgumentNullException.ThrowIfNull(paginator, nameof(paginator));

                var cacheServiceResponse = await _cacheService.GetMyAppsWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(string.Format(_cacheKeys.GetMyAppsCacheKey, ownerId)),
                    _cachingStrategy.Medium,
                    ownerId,
                    result);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppsNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                if (DataUtilities.IsPageValid(paginator, appResponse.Objects))
                {
                    result = PaginatorUtilities.PaginateApps(paginator, appResponse, result);

                    if (result.Message.Equals(
                        ServicesMesages.SortValueNotImplementedMessage))
                    {
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;

                    return result;
                }

                foreach (var app in result.Payload.ConvertAll(a => (App)a))
                {
                    if (!app.UseCustomSMTPServer)
                    {
                        app.NullifySMTPServerSettings();
                    }
                    else
                    {
                        app.SMTPServerSettings.Sanitize();
                    }

                    if (ownerId != 1)
                    {
                        foreach (var u in app.Users)
                        {
                            if (u.Id != ownerId)
                            {
                                u.NullifyEmail();
                            }
                        }
                    }
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppsFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetMyRegisteredAppsAsync(int userId, IPaginator paginator)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(paginator, nameof(paginator));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetMyRegisteredAppsWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(string.Format(_cacheKeys.GetMyRegisteredCacheKey, userId)),
                    _cachingStrategy.Medium,
                    userId,
                    result);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppsNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                if (DataUtilities.IsPageValid(paginator, appResponse.Objects))
                {
                    result = PaginatorUtilities.PaginateApps(paginator, appResponse, result);

                    if (result.Message.Equals(
                        ServicesMesages.SortValueNotImplementedMessage))
                    {
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;

                    return result;
                }

                foreach (var app in result.Payload.ConvertAll(a => (App)a))
                {
                    app.NullifyLicense();
                    app.NullifySMTPServerSettings();

                    if (userId != 1)
                    {
                        foreach (var u in app.Users)
                        {
                            if (u.Id != userId)
                            {
                                u.NullifyEmail();
                            }
                        }
                    }
                }

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppsFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> UpdateAsync(int id, IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                AppPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(AppPayload), out IPayload conversionResult))
                {
                    payload = (AppPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var getResponse = await _appsRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)getResponse.Object;

                app.Name = payload.Name;
                app.LocalUrl = payload.LocalUrl;
                app.TestUrl = payload.TestUrl;
                app.StagingUrl = payload.StagingUrl;
                app.ProdUrl = payload.ProdUrl;
                app.SourceCodeUrl = payload.SourceCodeUrl;
                app.IsActive = payload.IsActive;
                app.Environment = payload.Environment;
                app.PermitSuperUserAccess = payload.PermitSuperUserAccess;
                app.PermitCollectiveLogins = payload.PermitCollectiveLogins;
                app.DisableCustomUrls = payload.DisableCustomUrls;
                app.CustomEmailConfirmationAction = payload.CustomEmailConfirmationAction;
                app.CustomPasswordResetAction = payload.CustomPasswordResetAction;
                app.UseCustomSMTPServer = payload.UseCustomSMTPServer;

                if (payload.UseCustomSMTPServer)
                {
                    if (!string.IsNullOrEmpty(payload.SMTPServerSettings.SmtpServer))
                    {
                        app.SMTPServerSettings.SmtpServer = payload.SMTPServerSettings.SmtpServer;
                    }

                    if (payload.SMTPServerSettings.Port != 0)
                    {
                        app.SMTPServerSettings.Port = payload.SMTPServerSettings.Port;
                    }

                    if (!string.IsNullOrEmpty(payload.SMTPServerSettings.UserName))
                    {
                        app.SMTPServerSettings.UserName = payload.SMTPServerSettings.UserName;
                    }

                    if (!string.IsNullOrEmpty(payload.SMTPServerSettings.Password))
                    {
                        app.SMTPServerSettings.Password = payload.SMTPServerSettings.Password;
                    }

                    if (!string.IsNullOrEmpty(payload.SMTPServerSettings.FromEmail))
                    {
                        app.SMTPServerSettings.FromEmail = payload.SMTPServerSettings.FromEmail;
                    }
                }

                app.TimeFrame = payload.TimeFrame;
                app.AccessDuration = payload.AccessDuration;
                app.DisplayInGallery = payload.DisplayInGallery;
                app.DateUpdated = DateTime.UtcNow;

                var updateResponse = await _cacheService.UpdateWithCacheAsync<App>(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys,
                    app);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        AppsMessages.AppNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = true;
                result.Message = AppsMessages.AppUpdatedMessage;
                result.Payload.Add(app);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeleteOrResetAsync(int id, bool isReset = false)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(isReset, nameof(isReset));

                var getResponse = await _appsRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion
                
                if (isReset)
                {
                    var resetResponse = await _cacheService.ResetWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        _cacheKeys,
                        (App)getResponse.Object);

                    #region resetResponse fails
                    if (!resetResponse.IsSuccess)
                    {
                        result.IsSuccess = resetResponse.IsSuccess;
                        result.Message = resetResponse.Exception != null ? 
                            resetResponse.Exception.Message : 
                            AppsMessages.AppNotFoundMessage;

                        return result;
                    }
                    #endregion

                    result.IsSuccess = resetResponse.IsSuccess;
                    result.Message = AppsMessages.AppResetMessage;
                    result.Payload.Add(resetResponse.Object);

                    return result;
                }
                else
                {
                    if (id == 1)
                    {
                        result.IsSuccess = false;
                        result.Message = AppsMessages.AdminAppCannotBeDeletedMessage;

                        return result;
                    }

                    var deleteResponse = await _cacheService.DeleteWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        _cacheKeys,
                        (App)getResponse.Object);

                    #region deleteResponse fails
                    if (!deleteResponse.IsSuccess)
                    {
                        result.IsSuccess = deleteResponse.IsSuccess;
                        result.Message = deleteResponse.Exception != null ? 
                            deleteResponse.Exception.Message : 
                            AppsMessages.AppNotDeletedMessage;

                        return result;
                    }
                    #endregion

                    result.IsSuccess = deleteResponse.IsSuccess;
                    result.Message = AppsMessages.AppDeletedMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetAppUsersAsync(int id, int requestorId, IPaginator paginator, bool appUsers = true)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(paginator, nameof(paginator));

                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(requestorId, nameof(requestorId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestorId, nameof(requestorId));

                ArgumentNullException.ThrowIfNull(appUsers, nameof(appUsers));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, id),
                    _cachingStrategy.Medium,
                    id);

                var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                if (app == null)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                RepositoryResponse appResponse;

                if (appUsers)
                {
                    cacheServiceResponse = await _cacheService.GetAppUsersWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(string.Format(_cacheKeys.GetAppUsersCacheKey, id)),
                        _cachingStrategy.Light,
                        id,
                        result);
                }
                else
                {
                    cacheServiceResponse = await _cacheService.GetNonAppUsersWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(string.Format(_cacheKeys.GetNonAppUsersCacheKey, id)),
                        _cachingStrategy.Light,
                        id,
                        result);
                }

                appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        UsersMessages.UsersNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                result = PaginatorUtilities.PaginateUsers(paginator, appResponse, result);

                if (result.Message.Equals(
                    ServicesMesages.SortValueNotImplementedMessage))
                {
                    return result;
                }

                var requestor = (User)(await _usersRepository.GetAsync(requestorId)).Object;

                var users = new List<UserDTO>();

                if (requestor != null && !requestor.IsSuperUser)
                {
                    // Filter out user emails from the frontend...
                    foreach (var user in appResponse.Objects)
                    {
                        var emailConfirmed = ((IUser)user).IsEmailConfirmed;
                        ((IUser)user).NullifyEmail();
                        ((IUser)user).IsEmailConfirmed = emailConfirmed;
                        var u = (UserDTO)((User)user).Cast<UserDTO>();
                        users.Add(u);
                    }
                } else {

                    foreach (var user in appResponse.Objects)
                    {
                        var u = (UserDTO)((User)user).Cast<UserDTO>();
                        users.Add(u);
                    }
                }
                
                result.IsSuccess = appResponse.IsSuccess;
                result.Message = UsersMessages.UsersFoundMessage;
                result.Payload = users.ConvertAll(u => (object)u);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> AddAppUserAsync(int appId, int userId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)appResponse.Object;

                app.License = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, app.Id),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    app.Id)).Item1;

                cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var addUserToAppResponse = await _appsRepository.AddAppUserAsync(
                    userId,
                    app.License);

                #region addUserToAppResponse
                if (!addUserToAppResponse.IsSuccess)
                {
                    result.IsSuccess = addUserToAppResponse.IsSuccess;
                    result.Message = addUserToAppResponse.Exception != null ? 
                        addUserToAppResponse.Exception.Message : 
                        AppsMessages.UserNotAddedToAppMessage;

                    return result;
                }

                #endregion

                // Remove any cache items which may exist
                var removeKeys = new List<string> {
                    string.Format(_cacheKeys.GetAppCacheKey, app.Id),
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, app.License),
                    string.Format(_cacheKeys.GetAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetNonAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetMyAppsCacheKey, userId),
                    string.Format(_cacheKeys.GetMyRegisteredCacheKey, userId)
                };

                await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                result.IsSuccess = addUserToAppResponse.IsSuccess;
                result.Message = AppsMessages.UserAddedToAppMessage;
                result.Payload.Add((User)userResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> RemoveAppUserAsync(int appId, int userId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var userFound = await _cacheService.HasEntityWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.HasUserCacheKey, userId),
                    _cachingStrategy.Heavy,
                    userId);

                if (!userFound)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }

                var app = (App)appResponse.Object;

                app.License = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, app.Id),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    app.Id)).Item1;

                if (app.OwnerId == userId)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.UserIsTheAppOwnerMessage;

                    return result;
                }

                var removeResponse = await _appsRepository.RemoveAppUserAsync(
                    userId,
                    app.License);

                #region removeResponse fails
                if (!removeResponse.IsSuccess)
                {
                    result.IsSuccess = removeResponse.IsSuccess;
                    result.Message = removeResponse.Exception != null ? 
                        removeResponse.Exception.Message : 
                        AppsMessages.UserNotRemovedFromAppMessage;

                    return result;
                }
                #endregion

                // Remove any app cache items which may exist
                var removeKeys = new List<string> {
                    string.Format(_cacheKeys.GetAppCacheKey, app.Id),
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, app.License),
                    string.Format(_cacheKeys.GetAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetNonAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetMyAppsCacheKey, userId),
                    string.Format(_cacheKeys.GetMyRegisteredCacheKey, userId)
                };

                await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                result.IsSuccess = removeResponse.IsSuccess;
                result.Message = AppsMessages.UserRemovedFromAppMessage;
                result.Payload.Add(removeResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> ActivateAdminPrivilegesAsync(int appId, int userId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)appResponse.Object;

                app.License = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, appId),
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    appId)).Item1;

                cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);

                var userReponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userReponse fails
                if (!userReponse.IsSuccess)
                {
                    result.IsSuccess = userReponse.IsSuccess;
                    result.Message = userReponse.Exception != null ? 
                        userReponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userReponse.Object;

                if (user.IsSuperUser)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.SuperUserCannotBePromotedMessage;

                    return result;
                }

                if (!await _appsRepository.IsUserRegisteredToAppAsync(
                    app.Id, 
                    app.License, 
                    user.Id))
                {
                    _ = await _appsRepository.AddAppUserAsync(user.Id, app.License);
                }
                else
                {
                    if (await _appAdminsRepository.HasAdminRecordAsync(app.Id, user.Id))
                    {
                        var adminRecord = (AppAdmin)(await _appAdminsRepository
                            .GetAdminRecordAsync(app.Id, user.Id)).Object;

                        if (adminRecord.IsActive)
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.UserIsAlreadyAnAdminMessage;

                            return result;
                        }
                        else
                        {
                            adminRecord.IsActive = true;

                            var adminRecordUpdateResult = await _appAdminsRepository
                                .UpdateAsync(adminRecord);

                            var removeKeys = new List<string> {
                                string.Format(_cacheKeys.GetAppCacheKey, app.Id),
                                string.Format(_cacheKeys.GetAppByLicenseCacheKey, app.License),
                                string.Format(_cacheKeys.GetAppUsersCacheKey, app.Id),
                                string.Format(_cacheKeys.GetNonAppUsersCacheKey, app.Id),
                                string.Format(_cacheKeys.GetAppCacheKey, user.Apps.ToList()[0].AppId),
                                _cacheKeys.GetUsersCacheKey
                            };

                            foreach (var key in removeKeys)
                            {
                                if (await _distributedCache.GetAsync(key) != null)
                                {
                                    await _distributedCache.RemoveAsync(string.Format(key));
                                }
                            }

                            result.IsSuccess = adminRecordUpdateResult.IsSuccess;
                            result.Message = UsersMessages.UserHasBeenPromotedToAdminMessage;
                            result.Payload.Add(user);

                            return result;
                        }
                    }
                }

                if (!user.IsAdmin)
                {
                    var adminRole = (await _rolesRepository.GetAllAsync())
                        .Objects
                        .ConvertAll(r => (Role)r)
                        .FirstOrDefault(r => r.RoleLevel == RoleLevel.ADMIN);

                    user.Roles.Add(new UserRole {
                        UserId = user.Id,
                        User = user,
                        RoleId = adminRole.Id,
                        Role = adminRole}) ;

                    user = (User)(await _usersRepository.UpdateAsync(user)).Object;
                }

                var appAdmin = new AppAdmin(app.Id, user.Id);

                var appAdminResult = await _appAdminsRepository.AddAsync(appAdmin);

                #region appAdminResult fails
                if (!appAdminResult.IsSuccess)
                {
                    result.IsSuccess = appAdminResult.IsSuccess;
                    result.Message = appAdminResult.Exception != null ? 
                        appAdminResult.Exception.Message : 
                        UsersMessages.UserHasNotBeenPromotedToAdminMessage;

                    return result;
                }
                #endregion

                // Remove any app cache items which may exist
                var removeKeys2 = new List<string> {
                    string.Format(_cacheKeys.GetAppCacheKey, app.Id),
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, app.License),
                    string.Format(_cacheKeys.GetAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetNonAppUsersCacheKey, app.Id),
                    string.Format(_cacheKeys.GetMyAppsCacheKey, userId),
                    string.Format(_cacheKeys.GetMyRegisteredCacheKey, userId)
                };

                await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys2);

                result.IsSuccess = appAdminResult.IsSuccess;
                result.Message = UsersMessages.UserHasBeenPromotedToAdminMessage;
                result.Payload.Add((await _usersRepository.GetAsync(userId)).Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeactivateAdminPrivilegesAsync(int appId, int userId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)appResponse.Object;

                app.License = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, appId),
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    appId)).Item1;

                cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                if (!user.IsAdmin)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserDoesNotHaveAdminPrivilegesMessage;

                    return result;
                }

                if (!await _appAdminsRepository.HasAdminRecordAsync(app.Id, user.Id))
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.UserIsNotAnAssignedAdminMessage;

                    return result;
                }

                var appAdmin = (AppAdmin)
                    (await _appAdminsRepository.GetAdminRecordAsync(app.Id, user.Id))
                    .Object;

                appAdmin.IsActive = false;

                var appAdminResult = await _appAdminsRepository.UpdateAsync(appAdmin);

                #region appAdminResult fails
                if (!appAdminResult.IsSuccess)
                {
                    result.IsSuccess = appAdminResult.IsSuccess;
                    result.Message = appAdminResult.Exception != null ? 
                        appAdminResult.Exception.Message : 
                        AppsMessages.DeactivationOfAdminPrivilegesFailedMessage;

                    return result;
                }
                #endregion

                var removeKeys = new List<string> {
                        string.Format(_cacheKeys.GetAppCacheKey, app.Id),
                        string.Format(_cacheKeys.GetAppByLicenseCacheKey, app.License),
                        string.Format(_cacheKeys.GetAppUsersCacheKey, app.Id),
                        string.Format(_cacheKeys.GetNonAppUsersCacheKey, app.Id),
                        string.Format(_cacheKeys.GetAppCacheKey, user.Apps.ToList()[0].AppId),
                        _cacheKeys.GetUsersCacheKey
                    };

                foreach (var key in removeKeys)
                {
                    if (await _distributedCache.GetAsync(key) != null)
                    {
                        await _distributedCache.RemoveAsync(string.Format(key));
                    }
                }

                result.IsSuccess = appAdminResult.IsSuccess;
                result.Message = AppsMessages.AdminPrivilegesDeactivatedMessage;
                result.Payload.Add(
                    (await _usersRepository.GetAsync(user.Id))
                    .Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> ActivateAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var appResponse = await _cacheService.ActivatetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys,
                    id);

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotActivatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppActivatedMessage;
                result.Payload.Add(appResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeactivateAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var appResponse = await _cacheService.DeactivatetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys,
                    id);

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        appResponse.Exception.Message : 
                        AppsMessages.AppNotDeactivatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = appResponse.IsSuccess;
                result.Message = AppsMessages.AppDeactivatedMessage;
                result.Payload.Add(appResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<ILicenseResult> GetLicenseAsync(int id, int requestorId)
        {
            var result = new LicenseResult();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(nameof(requestorId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestorId, nameof(requestorId));

                var appExists = await _cacheService.HasEntityWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.HasAppCacheKey, id),
                    _cachingStrategy.Heavy,
                    id);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var response = await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, id),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    id,
                    result);

                var appResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, id),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, response.Item1),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var app = (App)((RepositoryResponse)appResponse.Item1).Object;
                var user = (User)((RepositoryResponse)userResponse.Item1).Object;

                if (!user.IsSuperUser && app.OwnerId != user.Id)
                {
                    result.IsSuccess = false;
                }
                else if (user.IsSuperUser || app.OwnerId == user.Id)
                {
                    result.IsSuccess = true;
                    result.IsFromCache = response.Item2.IsFromCache;
                    result.Message = AppsMessages.AppFoundMessage;
                    result.License = response.Item1;
                }

                return result;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<AppsService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }
        }

        public async Task<bool> IsUserOwnerOThisfAppAsync(
            IHttpContextAccessor httpContextAccessor, 
            string license,
            int userId,
            string requestorLicense,
            int requestorAppId,
            int requestorId)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentException.ThrowIfNullOrEmpty(requestorLicense, nameof(requestorLicense));

                ArgumentNullException.ThrowIfNull(requestorAppId, nameof(requestorAppId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestorAppId, nameof(requestorAppId));

                ArgumentNullException.ThrowIfNull(requestorId, nameof(requestorId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestorId, nameof(requestorId));

                var requestValid = await IsRequestValidOnThisTokenAsync(
                    httpContextAccessor,
                    requestorLicense,
                    requestorAppId,
                    userId);

                if (requestValid)
                {
                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, userId, license),
                        _cachingStrategy.Medium,
                        userId);

                    var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    var validLicense = await _cacheService.IsAppLicenseValidWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.IsAppLicenseValidCacheKey, license),
                        _cachingStrategy.Heavy,
                        license);

                    if (!userResponse.IsSuccess && !validLicense)
                    {
                        SudokuCollectiveLogger.LogWarning<AppsService>(
                            _logger,
                            LogsUtilities.GetServiceLogEventId(),
                            UsersMessages.UserNotFoundMessage,
                            (Request)_requestService.Get());

                        return false;
                    }

                    var requestorOwnerOfThisApp = await _appsRepository.IsUserOwnerOThisfAppAsync(
                        requestorId,
                        license,
                        userId);

                    if (requestorOwnerOfThisApp && validLicense)
                    {
                        return true;
                    }
                    else if (((User)userResponse.Object).IsSuperUser && validLicense)
                    {
                        return true;
                    }
                    else
                    {
                        SudokuCollectiveLogger.LogWarning<AppsService>(
                            _logger,
                            LogsUtilities.GetServiceLogEventId(),
                            AppsMessages.UserIsNotTheAppOwnerMessage,
                            (Request)_requestService.Get());

                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<AppsService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(),
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }

        public async Task<bool> IsRequestValidOnThisTokenAsync(
            IHttpContextAccessor httpContextAccessor, 
            string license, 
            int appId, 
            int userId)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                #region Obtain user id and app id from the JWT Token
                int tokenUserId, tokenAppId;

                if (httpContextAccessor != null)
                {
                    var jwtToken = (httpContextAccessor.HttpContext.Request.Headers["Authorization"]).ToString().Substring(7);

                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jwtToken);
                    var tokenS = jsonToken as JwtSecurityToken;

                    tokenUserId = Convert.ToInt32(tokenS.Claims.Skip(1).First(claim => claim.Type == ClaimTypes.Name).Value);
                    tokenAppId = Convert.ToInt32(tokenS.Claims.Skip(2).First(claim => claim.Type == ClaimTypes.Name).Value);

                    if (userId != tokenUserId || appId != tokenAppId)
                    {
                        SudokuCollectiveLogger.LogWarning<AppsService>(
                            _logger,
                            LogsUtilities.GetServiceLogEventId(),
                            LoggerMessages.TheUserOrAppIsNotValidForThisJWTToken,
                            (SudokuCollective.Logs.Models.Request)_requestService.Get());

                        return false;
                    }
                }
                else
                {
                    SudokuCollectiveLogger.LogWarning<AppsService>(
                        _logger,
                        LogsUtilities.GetServiceLogEventId(),
                        LoggerMessages.HttpContextAccessorIsNull,
                        (SudokuCollective.Logs.Models.Request)_requestService.Get());

                    return false;
                }
                #endregion

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, license),
                    _cachingStrategy.Medium,
                    userId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!appResponse.IsSuccess || !license.Equals(((App)appResponse.Object).License))
                {
                    SudokuCollectiveLogger.LogWarning<AppsService>(
                        _logger,
                        LogsUtilities.GetServiceLogEventId(),
                        LoggerMessages.TheLicenseIsNotValidOnThisRequest,
                        (SudokuCollective.Logs.Models.Request)_requestService.Get());

                    return false;
                }

                var validLicense = await _cacheService.IsAppLicenseValidWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.IsAppLicenseValidCacheKey, license),
                    _cachingStrategy.Heavy,
                    license);

                if (!userResponse.IsSuccess && !appResponse.IsSuccess && !validLicense)
                {
                    SudokuCollectiveLogger.LogWarning<AppsService>(
                        _logger,
                        LogsUtilities.GetServiceLogEventId(),
                        string.Format("{0} or {1}",
                            UsersMessages.UserNotFoundMessage,
                            AppsMessages.AppsNotFoundMessage),
                        (SudokuCollective.Logs.Models.Request)_requestService.Get());

                    return false;
                }

                bool userPermittedAccess;

                if (!((App)appResponse.Object).PermitCollectiveLogins)
                {
                    userPermittedAccess = await _appsRepository
                        .IsUserRegisteredToAppAsync(appId, license, userId);
                }
                else
                {
                    userPermittedAccess = true;
                }

                if (userPermittedAccess && validLicense)
                {
                    if (((App)appResponse.Object).IsActive)
                    {
                        return true;
                    }
                    else
                    {
                        SudokuCollectiveLogger.LogWarning<AppsService>(
                            _logger,
                            LogsUtilities.GetServiceLogEventId(),
                            string.Format(LoggerMessages.AppIsNotActive, ((App)appResponse.Object).Name),
                            (SudokuCollective.Logs.Models.Request)_requestService.Get());

                        return false;
                    }
                }
                else if (((User)userResponse.Object).IsSuperUser && validLicense)
                {
                    return true;
                }
                else
                {
                    SudokuCollectiveLogger.LogWarning<AppsService>(
                        _logger,
                        LogsUtilities.GetServiceLogEventId(),
                        "The requestor is not a super user or license is invalid",
                        (SudokuCollective.Logs.Models.Request)_requestService.Get());

                    return false;
                }
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<AppsService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(),
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }

        public async Task<IResult> GetGalleryAppsAsync(IPaginator paginator = null)
        {
            paginator ??= new Paginator();

            var result = new Result();

            try
            {
                var cacheServiceResponse = await _cacheService.GetGalleryAppsWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetGalleryAppsKey),
                    _cachingStrategy.Medium,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        AppsMessages.AppsNotFoundMessage;

                    return result;
                }

                result = (Result)cacheServiceResponse.Item2;

                if (DataUtilities.IsPageValid(paginator, response.Objects))
                {
                    result = PaginatorUtilities.PaginateGallery(
                        paginator,
                        response, 
                        result);

                    if (result.Message.Equals(
                        ServicesMesages.SortValueNotImplementedMessage))
                    {
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;

                    return result;
                }

                result.IsSuccess = response.IsSuccess;
                result.Message = AppsMessages.AppsFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AppsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }
        #endregion
    }
}
