using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
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
using Request = SudokuCollective.Logs.Models.Request;

namespace SudokuCollective.Data.Services
{
    public class UsersService(
        IUsersRepository<User> usersRepository,
        IAppsRepository<App> appsRepository,
        IRolesRepository<Role> rolesRepository,
        IAppAdminsRepository<AppAdmin> appAdminsRepository,
        IEmailConfirmationsRepository<EmailConfirmation> emailConfirmationsRepository,
        IPasswordResetsRepository<PasswordReset> passwordResetsRepository,
        IEmailService emailService,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<UsersService> logger) : IUsersService
    {
        #region Fields
        private readonly IUsersRepository<User> _usersRepository = usersRepository;
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly IRolesRepository<Role> _rolesRepository = rolesRepository;
        private readonly IAppAdminsRepository<AppAdmin> _appAdminsRepository = appAdminsRepository;
        private readonly IEmailConfirmationsRepository<EmailConfirmation> _emailConfirmationsRepository = emailConfirmationsRepository;
        private readonly IPasswordResetsRepository<PasswordReset> _passwordResetsRepository = passwordResetsRepository;
        private readonly IEmailService _emailService = emailService;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<UsersService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> CreateAsync(
            ISignupRequest request, 
            string baseUrl, 
            string emailTemplatePath)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

            ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

            var result = new Result();

            var isUserNameUnique = false;

            var isEmailUnique = false;

            // User name accepsts alphanumeric and special characters except double and single quotes
            var regex = new Regex("^[^-]{1}?[^\"\']*$");

            if (!string.IsNullOrEmpty(request.UserName))
            {
                isUserNameUnique = await _usersRepository.IsUserNameUniqueAsync(request.UserName);
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                isEmailUnique = await _usersRepository.IsEmailUniqueAsync(request.Email);
            }

            if (string.IsNullOrEmpty(request.UserName)
                || string.IsNullOrEmpty(request.Email)
                || !isUserNameUnique
                || !isEmailUnique
                || !regex.IsMatch(request.UserName))
            {
                if (string.IsNullOrEmpty(request.UserName))
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNameRequiredMessage;

                    return result;
                }
                else if (string.IsNullOrEmpty(request.Email))
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailRequiredMessage;

                    return result;
                }
                else if (!regex.IsMatch(request.UserName))
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNameInvalidMessage;

                    return result;
                }
                else if (!isUserNameUnique)
                {
                    result.IsSuccess = isUserNameUnique;
                    result.Message = UsersMessages.UserNameUniqueMessage;

                    return result;
                }
                else
                {
                    result.IsSuccess = isEmailUnique;
                    result.Message = UsersMessages.EmailUniqueMessage;

                    return result;
                }
            }
            else
            {
                try
                {
                    var cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppByLicenseCacheKey, request.License),
                        _cachingStrategy.Medium,
                        request.License,
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

                    var app = (App)appResponse.Object;

                    if (!app.IsActive)
                    {
                        result.IsSuccess = false;
                        result.Message = AppsMessages.AppDeactivatedMessage;

                        return result;
                    }

                    var salt = BCrypt.Net.BCrypt.GenerateSalt();

                    var user = new User(
                        0,
                        request.UserName,
                        request.FirstName,
                        request.LastName,
                        request.NickName,
                        request.Email,
                        false,
                        true,
                        BCrypt.Net.BCrypt.HashPassword(request.Password, salt),
                        false,
                        true,
                        DateTime.UtcNow,
                        DateTime.MinValue);

                    user.Apps.Add(
                        new UserApp()
                        {
                            User = user,
                            UserId = user.Id,
                            App = app,
                            AppId = app.Id
                        });

                    var rolesResponse = await _rolesRepository.GetAllAsync();

                    user.Roles.Add(
                        new UserRole()
                        {
                            User = user,
                            UserId = user.Id,
                            Role = rolesResponse
                                .Objects
                                .ConvertAll(o => (Role)o)
                                .Where(r => r.RoleLevel == RoleLevel.USER)
                                .FirstOrDefault(),
                            RoleId = rolesResponse
                                .Objects
                                .ConvertAll(o => (Role)o)
                                .Where(r => r.RoleLevel == RoleLevel.USER)
                                .FirstOrDefault().Id,
                        });

                    if (app.Id == 1)
                    {
                        user.Roles.Add(
                            new UserRole()
                            {
                                User = user,
                                UserId = user.Id,
                                Role = rolesResponse
                                    .Objects
                                    .ConvertAll(o => (Role)o)
                                    .Where(r => r.RoleLevel == RoleLevel.ADMIN)
                                    .FirstOrDefault(),
                                RoleId = rolesResponse
                                    .Objects
                                    .ConvertAll(o => (Role)o)
                                    .Where(r => r.RoleLevel == RoleLevel.ADMIN)
                                    .FirstOrDefault().Id,
                            });
                    }

                    var userResponse = await _cacheService.AddWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys.GetUserCacheKey,
                        _cachingStrategy.Medium,
                        _cacheKeys,
                        user);

                    #region userResponse fails
                    if (!userResponse.IsSuccess)
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = userResponse.Exception != null ? 
                            userResponse.Exception.Message : 
                            UsersMessages.UserNotCreatedMessage;

                        return result;
                    }
                    #endregion

                    user = (User)userResponse.Object;

                    if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                    {
                        var appAdmin = new AppAdmin(app.Id, user.Id);

                        _ = await _appAdminsRepository.AddAsync(appAdmin);
                    }

                    var emailConfirmation = new EmailConfirmation(
                        EmailConfirmationType.NEWPROFILECONFIRMED,
                        user.Id,
                        app.Id,
                        null,
                        user.Email);

                    emailConfirmation = await EnsureEmailConfirmationTokenIsUnique(emailConfirmation);

                    emailConfirmation = (EmailConfirmation)(await _emailConfirmationsRepository.CreateAsync(emailConfirmation))
                        .Object;

                    string EmailConfirmationAction;

                    if (app.UseCustomEmailConfirmationAction)
                    {
                        if (app.Environment == ReleaseEnvironment.LOCAL)
                        {
                            EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                app.LocalUrl,
                                app.CustomEmailConfirmationAction,
                                emailConfirmation.Token);
                        }
                        else if (app.Environment == ReleaseEnvironment.STAGING)
                        {
                            EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                app.StagingUrl,
                                app.CustomEmailConfirmationAction,
                                emailConfirmation.Token);
                        }
                        else if (app.Environment == ReleaseEnvironment.TEST)
                        {
                            EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                app.TestUrl,
                                app.CustomEmailConfirmationAction,
                                emailConfirmation.Token);
                        }
                        else
                        {
                            EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                app.ProdUrl,
                                app.CustomEmailConfirmationAction,
                                emailConfirmation.Token);
                        }
                    }
                    else
                    {
                        EmailConfirmationAction = string.Format("https://{0}/confirmEmail/{1}",
                            baseUrl,
                            emailConfirmation.Token);
                    }

                    var html = File.ReadAllText(emailTemplatePath);
                    var appTitle = app.Name;
                    var url = string.Empty;

                    if (app.Environment == ReleaseEnvironment.LOCAL)
                    {
                        url = app.LocalUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.STAGING)
                    {
                        url = app.StagingUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.TEST)
                    {
                        url = app.TestUrl;
                    }
                    else
                    {
                        url = app.ProdUrl;
                    }

                    html = html.Replace("{{USER_NAME}}", user.UserName);
                    html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                    html = html.Replace("{{APP_TITLE}}", appTitle);
                    html = html.Replace("{{URL}}", url);

                    var emailSubject = string.Format("Greetings from {0}: Please Confirm Email", appTitle);

                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = UsersMessages.UserCreatedMessage;

                    result.Payload.Add(
                        new EmailConfirmationSentResult() 
                        {
                            EmailConfirmationSent = await _emailService
                                .SendAsync(user.Email, emailSubject, html, app.Id)
                        });

                    return result;
                }
                catch (Exception e)
                {
                    return DataUtilities.ProcessException<UsersService>(
                        _requestService,
                        _logger,
                        result,
                        e);
                }
            }
        }

        public async Task<IResult> GetAsync(
            int id,
            string license,
            IRequest request = null)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var user = (User)response.Object;

                user.NullifyPassword();

                result.IsSuccess = response.IsSuccess;
                result.Message = UsersMessages.UserFoundMessage;

                cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                    _cachingStrategy.Medium,
                    license);

                var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                var appAdmins = (await _appAdminsRepository.GetAllAsync())
                    .Objects
                    .ConvertAll(aa => (AppAdmin)aa)
                    .ToList();

                if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                {
                    if (!user.IsSuperUser)
                    {
                        if (!appAdmins.Any(aa =>
                            aa.AppId == app.Id &&
                            aa.UserId == user.Id && aa.IsActive))
                        {
                            var adminRole = user
                                .Roles
                                .FirstOrDefault(ur =>
                                    ur.Role.RoleLevel == RoleLevel.ADMIN);

                            user.Roles.Remove(adminRole);
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
                }

                if (request != null)
                {
                    var getRequestorResponse = await _cacheService.GetWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, request.RequestorId, license),
                        _cachingStrategy.Medium,
                        request.RequestorId);

                    var requestorResponse = (RepositoryResponse)getRequestorResponse.Item1;

                    if (!user.IsSuperUser && request.RequestorId != id)
                    {
                        user.NullifyEmail();
                    }
                }
                else
                {
                    user.NullifyEmail();
                }

                result.Payload.Add(user.Cast<UserDTO>());

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> UpdateAsync(
            int id, 
            IRequest request, 
            string baseUrl, 
            string emailTemplatePath)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                var userResult = new UserResult();

                UpdateUserPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserPayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                // User name accepsts alphanumeric and special characters except double and single quotes
                var regex = new Regex("^[^-]{1}?[^\"\']*$");

                var isUserNameUnique = await _usersRepository.IsUpdatedUserNameUniqueAsync(id, payload.UserName);
                var isEmailUnique = await _usersRepository.IsUpdatedEmailUniqueAsync(id, payload.Email);

                if (string.IsNullOrEmpty(payload.UserName)
                    || string.IsNullOrEmpty(payload.Email)
                    || !isUserNameUnique
                    || !isEmailUnique
                    || !regex.IsMatch(payload.UserName))
                {
                    if (string.IsNullOrEmpty(payload.UserName))
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNameRequiredMessage;

                        return result;
                    }
                    else if (string.IsNullOrEmpty(payload.Email))
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailRequiredMessage;

                        return result;
                    }
                    else if (!regex.IsMatch(payload.UserName))
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNameInvalidMessage;

                        return result;
                    }
                    else if (!isUserNameUnique)
                    {
                        result.IsSuccess = isUserNameUnique;
                        result.Message = UsersMessages.UserNameUniqueMessage;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = isEmailUnique;
                        result.Message = UsersMessages.EmailUniqueMessage;

                        return result;
                    }
                }
                else
                {
                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, id, request.License),
                        _cachingStrategy.Medium,
                        id);

                    var getResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    #region getResponse fails
                    if (!getResponse.IsSuccess)
                    {
                        result.IsSuccess = getResponse.IsSuccess;
                        result.Message = getResponse.Exception != null ? 
                            getResponse.Exception.Message : 
                            UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                    #endregion

                    var user = (User)getResponse.Object;

                    cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppCacheKey, request.AppId),
                        _cachingStrategy.Medium,
                        request.AppId);

                    var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                    user.UserName = payload.UserName;
                    user.FirstName = payload.FirstName;
                    user.LastName = payload.LastName;
                    user.NickName = payload.NickName;
                    user.DateUpdated = DateTime.UtcNow;

                    if (!user.Email.ToLower().Equals(payload.Email.ToLower()))
                    {
                        if (!user.ReceivedRequestToUpdateEmail)
                        {
                            user.ReceivedRequestToUpdateEmail = true;
                        }

                        EmailConfirmation emailConfirmation;

                        if (await _emailConfirmationsRepository.HasOutstandingEmailConfirmationAsync(user.Id, app.Id))
                        {
                            emailConfirmation = (EmailConfirmation)(await _emailConfirmationsRepository
                                .RetrieveEmailConfirmationAsync(user.Id, app.Id)).Object;

                            if (!user.IsEmailConfirmed)
                            {
                                user.Email = emailConfirmation.OldEmailAddress;
                            }

                            emailConfirmation.OldEmailAddress = user.Email;
                            emailConfirmation.NewEmailAddress = payload.Email;
                        }
                        else
                        {
                            emailConfirmation = new EmailConfirmation(
                                EmailConfirmationType.OLDEMAILCONFIRMED,
                                user.Id,
                                request.AppId,
                                user.Email,
                                payload.Email);
                        }

                        emailConfirmation = await EnsureEmailConfirmationTokenIsUnique(emailConfirmation);

                        IRepositoryResponse emailConfirmationResponse;

                        if (emailConfirmation.Id == 0)
                        {
                            emailConfirmationResponse = await _emailConfirmationsRepository
                                .CreateAsync(emailConfirmation);
                        }
                        else
                        {
                            emailConfirmationResponse = await _emailConfirmationsRepository
                                .UpdateAsync(emailConfirmation);
                        }

                        string EmailConfirmationAction;

                        if (app.UseCustomEmailConfirmationAction)
                        {
                            if (app.Environment == ReleaseEnvironment.LOCAL)
                            {
                                EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                    app.LocalUrl,
                                    app.CustomEmailConfirmationAction,
                                    emailConfirmation.Token);
                            }
                            else if (app.Environment == ReleaseEnvironment.STAGING)
                            {
                                EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                    app.StagingUrl,
                                    app.CustomEmailConfirmationAction,
                                    emailConfirmation.Token);
                            }
                            else if (app.Environment == ReleaseEnvironment.TEST)
                            {
                                EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                    app.TestUrl,
                                    app.CustomEmailConfirmationAction,
                                    emailConfirmation.Token);
                            }
                            else
                            {
                                EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                    app.ProdUrl,
                                    app.CustomEmailConfirmationAction,
                                    emailConfirmation.Token);
                            }
                        }
                        else
                        {
                            EmailConfirmationAction = string.Format("https://{0}/confirmEmail/{1}",
                                baseUrl,
                                ((EmailConfirmation)emailConfirmationResponse.Object).Token);
                        }

                        var html = File.ReadAllText(emailTemplatePath);
                        var appTitle = app.Name;
                        var url = string.Empty;

                        if (app.Environment == ReleaseEnvironment.LOCAL)
                        {
                            url = app.LocalUrl;
                        }
                        else if (app.Environment == ReleaseEnvironment.STAGING)
                        {
                            url = app.StagingUrl;
                        }
                        else if (app.Environment == ReleaseEnvironment.TEST)
                        {
                            url = app.TestUrl;
                        }
                        else
                        {
                            url = app.ProdUrl;
                        }

                        html = html.Replace("{{USER_NAME}}", user.UserName);
                        html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                        html = html.Replace("{{APP_TITLE}}", appTitle);
                        html = html.Replace("{{URL}}", url);

                        var emailSubject = string.Format("Greetings from {0}: Please Confirm Old Email", appTitle);

                        userResult.ConfirmationEmailSuccessfullySent = await _emailService
                            .SendAsync(user.Email, emailSubject, html, app.Id);
                    }

                    var updateResponse = await _cacheService.UpdateWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        user,
                        request.License);

                    if (!updateResponse.IsSuccess)
                    {
                        result.IsSuccess = updateResponse.IsSuccess;
                        result.Message = updateResponse.Exception != null ? 
                            updateResponse.Exception.Message : 
                            UsersMessages.UserNotUpdatedMessage;

                        return result;
                    }

                    userResult.User = (UserDTO)((User)updateResponse.Object).Cast<UserDTO>();

                    var getRequestorResponse = await _cacheService.GetWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, request.RequestorId, request.License),
                        _cachingStrategy.Medium,
                        request.RequestorId);

                    var requestorResponse = (RepositoryResponse)getRequestorResponse.Item1;

                    if (!((User)requestorResponse.Object).IsSuperUser && request.RequestorId != id)
                    {
                        userResult.User.NullifyEmail();
                    }

                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = UsersMessages.UserUpdatedMessage;
                    result.Payload.Add(userResult);

                    return result;
                }
            }
            catch (ArgumentException e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    result.Message,
                    e,
                    (Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> GetUsersAsync(
            int requestorId, 
            string license, 
            IPaginator paginator)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(requestorId, nameof(requestorId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestorId, nameof(requestorId));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                ArgumentNullException.ThrowIfNull(paginator, nameof(paginator));

                var getResponse = await _usersRepository.GetAllAsync();

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ?
                        getResponse.Exception.Message :
                        UsersMessages.UsersNotFoundMessage;

                    return result;
                }
                #endregion

                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys.GetUsersCacheKey,
                    _cachingStrategy.Medium,
                    result);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                result = (Result)cacheServiceResponse.Item2;

                if (DataUtilities.IsPageValid(paginator, getResponse.Objects))
                {
                    result = PaginatorUtilities.PaginateUsers(paginator, getResponse, result);
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;

                    return result;
                }

                cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                    _cachingStrategy.Medium,
                    license);

                var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                var appAdmins = (await _appAdminsRepository.GetAllAsync())
                    .Objects
                    .ConvertAll(aa => (AppAdmin)aa)
                    .ToList();

                foreach (var user in result.Payload.ConvertAll(u => (IUser)u))
                {
                    if (user
                        .Roles
                        .Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                    {
                        if (!user.IsSuperUser)
                        {
                            if (!appAdmins.Any(aa =>
                                aa.AppId == app.Id &&
                                aa.UserId == user.Id &&
                                aa.IsActive))
                            {
                                var adminRole = user
                                    .Roles
                                    .FirstOrDefault(ur =>
                                        ur.Role.RoleLevel == RoleLevel.ADMIN);

                                user.Roles.Remove(adminRole);
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
                    }
                }

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, requestorId, license),
                    _cachingStrategy.Medium,
                    requestorId);

                var requestor = (User)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                if (!requestor.IsSuperUser)
                {
                    // Filter out user emails from the frontend...
                    foreach (var user in result.Payload.ConvertAll(u => (IUser)u))
                    {
                        if (user.Id != requestorId)
                        {
                            var emailConfirmed = user.IsEmailConfirmed;
                            user.NullifyEmail();
                            user.IsEmailConfirmed = emailConfirmed;
                        }
                    }
                }

                // Transform the payload of users into a payload of translated users
                var transformedUsers = new List<IUserDTO>();

                foreach (var user in result.Payload.ConvertAll(u => (User)u))
                {
                    transformedUsers.Add((UserDTO)user.Cast<UserDTO>());
                }

                result.Payload = transformedUsers.ConvertAll(u => (object)u);

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, requestorId, license),
                    _cachingStrategy.Medium,
                    requestorId);


                result.IsSuccess = getResponse.IsSuccess;
                result.Message = UsersMessages.UsersFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeleteAsync(int id, string license)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var getResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                if (((User)getResponse.Object).Id == 1 && ((User)getResponse.Object).IsSuperUser)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.SuperUserCannotBeDeletedMessage;

                    return result;
                }

                var deleteResponse = await _cacheService.DeleteWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys,
                    (User)getResponse.Object,
                    license);

                #region deleteResponse fails
                if (!deleteResponse.IsSuccess)
                {
                    result.IsSuccess = deleteResponse.IsSuccess;
                    result.Message = deleteResponse.Exception != null ? 
                        deleteResponse.Exception.Message : 
                        UsersMessages.UserNotDeletedMessage;

                    return result;
                }
                #endregion

                var admins = (await _appAdminsRepository.GetAllAsync())
                    .Objects
                    .ConvertAll(aa => (AppAdmin)aa)
                    .Where(aa => aa.UserId == id)
                    .ToList();

                _ = await _appAdminsRepository.DeleteRangeAsync(admins);

                result.IsSuccess = true;
                result.Message = UsersMessages.UserDeletedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetUserByPasswordTokenAsync(string token)
        {
            var result = new Result();

            try
            {
                ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

                var response = await _passwordResetsRepository.GetAsync(token);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.PasswordResetTokenNotFound;

                    return result;
                }
                #endregion

                var license = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, ((PasswordReset)response.Object).AppId),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    ((PasswordReset)response.Object).AppId)).Item1;

                result.Payload.Add((User)((await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, ((PasswordReset)response.Object).UserId, license),
                    _cachingStrategy.Medium,
                    ((PasswordReset)response.Object).UserId,
                    result)).Item1).Object);

                result.IsSuccess = response.IsSuccess;
                result.Message = UsersMessages.UserFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<ILicenseResult> GetAppLicenseByPasswordTokenAsync(string token)
        {
            var result = new LicenseResult();

            try
            {
                ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

                var response = await _passwordResetsRepository.GetAsync(token);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.NoOutstandingRequestToResetPasswordMessage;

                    return result;
                }
                #endregion

                result.License = await _appsRepository.GetLicenseAsync(((PasswordReset)response.Object).AppId);
                result.IsSuccess = response.IsSuccess;
                result.Message = AppsMessages.AppFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> AddUserRolesAsync(
            int userid,
            IRequest request, 
            string license)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(userid, nameof(userid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userid, nameof(userid));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                UpdateUserRolePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserRolePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var rolesExist = await _rolesRepository.IsListValidAsync(payload.RoleIds);

                if (!rolesExist)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.RolesInvalidMessage;

                    return result;
                }

                if (payload.RoleIds.Contains(2) || payload.RoleIds.Contains(4))
                {
                    throw new ArgumentException(RolesMessages.RolesCannotBeAddedUsingThisEndpoint);
                }

                var response = await _usersRepository.AddRolesAsync(userid, payload.RoleIds);

                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.RolesNotAddedMessage;

                    return result;
                }

                var cacheServiceResponse = new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse(),
                    new Result());

                var roles = response
                    .Objects
                    .ConvertAll(ur => (UserRole)ur)
                    .ToList();

                foreach (var role in roles)
                {
                    if (role.Role.RoleLevel == RoleLevel.ADMIN)
                    {
                        cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                            _appsRepository,
                            _distributedCache,
                            string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                            _cachingStrategy.Medium,
                            license);

                        var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                        var appAdmin = (AppAdmin)(await _appAdminsRepository.AddAsync(new AppAdmin(app.Id, userid))).Object;
                    }

                    result.Payload.ConvertAll(r => (Role)r).Add(role.Role);
                }

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userid, license),
                    _cachingStrategy.Medium,
                    userid);

                var user = (User)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                var getRequestorResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, request.RequestorId, request.License),
                    _cachingStrategy.Medium,
                    request.RequestorId);

                var requestorResponse = (RepositoryResponse)getRequestorResponse.Item1;

                if (!((User)requestorResponse.Object).IsSuperUser && request.RequestorId != user.Id)
                {
                    user.NullifyEmail();
                }

                // Remove any user cache items which may exist
                var removeKeys = new List<string> {
                        string.Format(_cacheKeys.GetUserCacheKey, user.Id, license),
                        string.Format(_cacheKeys.GetUserByUsernameCacheKey, user.UserName, license),
                        string.Format(_cacheKeys.GetUserByEmailCacheKey, user.Email, license)
                    };

                await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                user.NullifyPassword();

                result.IsSuccess = response.IsSuccess;
                result.Message = UsersMessages.RolesAddedMessage;
                result.Payload.Add(user);

                return result;
            }
            catch (ArgumentException e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> RemoveUserRolesAsync(
            int userid,
            IRequest request, 
            string license)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(userid, nameof(userid));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userid, nameof(userid));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                UpdateUserRolePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserRolePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var rolesExist = await _rolesRepository.IsListValidAsync(payload.RoleIds);

                if (!rolesExist)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.RolesInvalidMessage;

                    return result;
                }

                if (payload.RoleIds.Contains(2) || payload.RoleIds.Contains(4))
                {
                    throw new ArgumentException(RolesMessages.RolesCannotBeRemovedUsingThisEndpoint);
                }

                var response = await _usersRepository.RemoveRolesAsync(userid, payload.RoleIds);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.RolesNotRemovedMessage;

                    return result;
                }
                #endregion

                var cacheServiceResponse = new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse(),
                    new Result());

                var roles = response
                    .Objects
                    .ConvertAll(ur => (UserRole)ur)
                    .ToList();

                foreach (var role in roles)
                {
                    if (role.Role.RoleLevel == RoleLevel.ADMIN)
                    {
                        cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                            _appsRepository,
                            _distributedCache,
                            string.Format(_cacheKeys.GetAppByLicenseCacheKey, license),
                            _cachingStrategy.Medium,
                            license);

                        var app = (App)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                        var appAdmin = (AppAdmin)
                            (await _appAdminsRepository.GetAdminRecordAsync(app.Id, userid))
                            .Object;

                        _ = await _appAdminsRepository.DeleteAsync(appAdmin);
                    }
                }

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userid, license),
                    _cachingStrategy.Medium,
                    userid);

                var user = (User)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                // Remove any user cache items which may exist
                var removeKeys = new List<string> {
                        string.Format(_cacheKeys.GetUserCacheKey, user.Id, license),
                        string.Format(_cacheKeys.GetUserByUsernameCacheKey, user.UserName, license),
                        string.Format(_cacheKeys.GetUserByEmailCacheKey, user.Email, license)
                    };

                await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                result.IsSuccess = response.IsSuccess;
                result.Message = UsersMessages.RolesRemovedMessage;

                return result;
            }
            catch (ArgumentException e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> ActivateAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                if (await _usersRepository.ActivateAsync(id))
                {
                    var license = (await _cacheService.GetLicenseWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppLicenseCacheKey, 1),
                        _cachingStrategy.Medium,
                        _cacheKeys,
                        1)).Item1;

                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, id, license),
                        _cachingStrategy.Medium,
                        id);

                    var user = (User)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                    // Remove any user cache items which may exist
                    var removeKeys = new List<string> {
                                string.Format(_cacheKeys.GetUserCacheKey, user.Id, license),
                                string.Format(_cacheKeys.GetUserByUsernameCacheKey, user.UserName, license),
                                string.Format(_cacheKeys.GetUserByEmailCacheKey, user.Email, license)
                            };

                    await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                    user.NullifyPassword();

                    result.IsSuccess = true;
                    result.Message = UsersMessages.UserActivatedMessage;
                    result.Payload.Add(user);

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotActivatedMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
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
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                if (await _usersRepository.DeactivateAsync(id))
                {
                    var license = (await _cacheService.GetLicenseWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppLicenseCacheKey, 1),
                        _cachingStrategy.Medium,
                        _cacheKeys,
                        1)).Item1;

                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, id, license),
                        _cachingStrategy.Medium,
                        id);

                    var user = (User)((RepositoryResponse)cacheServiceResponse.Item1).Object;

                    // Remove any user cache items which may exist
                    var removeKeys = new List<string> {
                                string.Format(_cacheKeys.GetUserCacheKey, user.Id, license),
                                string.Format(_cacheKeys.GetUserByUsernameCacheKey, user.UserName, license),
                                string.Format(_cacheKeys.GetUserByEmailCacheKey, user.Email, license)
                            };

                    await _cacheService.RemoveKeysAsync(_distributedCache, removeKeys);

                    result.IsSuccess = true;
                    result.Message = UsersMessages.UserDeactivatedMessage;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotDeactivatedMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> ResendEmailConfirmationAsync(
            int userId, 
            int appId, 
            string baseUrl, 
            string emailTemplatePath, 
            string license)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                var userResult = new UserResult();

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, license),
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

                var user = (User)userResponse.Object;

                if (user.IsEmailConfirmed || !user.ReceivedRequestToUpdateEmail)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailConfirmedMessage;

                    return result;
                }

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)appResponse.Object;

                var hasOutstandingConfirmation = await _emailConfirmationsRepository.HasOutstandingEmailConfirmationAsync(userId, appId);

                if (!hasOutstandingConfirmation)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailConfirmationRequestNotFoundMessage;

                    return result;
                }

                var emailConfirmationResponse = await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(userId, appId);

                #region emailConfirmationResponse fails
                if (!emailConfirmationResponse.IsSuccess)
                {
                    result.IsSuccess = emailConfirmationResponse.IsSuccess;
                    result.Message = emailConfirmationResponse.Exception != null ? 
                        emailConfirmationResponse.Exception.Message : 
                        UsersMessages.EmailConfirmationRequestNotFoundMessage;

                    return result;
                }
                #endregion

                var emailConfirmation = (EmailConfirmation)emailConfirmationResponse.Object;

                string EmailConfirmationAction;

                if (app.UseCustomEmailConfirmationAction)
                {
                    if (app.Environment == ReleaseEnvironment.LOCAL)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.LocalUrl,
                            app.CustomEmailConfirmationAction,
                            emailConfirmation.Token);
                    }
                    else if (app.Environment == ReleaseEnvironment.STAGING)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.StagingUrl,
                            app.CustomEmailConfirmationAction,
                            emailConfirmation.Token);
                    }
                    else if (app.Environment == ReleaseEnvironment.TEST)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.TestUrl,
                            app.CustomEmailConfirmationAction,
                            emailConfirmation.Token);
                    }
                    else
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.ProdUrl,
                            app.CustomEmailConfirmationAction,
                            emailConfirmation.Token);
                    }
                }
                else
                {
                    EmailConfirmationAction = string.Format("https://{0}/confirmEmail/{1}",
                        baseUrl,
                        ((EmailConfirmation)emailConfirmationResponse.Object).Token);
                }

                var html = File.ReadAllText(emailTemplatePath);
                var appTitle = app.Name;
                var url = string.Empty;

                if (app.Environment == ReleaseEnvironment.LOCAL)
                {
                    url = app.LocalUrl;
                }
                else if (app.Environment == ReleaseEnvironment.STAGING)
                {
                    url = app.StagingUrl;
                }
                else if (app.Environment == ReleaseEnvironment.TEST)
                {
                    url = app.TestUrl;
                }
                else
                {
                    url = app.ProdUrl;
                }

                html = html.Replace("{{USER_NAME}}", user.UserName);
                html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                html = html.Replace("{{APP_TITLE}}", appTitle);
                html = html.Replace("{{URL}}", url);

                var emailSubject = string.Format("Greetings from {0}: Please Confirm Email", appTitle);

                userResult.ConfirmationEmailSuccessfullySent = await _emailService
                    .SendAsync(user.Email, emailSubject, html, app.Id);

                user.NullifyPassword();

                var emailSent = (bool)userResult.ConfirmationEmailSuccessfullySent;

                if (!emailSent)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailConfirmationEmailNotResentMessage;

                    return result;
                }

                userResult.User = (UserDTO)user.Cast<UserDTO>();
                result.IsSuccess = true;
                result.Message = UsersMessages.EmailConfirmationEmailResentMessage;
                result.Payload.Add(userResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> ConfirmEmailAsync(
            string token, 
            string baseUrl, 
            string emailTemplatePath)
        {
            var result = new Result();

            try
            {
                ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                var emailConfirmResult = new ConfirmEmailResult();

                var emailConfirmations = (await _emailConfirmationsRepository.GetAllAsync()).Objects.ConvertAll(x => (EmailConfirmation)x);

                var emailConfirmation = emailConfirmations.FirstOrDefault(ec => ec.Token.ToLower().Equals(token.ToLower()));

                if (emailConfirmation == null)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailConfirmationTokenNotFound;

                    return result;
                }

                var license = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, emailConfirmation.AppId),
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    emailConfirmation.AppId)).Item1;

                if (!emailConfirmation.IsUpdate && emailConfirmation.ConfirmationType == EmailConfirmationType.NEWPROFILECONFIRMED)
                {
                    var response = await _cacheService.ConfirmEmailWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        emailConfirmation,
                        license);

                    #region response fails
                    if (!response.IsSuccess)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception != null ? 
                            response.Exception.Message : 
                            UsersMessages.EmailConfirmationTokenNotFound;

                        return result;
                    }
                    #endregion

                    var user = (User)response.Object;

                    if (DateTime.UtcNow > emailConfirmation.ExpirationDate && emailConfirmation.IsUpdate)
                    {
                        user.Email = emailConfirmation.OldEmailAddress;
                        user.ReceivedRequestToUpdateEmail = false;

                        _ = await _cacheService.UpdateWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            license);

                        _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmationTokenExpired;

                        return result;
                    }

                    result.IsSuccess = response.IsSuccess;

                    emailConfirmResult.ConfirmationType = EmailConfirmationType.NEWPROFILECONFIRMED;
                    emailConfirmResult.IsUpdate = emailConfirmation.IsUpdate;
                    emailConfirmResult.UserName = user.UserName;
                    emailConfirmResult.Email = user.Email;
                    emailConfirmResult.NewEmailAddressConfirmed = true;
                    emailConfirmResult.ConfirmationType = emailConfirmation.ConfirmationType;
                    emailConfirmResult.AppTitle = user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Name)
                        .FirstOrDefault();

                    if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.LOCAL)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.LocalUrl)
                            .FirstOrDefault();
                    }
                    else if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.STAGING)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.StagingUrl)
                            .FirstOrDefault();
                    }
                    else if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.TEST)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.TestUrl)
                            .FirstOrDefault();
                    }
                    else
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.ProdUrl)
                            .FirstOrDefault();
                    }
                            
                    _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                    user.ReceivedRequestToUpdateEmail = false;

                    _ = await _cacheService.UpdateWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        user,
                        license);

                    result.Message = UsersMessages.EmailConfirmedMessage;
                    result.Payload.Add(emailConfirmResult);

                    return result;
                }
                else if (emailConfirmation.IsUpdate && emailConfirmation.ConfirmationType == EmailConfirmationType.OLDEMAILCONFIRMED)
                {
                    var response = await _cacheService.UpdateEmailWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        emailConfirmation,
                        license);

                    #region response fails
                    if (!response.IsSuccess)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception != null ? 
                            response.Exception.Message : 
                            UsersMessages.OldEmailNotConfirmedMessage;

                        return result;
                    }
                    #endregion

                    var user = (User)response.Object;

                    if (DateTime.UtcNow > emailConfirmation.ExpirationDate && emailConfirmation.IsUpdate)
                    {
                        user.Email = emailConfirmation.OldEmailAddress;
                        user.ReceivedRequestToUpdateEmail = false;

                        _ = await _cacheService.UpdateWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            license);

                        _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmationTokenExpired;

                        return result;
                    }

                    var app = (App)(await _appsRepository.GetAsync(emailConfirmation.AppId)).Object;

                    var html = File.ReadAllText(emailTemplatePath);

                    var url = string.Empty;

                    if (app.Environment == ReleaseEnvironment.LOCAL)
                    {
                        url = app.LocalUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.STAGING)
                    {
                        url = app.StagingUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.TEST)
                    {
                        url = app.TestUrl;
                    }
                    else
                    {
                        url = app.ProdUrl;
                    }

                    string EmailConfirmationAction;

                    if (!app.DisableCustomUrls && !string.IsNullOrEmpty(app.CustomEmailConfirmationAction))
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            url,
                            app.CustomEmailConfirmationAction,
                            emailConfirmation.Token);
                    }
                    else
                    {
                        EmailConfirmationAction = string.Format("https://{0}/confirmEmail/{1}",
                            baseUrl,
                            emailConfirmation.Token);
                    }

                    var appTitle = app.Name;

                    html = html.Replace("{{USER_NAME}}", user.UserName);
                    html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                    html = html.Replace("{{APP_TITLE}}", appTitle);
                    html = html.Replace("{{URL}}", url);

                    var emailSubject = string.Format("Greetings from {0}: Please Confirm New Email", appTitle);

                    emailConfirmResult.ConfirmationEmailSuccessfullySent = await _emailService
                        .SendAsync(user.Email, emailSubject, html, app.Id);

                    emailConfirmation.OldEmailAddressConfirmed = emailConfirmResult.ConfirmationEmailSuccessfullySent;
                    emailConfirmation.ConfirmationType = EmailConfirmationType.NEWPROFILECONFIRMED;

                    emailConfirmation = (EmailConfirmation)(await _emailConfirmationsRepository.UpdateAsync(emailConfirmation)).Object;

                    result.IsSuccess = response.IsSuccess;
                    result.Message = UsersMessages.OldEmailConfirmedMessage;

                    emailConfirmResult.ConfirmationType = EmailConfirmationType.OLDEMAILCONFIRMED;
                    emailConfirmResult.IsUpdate = emailConfirmation.IsUpdate;
                    emailConfirmResult.UserName = user.UserName;
                    emailConfirmResult.Email = emailConfirmation.NewEmailAddress;
                    emailConfirmResult.AppTitle = appTitle;
                    emailConfirmResult.AppUrl = url;

                    result.Payload.Add(emailConfirmResult);

                    return result;
                }
                else
                {
                    var response = await _cacheService.ConfirmEmailWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        emailConfirmation,
                        license);

                    #region response fails
                    if (!response.IsSuccess)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception != null ? 
                            response.Exception.Message : 
                            UsersMessages.EmailConfirmationTokenNotFound;

                        return result;
                    }
                    #endregion

                    var user = (User)response.Object;

                    if (DateTime.UtcNow > emailConfirmation.ExpirationDate && emailConfirmation.IsUpdate)
                    {
                        user.Email = emailConfirmation.OldEmailAddress;
                        user.ReceivedRequestToUpdateEmail = false;

                        _ = await _cacheService.UpdateWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            license);

                        _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmationTokenExpired;

                        return result;
                    }

                    result.IsSuccess = response.IsSuccess;

                    if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.LOCAL)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.LocalUrl)
                            .FirstOrDefault();
                    }
                    else if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.STAGING)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.StagingUrl)
                            .FirstOrDefault();
                    }
                    else if (user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Environment == ReleaseEnvironment.TEST)
                        .FirstOrDefault())
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.TestUrl)
                            .FirstOrDefault();
                    }
                    else
                    {
                        emailConfirmResult.AppUrl = user
                            .Apps
                            .Where(ua => ua.AppId == emailConfirmation.AppId)
                            .Select(ua => ua.App.ProdUrl)
                            .FirstOrDefault();
                    }

                    user.ReceivedRequestToUpdateEmail = false;

                    _ = await _cacheService.UpdateWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        user,
                        license);

                    result.Message = UsersMessages.EmailConfirmedMessage;

                    emailConfirmResult.ConfirmationType = EmailConfirmationType.NEWEMAILCONFIRMED;
                    emailConfirmResult.IsUpdate = emailConfirmation.IsUpdate;
                    emailConfirmResult.Email = user.Email;
                    emailConfirmResult.UserName = user.UserName;
                    emailConfirmResult.NewEmailAddressConfirmed = true;
                    emailConfirmResult.AppTitle = user
                        .Apps
                        .Where(ua => ua.AppId == emailConfirmation.AppId)
                        .Select(ua => ua.App.Name)
                        .FirstOrDefault();

                    result.Payload.Add(emailConfirmResult);

                    _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> CancelEmailConfirmationRequestAsync(int id, int appId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));
                
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                var userResult = new UserResult();

                var license = await _appsRepository.GetLicenseAsync(appId);

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var hasEmailConfirmation = await _emailConfirmationsRepository.HasOutstandingEmailConfirmationAsync(id, appId);

                if (!hasEmailConfirmation)
                {
                    userResult.User = (UserDTO)(await _usersRepository.GetAsync(id)).Object;
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailConfirmationRequestNotFoundMessage;
                    result.Payload.Add(userResult);

                    return result;
                }

                var user = (User)userResponse.Object;

                var emailConfirmation = (EmailConfirmation)
                    (await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(id, appId))
                    .Object;

                var response = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                #region response fails
                if (!response.IsSuccess)
                {
                    userResult.User = (UserDTO)(await _usersRepository.UpdateAsync(user)).Object;
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        UsersMessages.EmailConfirmationRequestNotCancelledMessage;
                    result.Payload.Add(userResult);

                    return result;
                }
                #endregion

                // Role back email request
                user.Email = emailConfirmation.OldEmailAddress;
                user.ReceivedRequestToUpdateEmail = false;
                user.IsEmailConfirmed = true;

                userResult.User = (UserDTO)(await _cacheService.UpdateWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys,
                    user,
                    license)).Object.Cast<UserDTO>();

                result.IsSuccess = response.IsSuccess;
                result.Message = UsersMessages.EmailConfirmationRequestCancelledMessage;
                result.Payload.Add(userResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> CancelAllEmailRequestsAsync(int id, int appId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                var userResult = new UserResult();

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;
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
                    string.Format(_cacheKeys.GetUserCacheKey, id, app.License),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }

                var appExists = await _appsRepository.HasEntityAsync(appId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var emailConfirmationResponse = await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(id, appId);
                var passwordResetResponse = await _passwordResetsRepository.RetrievePasswordResetAsync(id, appId);

                var user = (User)userResponse.Object;

                if (!emailConfirmationResponse.IsSuccess || !passwordResetResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailRequestsNotFoundMessage;

                    return result;
                }

                if (emailConfirmationResponse.IsSuccess)
                {
                    var emailConfirmation = (EmailConfirmation)emailConfirmationResponse.Object;

                    var response = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                    if (response.IsSuccess)
                    {
                        // Role back email request
                        user.Email = emailConfirmation.OldEmailAddress;
                        user.ReceivedRequestToUpdateEmail = false;
                        user.IsEmailConfirmed = true;

                        user = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            app.License)).Object;

                        result.IsSuccess = response.IsSuccess;
                        result.Message = UsersMessages.EmailConfirmationRequestCancelledMessage;
                    }
                    else if (response.IsSuccess == false && response.Exception != null)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception.Message;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmationRequestNotCancelledMessage;
                    }
                }

                if (passwordResetResponse.IsSuccess)
                {
                    var passwordReset = (PasswordReset)passwordResetResponse.Object;

                    var response = await _passwordResetsRepository.DeleteAsync(passwordReset);

                    if (response.IsSuccess)
                    {
                        // Role back password reset
                        user.ReceivedRequestToUpdatePassword = false;

                        user = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            app.License)).Object;

                        result.IsSuccess = response.IsSuccess;
                        result.Message = string.IsNullOrEmpty(result.Message) ?
                            UsersMessages.PasswordResetRequestCancelledMessage :
                            string.Format("{0} and {1}", result.Message.TrimEnd('.'), UsersMessages.PasswordResetRequestCancelledMessage.ToLower());
                    }
                    else if (response.IsSuccess == false && response.Exception != null)
                    {
                        result.IsSuccess = result.IsSuccess ? result.IsSuccess : response.IsSuccess;
                        result.Message = string.IsNullOrEmpty(result.Message) ?
                            response.Exception.Message :
                            string.Format("{0} and {1}", result.Message, response.Exception.Message);
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = string.IsNullOrEmpty(result.Message) ?
                            UsersMessages.PasswordResetRequestNotCancelledMessage :
                            string.Format("{0} and {1}", result.Message, UsersMessages.PasswordResetRequestNotCancelledMessage);
                    }
                }

                userResult.User = (UserDTO)user.Cast<UserDTO>();
                result.Payload.Add(userResult);
                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> InitiatePasswordResetAsync(string token, string license)
        {
            var result = new Result();

            try
            {
                ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));

                ArgumentException.ThrowIfNullOrEmpty(license, nameof(license));

                var initiatePasswordResetResult = new InitiatePasswordResetResult();

                var passwordResetResponse = await _passwordResetsRepository.GetAsync(token);

                #region passwordResetResponse fails
                if (!passwordResetResponse.IsSuccess)
                {
                    result.IsSuccess = passwordResetResponse.IsSuccess;
                    result.Message = passwordResetResponse.Exception != null ? 
                        passwordResetResponse.Exception.Message : 
                        UsersMessages.PasswordResetRequestNotFoundMessage;

                    return result;
                }
                #endregion

                var passwordReset = (PasswordReset)passwordResetResponse.Object;

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, passwordReset.UserId, license),
                    _cachingStrategy.Medium,
                    passwordReset.UserId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = passwordResetResponse.IsSuccess;
                    result.Message = userResponse.Exception != null ? 
                        passwordResetResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, passwordReset.AppId),
                    _cachingStrategy.Medium,
                    passwordReset.AppId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse fails
                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = passwordResetResponse.IsSuccess;
                    result.Message = appResponse.Exception != null ? 
                        passwordResetResponse.Exception.Message : 
                        AppsMessages.AppNotFoundMessage;

                    return result;
                }
                #endregion

                var app = (App)appResponse.Object;

                user.NullifyPassword();

                var userSignedUp = user.Apps.Any(ua => ua.AppId == app.Id);

                if (!userSignedUp)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.UserNotSignedUpToAppMessage;

                    return result;
                }

                if (!user.ReceivedRequestToUpdatePassword)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoOutstandingRequestToResetPasswordMessage;

                    return result;
                }

                result.IsSuccess = true;
                result.Message = UsersMessages.UserFoundMessage;

                initiatePasswordResetResult.User = (UserDTO)user.Cast<UserDTO>();
                initiatePasswordResetResult.App = app;

                result.Payload.Add(initiatePasswordResetResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> ResendPasswordResetAsync(
            int userId, 
            int appId, 
            string baseUrl, 
            string emailTemplatePath)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(userId, nameof(userId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!appResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var app = (App)((await _appsRepository.GetAsync(appId)).Object);
                app.License = await _appsRepository.GetLicenseAsync(app.Id);

                cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                    _cachingStrategy.Medium,
                    userId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }

                var user = (User)userResponse.Object;

                if (!user.ReceivedRequestToUpdatePassword)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoOutstandingRequestToResetPasswordMessage;

                    return result;
                }

                if (!await _passwordResetsRepository.HasOutstandingPasswordResetAsync(userId, appId))
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoOutstandingRequestToResetPasswordMessage;

                    return result;
                }

                var passwordReset = (PasswordReset)
                    ((await _passwordResetsRepository.RetrievePasswordResetAsync(userId, appId)).Object);

                string EmailConfirmationAction;

                if (app.UseCustomPasswordResetAction)
                {
                    if (app.Environment == ReleaseEnvironment.LOCAL)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.LocalUrl,
                            app.CustomPasswordResetAction,
                            passwordReset.Token);
                    }
                    else if (app.Environment == ReleaseEnvironment.STAGING)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.StagingUrl,
                            app.CustomPasswordResetAction,
                            passwordReset.Token);
                    }
                    else if (app.Environment == ReleaseEnvironment.TEST)
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.TestUrl,
                            app.CustomPasswordResetAction,
                            passwordReset.Token);
                    }
                    else
                    {
                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                            app.ProdUrl,
                            app.CustomPasswordResetAction,
                            passwordReset.Token);
                    }
                }
                else
                {
                    EmailConfirmationAction = string.Format("https://{0}/passwordReset/{1}",
                        baseUrl,
                        passwordReset.Token);
                }

                var html = File.ReadAllText(emailTemplatePath);
                var appTitle = app.Name;
                var url = string.Empty;

                if (app.Environment == ReleaseEnvironment.LOCAL)
                {
                    url = app.LocalUrl;
                }
                else if (app.Environment == ReleaseEnvironment.STAGING)
                {
                    url = app.StagingUrl;
                }
                else if (app.Environment == ReleaseEnvironment.TEST)
                {
                    url = app.TestUrl;
                }
                else
                {
                    url = app.ProdUrl;
                }

                html = html.Replace("{{USER_NAME}}", user.UserName);
                html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                html = html.Replace("{{APP_TITLE}}", appTitle);
                html = html.Replace("{{URL}}", url);

                var emailSubject = string.Format("Greetings from {0}: Password Update Request Received", appTitle);

                result.IsSuccess = await _emailService.SendAsync(
                    user.Email, 
                    emailSubject, 
                    html, 
                    app.Id);

                if (!result.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.PasswordResetEmailNotResentMessage;

                    return result;
                }

                result.Message = UsersMessages.PasswordResetEmailResentMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> RequestPasswordResetAsync(
            IRequestPasswordResetRequest request, 
            string baseUrl, 
            string emailTemplatePath)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                var cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, request.License),
                    _cachingStrategy.Medium,
                    request.License);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region appResponse
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
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, app.Id),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    app.Id)).Item1;

                cacheServiceResponse = await _cacheService.GetByEmailWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserByEmailCacheKey, request.Email, app.License),
                    _cachingStrategy.Medium,
                    request.Email);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.NoUserIsUsingThisEmailMessage;

                    return result;
                }

                var user = (User)userResponse.Object;
                PasswordReset passwordReset;

                var userSignedUp = user.Apps.Any(ua => ua.AppId == app.Id);

                if (!userSignedUp)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.UserNotSignedUpToAppMessage;

                    return result;
                }

                if (!user.IsEmailConfirmed)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserEmailNotConfirmedMessage;

                    return result;
                }

                if (await _passwordResetsRepository.HasOutstandingPasswordResetAsync(user.Id, app.Id))
                {
                    passwordReset = (PasswordReset)(await _passwordResetsRepository.RetrievePasswordResetAsync(
                        user.Id,
                        app.Id)).Object;

                    passwordReset = await EnsurePasswordResetTokenIsUnique(passwordReset);

                    passwordReset = (PasswordReset)(await _passwordResetsRepository.UpdateAsync(passwordReset)).Object;

                    if (!user.ReceivedRequestToUpdatePassword)
                    {
                        user.ReceivedRequestToUpdatePassword = true;

                        user = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            app.License)).Object;
                    }

                    return await SendPasswordResetEmailAsync(
                        user,
                        app,
                        passwordReset,
                        emailTemplatePath,
                        baseUrl,
                        result,
                        false);
                }
                else
                {
                    passwordReset = new PasswordReset(user.Id, app.Id);

                    passwordReset = await EnsurePasswordResetTokenIsUnique(passwordReset);

                    var passwordResetResponse = await _passwordResetsRepository.CreateAsync(passwordReset);

                    #region passwordResetResponse fails
                    if (!passwordResetResponse.IsSuccess)
                    {
                        result.IsSuccess = passwordResetResponse.IsSuccess;
                        result.Message = passwordResetResponse.Exception != null ? 
                            passwordResetResponse.Exception.Message : 
                            UsersMessages.UnableToProcessPasswordResetRequesMessage;

                        return result;
                    }
                    #endregion

                    user.ReceivedRequestToUpdatePassword = true;

                    user = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        user,
                        app.License)).Object;

                    return await SendPasswordResetEmailAsync(
                        user,
                        app,
                        passwordReset,
                        emailTemplatePath,
                        baseUrl,
                        result,
                        true);
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> UpdatePasswordAsync(IUpdatePasswordRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var salt = BCrypt.Net.BCrypt.GenerateSalt();

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, request.UserId, request.License),
                    _cachingStrategy.Medium,
                    request.UserId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = userResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.UserNotFoundMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;

                cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppByLicenseCacheKey, request.License),
                    _cachingStrategy.Medium,
                    request.License);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;
                var app = (App)appResponse.Object;

                app.License = (await _cacheService.GetLicenseWithCacheAsync(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppLicenseCacheKey, app.Id),
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    app.Id)).Item1;

                if (!user.ReceivedRequestToUpdatePassword)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoOutstandingRequestToResetPasswordMessage;

                    return result;
                }

                var passwordResetReponse = await _passwordResetsRepository
                    .RetrievePasswordResetAsync(
                        user.Id,
                        app.Id);

                #region passwordResetReponse fails
                if (!passwordResetReponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = passwordResetReponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.PasswordResetRequestNotFoundMessage;

                    return result;
                }
                #endregion

                var passwordReset = (PasswordReset)passwordResetReponse.Object;

                if (DateTime.UtcNow > passwordReset.ExpirationDate)
                {
                    user.ReceivedRequestToUpdatePassword = false;

                    _ = await _usersRepository.UpdateAsync(user);
                    _ = await _passwordResetsRepository.DeleteAsync(passwordReset);

                    result.IsSuccess = false;
                    result.Message = UsersMessages.PasswordResetTokenExpired;

                    return result;
                }

                user.Password = BCrypt.Net.BCrypt
                        .HashPassword(request.NewPassword, salt);

                user.DateUpdated = DateTime.UtcNow;

                user.ReceivedRequestToUpdatePassword = false;

                var updateResponse = await _cacheService.UpdateWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys,
                    user,
                    app.License);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = userResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        userResponse.Exception.Message : 
                        UsersMessages.PasswordNotResetMessage;

                    return result;
                }
                #endregion

                _ = await _passwordResetsRepository.DeleteAsync(passwordReset);

                user = (User)updateResponse.Object;

                user.NullifyPassword();

                result.IsSuccess = userResponse.IsSuccess;
                result.Message = UsersMessages.PasswordResetMessage;
                result.Payload.Add(user.Cast<UserDTO>());

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> CancelPasswordResetRequestAsync(int id, int appId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                var userResult = new UserResult();

                var license = await _appsRepository.GetLicenseAsync(appId);

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }

                var hasPasswordReset = await _passwordResetsRepository.HasOutstandingPasswordResetAsync(id, appId);

                if (!hasPasswordReset)
                {
                    userResult.User = (UserDTO)(await _usersRepository.GetAsync(id)).Object;
                    result.IsSuccess = false;
                    result.Message = UsersMessages.PasswordResetRequestNotFoundMessage;
                    result.Payload.Add(userResult);

                    return result;
                }

                var user = (User)userResponse.Object;

                var passwordReset = (PasswordReset)
                    (await _passwordResetsRepository.RetrievePasswordResetAsync(id, appId))
                    .Object;

                var deleteResponse = await _passwordResetsRepository.DeleteAsync(passwordReset);

                #region deleteResponse fails
                if (deleteResponse.IsSuccess == false)
                {
                    userResult.User = (UserDTO)(await _usersRepository.UpdateAsync(user)).Object;
                    result.IsSuccess = deleteResponse.IsSuccess;
                    result.Message = deleteResponse.Exception != null ? 
                        deleteResponse.Exception.Message : 
                        UsersMessages.PasswordResetRequestNotCancelledMessage;
                    result.Payload.Add(userResult);

                    return result;
                }
                #endregion

                // Role back password reset
                user.ReceivedRequestToUpdatePassword = false;

                userResult.User = (UserDTO)(await _cacheService.UpdateWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys,
                    user,
                    license)).Object.Cast<UserDTO>();

                result.IsSuccess = deleteResponse.IsSuccess;
                result.Message = UsersMessages.PasswordResetRequestCancelledMessage;
                result.Payload.Add(userResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }
        
        private async Task<EmailConfirmation> EnsureEmailConfirmationTokenIsUnique(EmailConfirmation emailConfirmation)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(emailConfirmation, nameof(emailConfirmation));

                var emailConfirmationResponse = await _emailConfirmationsRepository.GetAllAsync();

                if (emailConfirmationResponse.IsSuccess)
                {
                    bool tokenNotUnique;

                    var emailConfirmations = emailConfirmationResponse
                        .Objects
                        .ConvertAll(ec => (EmailConfirmation)ec);

                    do
                    {
                        if (emailConfirmations
                            .Any(ec => ec.Token.ToLower()
                            .Equals(emailConfirmation.Token.ToLower()) && ec.Id != emailConfirmation.Id))
                        {
                            tokenNotUnique = true;

                            emailConfirmation.Token = Guid.NewGuid().ToString();
                        }
                        else
                        {
                            tokenNotUnique = false;
                        }

                    } while (tokenNotUnique);
                }

                return emailConfirmation;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }

        private async Task<PasswordReset> EnsurePasswordResetTokenIsUnique(PasswordReset passwordReset)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(passwordReset, nameof(passwordReset));

                var passwordResetResponse = await _passwordResetsRepository.GetAllAsync();

                if (passwordResetResponse.IsSuccess)
                {
                    bool tokenUnique;

                    var passwordResets = passwordResetResponse
                        .Objects
                        .ConvertAll(pu => (PasswordReset)pu);

                    do
                    {
                        if (passwordResets
                            .Where(pw => pw.Id != passwordReset.Id)
                            .ToList()
                            .Count > 0)
                        {
                            if (passwordResets
                                .Where(pw => pw.Id != passwordReset.Id)
                                .Any(pw => pw.Token.ToLower().Equals(passwordReset.Token.ToLower())))
                            {
                                tokenUnique = false;

                                passwordReset.Token = Guid.NewGuid().ToString();
                            }
                            else
                            {
                                tokenUnique = true;
                            }
                        }
                        else
                        {
                            passwordReset.Token = Guid.NewGuid().ToString();

                            tokenUnique = true;
                        }

                    } while (!tokenUnique);
                }
                else
                {
                    if (passwordReset.Id != 0)
                    {
                        passwordReset.Token = Guid.NewGuid().ToString();
                    }
                }

                return passwordReset;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }

        private async Task<Result> SendPasswordResetEmailAsync(
            User user,
            App app,
            PasswordReset passwordReset,
            string emailTemplatePath,
            string baseUrl,
            Result result,
            bool newRequest)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(user, nameof(user));

                ArgumentNullException.ThrowIfNull(app, nameof(app));

                ArgumentNullException.ThrowIfNull(passwordReset, nameof(passwordReset));

                ArgumentException.ThrowIfNullOrEmpty(emailTemplatePath, nameof(emailTemplatePath));

                ArgumentException.ThrowIfNullOrEmpty(baseUrl, nameof(baseUrl));

                ArgumentNullException.ThrowIfNull(result, nameof(result));

                ArgumentNullException.ThrowIfNull(newRequest, nameof(newRequest));

                string EmailConfirmationAction;

                if (app.UseCustomPasswordResetAction)
                {
                    string emailUrl;

                    if (app.Environment == ReleaseEnvironment.LOCAL)
                    {
                        emailUrl = app.LocalUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.STAGING)
                    {
                        emailUrl = app.StagingUrl;
                    }
                    else if (app.Environment == ReleaseEnvironment.TEST)
                    {
                        emailUrl = app.TestUrl;
                    }
                    else
                    {
                        emailUrl = app.StagingUrl;
                    }

                    EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                        emailUrl,
                        app.CustomPasswordResetAction,
                        passwordReset.Token);
                }
                else
                {
                    EmailConfirmationAction = string.Format("https://{0}/passwordReset/{1}",
                        baseUrl,
                        passwordReset.Token);
                }

                var html = File.ReadAllText(emailTemplatePath);
                var appTitle = app.Name;
                string url;

                if (app.Environment == ReleaseEnvironment.LOCAL)
                {
                    url = app.LocalUrl;
                }
                else if (app.Environment == ReleaseEnvironment.STAGING)
                {
                    url = app.StagingUrl;
                }
                else if (app.Environment == ReleaseEnvironment.TEST)
                {
                    url = app.TestUrl;
                }
                else
                {
                    url = app.StagingUrl;
                }

                html = html.Replace("{{USER_NAME}}", user.UserName);
                html = html.Replace("{{CONFIRM_EMAIL_URL}}", EmailConfirmationAction);
                html = html.Replace("{{APP_TITLE}}", appTitle);
                html = html.Replace("{{URL}}", url);

                var emailSubject = string.Format("Greetings from {0}: Password Update Request Received", appTitle);

                result.IsSuccess = await _emailService.SendAsync(user.Email, emailSubject, html, app.Id);

                if (!result.IsSuccess)
                {
                    result.Message = UsersMessages.UnableToProcessPasswordResetRequesMessage;

                    return result;
                }

                if (newRequest)
                {
                    result.Message = UsersMessages.ProcessedPasswordResetRequestMessage;
                }
                else
                {
                    result.Message = UsersMessages.ResentPasswordResetRequestMessage;
                }

                result.Payload.Add(user.Cast<UserDTO>());

                return result;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (Request)_requestService.Get());

                throw;
            }
        }
        #endregion
    }
}
