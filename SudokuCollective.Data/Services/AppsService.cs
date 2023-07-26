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

namespace SudokuCollective.Data.Services
{
    public class AppsService : IAppsService
    {
        #region Fields
        private readonly IAppsRepository<App> _appsRepository;
        private readonly IUsersRepository<User> _usersRepository;
        private readonly IAppAdminsRepository<AppAdmin> _appAdminsRepository;
        private readonly IRolesRepository<Role> _rolesRepository;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly ILogger<AppsService> _logger;
        #endregion

        #region Constructor
        public AppsService(
            IAppsRepository<App> appRepository, 
            IUsersRepository<User> userRepository,
            IAppAdminsRepository<AppAdmin> appAdminsRepository,
            IRolesRepository<Role> rolesRepository,
            IRequestService requestService,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            ICachingStrategy cachingStrategy,
            ILogger<AppsService> logger)
        {
            _appsRepository = appRepository;
            _usersRepository = userRepository;
            _appAdminsRepository = appAdminsRepository;
            _rolesRepository = rolesRepository;
            _requestService = requestService;
            _distributedCache = distributedCache;
            _cacheService = cacheService;
            _cacheKeys = cacheKeys;
            _cachingStrategy = cachingStrategy;
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> CreateAync(IRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            LicensePayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(LicensePayload), out IPayload conversionResult))
            {
                payload = (LicensePayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
            }

            try
            {
                // Ensure the intended owner exists by pull the records from the repository
                var userResponse = await _usersRepository.GetAsync(payload.OwnerId);

                if (userResponse.IsSuccess)
                {
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
                        payload.StagingUrl,
                        payload.ProdUrl,
                        payload.SourceCodeUrl);

                    var addAppResponse = await _cacheService.AddWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        _cacheKeys.GetAppCacheKey,
                        _cachingStrategy.Medium,
                        _cacheKeys,
                        app);

                    if (addAppResponse.IsSuccess)
                    {
                        if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                        {
                            var appAdmin = new AppAdmin(app.Id, user.Id);

                            _ = await _appAdminsRepository.AddAsync(appAdmin);
                        }

                        result.IsSuccess = addAppResponse.IsSuccess;
                        result.Message = AppsMessages.AppCreatedMessage;
                        result.Payload.Add((App)addAppResponse.Object);

                        return result;
                    }
                    else if (!addAppResponse.IsSuccess && addAppResponse.Exception != null)
                    {
                        result.IsSuccess = addAppResponse.IsSuccess;
                        result.Message = addAppResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = AppsMessages.AppNotCreatedMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserDoesNotExistMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetAsync(
            int id,
            int userId)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var appCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, id),
                    _cachingStrategy.Medium,
                    id,
                    result);

                var appResponse = (RepositoryResponse)appCacheServiceResponse.Item1;
                result = (Result)appCacheServiceResponse.Item2;

                if (appResponse.IsSuccess)
                {
                    var app = (App)appResponse.Object;

                    var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                        _cachingStrategy.Medium,
                        userId);
                    
                    var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
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
                    else if (!userResponse.IsSuccess && userResponse.Exception != null)
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = userResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!appResponse.IsSuccess && appResponse.Exception != null)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetByLicenseAsync(
            string license,
            int userId)
        {
            var result = new Result();

            if (string.IsNullOrEmpty(license))
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                    _cachingStrategy.Medium,
                    license,
                    result);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (appResponse.IsSuccess)
                {
                    var app = (IApp)appResponse.Object;

                    var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                        _cachingStrategy.Medium,
                        userId);
                    
                    var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
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
                    else if (!userResponse.IsSuccess && userResponse.Exception != null)
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = userResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!appResponse.IsSuccess && appResponse.Exception != null)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetAppsAsync(IRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            if (request.RequestorId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppsCacheKey),
                    _cachingStrategy.Medium,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    var userCacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, request.RequestorId, request.License),
                        _cachingStrategy.Medium,
                        request.RequestorId);

                    var userResponse = (RepositoryResponse)userCacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
                        var user = (User)userResponse.Object;

                        if (DataUtilities.IsPageValid(request.Paginator, response.Objects))
                        {
                            result = PaginatorUtilities.PaginateApps(
                                request.Paginator,
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

                        return result;
                    }
                    else if (!userResponse.IsSuccess && userResponse.Exception != null)
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = userResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppsNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetMyAppsAsync(int ownerId, IPaginator paginator)
        {
            if (paginator == null) throw new ArgumentNullException(nameof(paginator));

            var result = new Result();

            if (ownerId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetMyAppsWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(string.Format(_cacheKeys.GetMyAppsCacheKey, ownerId)),
                    _cachingStrategy.Medium,
                    ownerId,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    if (DataUtilities.IsPageValid(paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateApps(paginator, response, result);

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

                    return result;
                }
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppsNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetMyRegisteredAppsAsync(int userId, IPaginator paginator)
        {
            if (paginator == null) throw new ArgumentNullException(nameof(paginator));

            var result = new Result();

            if (userId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetMyRegisteredAppsWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(string.Format(_cacheKeys.GetMyRegisteredCacheKey, userId)),
                    _cachingStrategy.Medium,
                    userId,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    if (DataUtilities.IsPageValid(paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateApps(paginator, response, result);

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

                    return result;
                }
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppsNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> UpdateAsync(int id, IRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            AppPayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(AppPayload), out IPayload conversionResult))
            {
                payload = (AppPayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
            }

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var getAppResponse = await _appsRepository.GetAsync(id);

                if (getAppResponse.IsSuccess)
                {
                    if (getAppResponse.IsSuccess)
                    {
                        var app = (App)getAppResponse.Object;

                        app.Name = payload.Name;
                        app.LocalUrl = payload.LocalUrl;
                        app.QaUrl = payload.QaUrl;
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

                        var updateAppResponse = await _cacheService.UpdateWithCacheAsync<App>(
                            _appsRepository,
                            _distributedCache,
                            _cacheKeys,
                            app);

                        if (updateAppResponse.IsSuccess)
                        {
                            result.IsSuccess = true;
                            result.Message = AppsMessages.AppUpdatedMessage;
                            result.Payload.Add(app);

                            return result;
                        }
                        else if (!updateAppResponse.IsSuccess && updateAppResponse.Exception != null)
                        {
                            result.IsSuccess = updateAppResponse.IsSuccess;
                            result.Message = updateAppResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.AppNotUpdatedMessage;

                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = getAppResponse.Exception.Message;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> DeleteOrResetAsync(int id, bool isReset = false)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var getAppResponse = await _appsRepository.GetAsync(id);

                if (getAppResponse.IsSuccess)
                {
                    if (isReset)
                    {
                        if (getAppResponse.IsSuccess)
                        {
                            var resetAppResponse = await _cacheService.ResetWithCacheAsync(
                                _appsRepository,
                                _distributedCache,
                                _cacheKeys,
                                (App)getAppResponse.Object);

                            if (resetAppResponse.IsSuccess)
                            {
                                result.IsSuccess = resetAppResponse.IsSuccess;
                                result.Message = AppsMessages.AppResetMessage;
                                result.Payload.Add(resetAppResponse.Object);

                                return result;
                            }
                            else if (!resetAppResponse.IsSuccess && resetAppResponse.Exception != null)
                            {
                                result.IsSuccess = resetAppResponse.IsSuccess;
                                result.Message = resetAppResponse.Exception.Message;

                                return result;
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = AppsMessages.AppNotFoundMessage;

                                return result;
                            }
                        }
                        else if (!getAppResponse.IsSuccess && getAppResponse.Exception != null)
                        {
                            result.IsSuccess = getAppResponse.IsSuccess;
                            result.Message = getAppResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.AppNotFoundMessage;

                            return result;
                        }
                    }
                    else
                    {
                        if (id == 1)
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.AdminAppCannotBeDeletedMessage;

                            return result;
                        }

                        if (getAppResponse.IsSuccess)
                        {
                            var deleteAppResponse = await _cacheService.DeleteWithCacheAsync(
                                _appsRepository,
                                _distributedCache,
                                _cacheKeys,
                                (App)getAppResponse.Object);

                            if (deleteAppResponse.IsSuccess)
                            {
                                result.IsSuccess = deleteAppResponse.IsSuccess;
                                result.Message = AppsMessages.AppDeletedMessage;

                                return result;
                            }
                            else if (!deleteAppResponse.IsSuccess && deleteAppResponse.Exception != null)
                            {
                                result.IsSuccess = deleteAppResponse.IsSuccess;
                                result.Message = deleteAppResponse.Exception.Message;

                                return result;
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = AppsMessages.AppNotDeletedMessage;

                                return result;
                            }
                        }
                        else if (!getAppResponse.IsSuccess && getAppResponse.Exception != null)
                        {
                            result.IsSuccess = getAppResponse.IsSuccess;
                            result.Message = getAppResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.AppNotFoundMessage;

                            return result;
                        }
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetAppUsersAsync(int id, int requestorId, IPaginator paginator, bool appUsers = true)
        {
            if (paginator == null) throw new ArgumentNullException(nameof(paginator));

            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            if (requestorId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, id),
                    _cachingStrategy.Medium,
                    id);

                var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                if (app != null)
                {
                    RepositoryResponse response;

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

                    response = (RepositoryResponse)cacheServiceResponse.Item1;
                    result = (Result)cacheServiceResponse.Item2;

                    if (response.IsSuccess)
                    {
                        result = PaginatorUtilities.PaginateUsers(paginator, response, result);

                        if (result.Message.Equals(
                            ServicesMesages.SortValueNotImplementedMessage))
                        {
                            return result;
                        }

                        var requestor = (User)(await _usersRepository.GetAsync(requestorId)).Object;

                        var users = new List<TranslatedUser>();

                        if (requestor != null && !requestor.IsSuperUser)
                        {
                            // Filter out user emails from the frontend...
                            foreach (var user in result.Payload)
                            {
                                var emailConfirmed = ((IUser)user).IsEmailConfirmed;
                                ((IUser)user).NullifyEmail();
                                ((IUser)user).IsEmailConfirmed = emailConfirmed;
                                var u = (TranslatedUser)((User)user).Cast<TranslatedUser>();
                                users.Add(u);
                            }
                        }

                        result.Payload = users.ConvertAll(u => (object)u);

                        result.IsSuccess = response.IsSuccess;
                        result.Message = UsersMessages.UsersFoundMessage;

                        return result;
                    }
                    else if (!response.IsSuccess && response.Exception != null)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UsersNotFoundMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> AddAppUserAsync(int appId, int userId)
        {
            var result = new Result();

            if (appId == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            if (userId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (appResponse.IsSuccess)
                {
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

                    if (userResponse.IsSuccess)
                    {
                        var addUserToAppResponse = await _appsRepository.AddAppUserAsync(
                            userId,
                            app.License);

                        if (addUserToAppResponse.IsSuccess)
                        {
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
                        else if (!addUserToAppResponse.IsSuccess && addUserToAppResponse.Exception != null)
                        {
                            result.IsSuccess = addUserToAppResponse.IsSuccess;
                            result.Message = addUserToAppResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.UserNotAddedToAppMessage;

                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> RemoveAppUserAsync(int appId, int userId)
        {
            var result = new Result();

            if (appId == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            if (userId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (appResponse.IsSuccess)
                {
                    if (await _cacheService.HasEntityWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.HasUserCacheKey, userId),
                        _cachingStrategy.Heavy,
                        userId))
                    {
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

                        var removeUserToAppResponse = await _appsRepository.RemoveAppUserAsync(
                            userId,
                            app.License);

                        if (removeUserToAppResponse.IsSuccess)
                        {
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

                            result.IsSuccess = removeUserToAppResponse.IsSuccess;
                            result.Message = AppsMessages.UserRemovedFromAppMessage;
                            result.Payload.Add(removeUserToAppResponse.Object);

                            return result;
                        }
                        else if (!removeUserToAppResponse.IsSuccess && removeUserToAppResponse.Exception != null)
                        {
                            result.IsSuccess = removeUserToAppResponse.IsSuccess;
                            result.Message = removeUserToAppResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.UserNotRemovedFromAppMessage;

                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> ActivateAdminPrivilegesAsync(int appId, int userId)
        {
            var result = new Result();

            if (appId == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            if (userId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (appResponse.IsSuccess)
                {
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

                    if (userReponse.IsSuccess)
                    {
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

                        if (appAdminResult.IsSuccess)
                        {
                            result.IsSuccess = appAdminResult.IsSuccess;
                            result.Message = UsersMessages.UserHasBeenPromotedToAdminMessage;
                            result.Payload.Add(
                                (await _usersRepository.GetAsync(userId))
                                    .Object);

                            return result;
                        }
                        else if (!appAdminResult.IsSuccess && appAdminResult.Exception != null)
                        {
                            result.IsSuccess = appAdminResult.IsSuccess;
                            result.Message = appAdminResult.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.UserHasNotBeenPromotedToAdminMessage;

                            return result;
                        }
                    }
                    else if (!userReponse.IsSuccess && userReponse.Exception != null)
                    {
                        result.IsSuccess = userReponse.IsSuccess;
                        result.Message = userReponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = userReponse.IsSuccess;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!appResponse.IsSuccess && appResponse.Exception != null)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> DeactivateAdminPrivilegesAsync(int appId, int userId)
        {
            var result = new Result();

            if (appId == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            if (userId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (appResponse.IsSuccess)
                {
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

                    if (userResponse.IsSuccess)
                    {

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

                        if (appAdminResult.IsSuccess)
                        {
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
                        else if (!appAdminResult.IsSuccess && appAdminResult.Exception != null)
                        {
                            result.IsSuccess = appAdminResult.IsSuccess;
                            result.Message = appAdminResult.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.DeactivationOfAdminPrivilegesFailedMessage;

                            return result;
                        }
                    }
                    else if (!userResponse.IsSuccess && userResponse.Exception != null)
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = userResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!appResponse.IsSuccess && appResponse.Exception != null)
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = appResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = appResponse.IsSuccess;
                    result.Message = AppsMessages.AppNotFoundMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> ActivateAsync(int id)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var activateAppResponse = await _cacheService.ActivatetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys,
                    id);

                if (activateAppResponse.IsSuccess)
                {
                    result.IsSuccess = activateAppResponse.IsSuccess;
                    result.Message = AppsMessages.AppActivatedMessage;
                    result.Payload.Add(activateAppResponse.Object);

                    return result;
                }
                else if (!activateAppResponse.IsSuccess && activateAppResponse.Exception != null)
                {
                    result.IsSuccess = activateAppResponse.IsSuccess;
                    result.Message = activateAppResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotActivatedMessage;

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

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> DeactivateAsync(int id)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                var deactivateAppResponse = await _cacheService.DeactivatetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    _cacheKeys,
                    id);

                if (deactivateAppResponse.IsSuccess)
                {
                    result.IsSuccess = deactivateAppResponse.IsSuccess;
                    result.Message = AppsMessages.AppDeactivatedMessage;
                    result.Payload.Add(deactivateAppResponse.Object);

                    return result;
                }
                else if (!deactivateAppResponse.IsSuccess && deactivateAppResponse.Exception != null)
                {
                    result.IsSuccess = deactivateAppResponse.IsSuccess;
                    result.Message = deactivateAppResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotDeactivatedMessage;

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

        public async Task<ILicenseResult> GetLicenseAsync(int id, int requestorId)
        {
            var result = new LicenseResult();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = AppsMessages.AppNotFoundMessage;

                return result;
            }

            try
            {
                if (await _cacheService.HasEntityWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.HasAppCacheKey, id),
                    _cachingStrategy.Heavy,
                    id))
                {
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
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }
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
            if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));

            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));
            
            if (string.IsNullOrEmpty(requestorLicense)) throw new ArgumentNullException(nameof(license));

            if (requestorAppId == 0 || userId == 0 || requestorId == 0)
            {
                return false;
            }

            var requestValid = await IsRequestValidOnThisTokenAsync(
                httpContextAccessor, 
                requestorLicense, 
                requestorAppId, 
                userId);

            if (requestValid)
            {
                try
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

                    if (userResponse.IsSuccess && validLicense)
                    {
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
                                (SudokuCollective.Logs.Models.Request)_requestService.Get());

                            return false;
                        }
                    }
                    else
                    {
                        SudokuCollectiveLogger.LogWarning<AppsService>(
                            _logger,
                            LogsUtilities.GetServiceLogEventId(),
                            UsersMessages.UserNotFoundMessage,
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
                        (SudokuCollective.Logs.Models.Request)_requestService.Get());

                    throw;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> IsRequestValidOnThisTokenAsync(
            IHttpContextAccessor httpContextAccessor, 
            string license, 
            int appId, 
            int userId)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));

            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));

            if (appId == 0 || userId == 0)
            {
                return false;
            }

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

            try
            {
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

                if (userResponse.IsSuccess && appResponse.IsSuccess && validLicense)
                {
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
                else
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
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<AppsService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }

        public async Task<Core.Interfaces.Models.DomainObjects.Params.IResult> GetGalleryAppsAsync(IPaginator paginator = null)
        {
            if (paginator == null)
            {
                paginator = new Paginator();
            }

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
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
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
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppsNotFoundMessage;

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
        #endregion
    }
}
