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

namespace SudokuCollective.Data.Services
{
    public class UsersService : IUsersService
    {
        #region Fields
        private readonly IUsersRepository<User> _usersRepository;
        private readonly IAppsRepository<App> _appsRepository;
        private readonly IRolesRepository<Role> _rolesRepository;
        private readonly IAppAdminsRepository<AppAdmin> _appAdminsRepository;
        private readonly IEmailConfirmationsRepository<EmailConfirmation> _emailConfirmationsRepository;
        private readonly IPasswordResetsRepository<PasswordReset> _passwordResetsRepository;
        private readonly IEmailService _emailService;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly ILogger<UsersService> _logger;
        #endregion

        #region Constructor
        public UsersService(
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
            ILogger<UsersService> logger)
        {
            _usersRepository = usersRepository;
            _appsRepository = appsRepository;
            _rolesRepository = rolesRepository;
            _appAdminsRepository = appAdminsRepository;
            _emailConfirmationsRepository = emailConfirmationsRepository;
            _passwordResetsRepository = passwordResetsRepository;
            _emailService = emailService;
            _requestService = requestService;
            _distributedCache = distributedCache;
            _cacheService = cacheService;
            _cacheKeys = cacheKeys;
            _cachingStrategy = cachingStrategy;
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<IResult> CreateAsync(
            ISignupRequest request, 
            string baseUrl, 
            string emailTemplatePath)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentNullException(nameof(emailTemplatePath));

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

                    if (appResponse.IsSuccess)
                    {
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
                            false,
                            BCrypt.Net.BCrypt.HashPassword(request.Password, salt),
                            false,
                            true,
                            DateTime.UtcNow,
                            DateTime.MinValue);

                        user.Apps.Add(
                            new UserApp()
                            {
                                User = user,
                                App = app,
                                AppId = app.Id
                            });

                        var userResponse = await _cacheService.AddWithCacheAsync<User>(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys.GetUserCacheKey,
                            _cachingStrategy.Medium,
                            _cacheKeys,
                            user);

                        if (userResponse.IsSuccess)
                        {
                            user = (User)userResponse.Object;

                            if (user.Roles.Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                            {
                                var appAdmin = new AppAdmin(app.Id, user.Id);

                                _ = await _appAdminsRepository.AddAsync(appAdmin);
                            }

                            var emailConfirmation = new EmailConfirmation(
                                user.Id,
                                app.Id);

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
                                else if (app.Environment == ReleaseEnvironment.QA)
                                {
                                    EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                        app.QaUrl,
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
                            else if (app.Environment == ReleaseEnvironment.QA)
                            {
                                url = app.QaUrl;
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
                        else if (!userResponse.IsSuccess && userResponse.Exception != null)
                        {
                            result.IsSuccess = userResponse.IsSuccess;
                            result.Message = userResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = userResponse.IsSuccess;
                            result.Message = UsersMessages.UserNotCreatedMessage;

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
            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));

            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    var user = (User)response.Object;

                    user.NullifyPassword();

                    result.IsSuccess = response.IsSuccess;
                    result.Message = UsersMessages.UserFoundMessage;
                    result.Payload.Add(user);

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

                    if (((User)(result
                        .Payload[0]))
                        .Roles
                        .Any(ur => ur.Role.RoleLevel == RoleLevel.ADMIN))
                    {
                        if (!user.IsSuperUser)
                        {
                            if (!appAdmins.Any(aa =>
                                aa.AppId == app.Id &&
                                aa.UserId == ((User)(result.Payload[0])).Id &&
                                aa.IsActive))
                            {
                                var adminRole = ((User)(result.Payload[0]))
                                    .Roles
                                    .FirstOrDefault(ur =>
                                        ur.Role.RoleLevel == RoleLevel.ADMIN);

                                ((User)(result.Payload[0])).Roles.Remove(adminRole);
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

                        if (!((User)requestorResponse.Object).IsSuperUser && request.RequestorId != id)
                        {
                            ((User)result.Payload[0]).NullifyEmail();
                        }
                    }
                    else
                    {
                        ((User)result.Payload[0]).NullifyEmail();
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
                    result.Message = UsersMessages.UserNotFoundMessage;

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

        public async Task<IResult> UpdateAsync(
            int id, 
            IRequest request, 
            string baseUrl, 
            string emailTemplatePath)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentNullException(nameof(emailTemplatePath));

            var result = new Result();
            
            var userResult = new UserResult();

            UpdateUserPayload payload;

            try
            {
                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserPayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserPayload)conversionResult;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.InvalidRequestMessage;

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
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
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
                try
                {
                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, id, request.License),
                        _cachingStrategy.Medium,
                        id);

                    var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
                        var user = (User)userResponse.Object;

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
                                else if (app.Environment == ReleaseEnvironment.QA)
                                {
                                    EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                        app.QaUrl,
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
                            else if (app.Environment == ReleaseEnvironment.QA)
                            {
                                url = app.QaUrl;
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

                        var updateUserResponse = await _cacheService.UpdateWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            request.License);

                        if (updateUserResponse.IsSuccess)
                        {
                            userResult.User = (User)updateUserResponse.Object;

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
                            
                            userResult.User.NullifyPassword();

                            result.IsSuccess = userResponse.IsSuccess;
                            result.Message = UsersMessages.UserUpdatedMessage;
                            result.Payload.Add(userResult);

                            return result;
                        }
                        else if (!updateUserResponse.IsSuccess && updateUserResponse.Exception != null)
                        {
                            result.IsSuccess = updateUserResponse.IsSuccess;
                            result.Message = updateUserResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.UserNotUpdatedMessage;

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
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotFoundMessage;

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
        }

        public async Task<IResult> GetUsersAsync(
            int requestorId, 
            string license, 
            IPaginator paginator)
        {
            if (paginator == null) throw new ArgumentNullException(nameof(paginator));

            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));

            var result = new Result();

            if (requestorId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                var response = await _usersRepository.GetAllAsync();

                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys.GetUsersCacheKey,
                    _cachingStrategy.Medium,
                    result);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    if (DataUtilities.IsPageValid(paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateUsers(paginator, response, result);
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
                            var emailConfirmed = user.IsEmailConfirmed;
                            user.NullifyEmail();
                            user.IsEmailConfirmed = emailConfirmed;
                        }
                    }
                    
                    // Nullify all passwords
                    foreach (var user in result.Payload.ConvertAll(u => (IUser)u))
                    {
                        user.NullifyPassword();
                    }

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

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (userResponse.IsSuccess)
                {
                    if (((User)userResponse.Object).Id == 1 && ((User)userResponse.Object).IsSuperUser)
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.SuperUserCannotBeDeletedMessage;

                        return result;
                    }

                    var deletionResponse = await _cacheService.DeleteWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        _cacheKeys,
                        (User)userResponse.Object);

                    if (deletionResponse.IsSuccess)
                    {
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
                    else if (!deletionResponse.IsSuccess && deletionResponse.Exception != null)
                    {
                        result.IsSuccess = deletionResponse.IsSuccess;
                        result.Message = deletionResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.UserNotDeletedMessage;

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
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

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

        public async Task<IResult> GetUserByPasswordTokenAsync(string token)
        {
            var result = new Result();

            try
            {
                var response = await _passwordResetsRepository.GetAsync(token);

                if (response.IsSuccess)
                {
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
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = UsersMessages.UserNotFoundMessage;

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

        public async Task<ILicenseResult> GetAppLicenseByPasswordTokenAsync(string token)
        {
            var result = new LicenseResult();

            try
            {
                var response = await _passwordResetsRepository.GetAsync(token);

                if (response.IsSuccess)
                {
                    result.License = await _appsRepository.GetLicenseAsync(((PasswordReset)response.Object).AppId);
                    result.IsSuccess = response.IsSuccess;
                    result.Message = AppsMessages.AppFoundMessage;

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
                    result.IsSuccess = response.IsSuccess;
                    result.Message = UsersMessages.NoOutstandingRequestToResetPassworMessage;

                    return result;
                }
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
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> AddUserRolesAsync(
            int userid,
            IRequest request, 
            string license)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));

            var result = new Result();

            UpdateUserRolePayload payload;

            try
            {
                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserRolePayload)conversionResult;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.InvalidRequestMessage;

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
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }

            if (userid == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                if (await _rolesRepository.IsListValidAsync(payload.RoleIds))
                {
                    if (payload.RoleIds.Contains(2) || payload.RoleIds.Contains(4))
                    {
                        result.IsSuccess = false;
                        result.Message = RolesMessages.RolesCannotBeAddedUsingThisEndpoint;

                        return result;
                    }

                    var response = await _usersRepository.AddRolesAsync(userid, payload.RoleIds);

                    var cacheServiceResponse = new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse(), 
                        new Result());

                    if (response.IsSuccess)
                    {
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
                    else if (!response.IsSuccess && response.Exception != null)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.RolesNotAddedMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.RolesInvalidMessage;

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

        public async Task<IResult> RemoveUserRolesAsync(
            int userid,
            IRequest request, 
            string license)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(license)) throw new ArgumentNullException(nameof(license));

            var result = new Result();

            UpdateUserRolePayload payload;

            try
            {
                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload conversionResult))
                {
                    payload = (UpdateUserRolePayload)conversionResult;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.InvalidRequestMessage;

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
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                return result;
            }

            if (userid == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
                if (await _rolesRepository.IsListValidAsync(payload.RoleIds))
                {
                    if (payload.RoleIds.Contains(2) || payload.RoleIds.Contains(4))
                    {
                        result.IsSuccess = false;
                        result.Message = RolesMessages.RolesCannotBeRemovedUsingThisEndpoint;

                        return result;
                    }

                    var response = await _usersRepository.RemoveRolesAsync(userid, payload.RoleIds);

                    var cacheServiceResponse = new Tuple<IRepositoryResponse, IResult>(
                        new RepositoryResponse(),
                        new Result());

                    if (response.IsSuccess)
                    {
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
                    else if (!response.IsSuccess && response.Exception != null)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = response.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.RolesNotRemovedMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.RolesInvalidMessage;

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

        public async Task<IResult> ActivateAsync(int id)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
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

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage;

                return result;
            }

            try
            {
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
            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentException(nameof(emailTemplatePath));

            var result = new Result();

            var userResult = new UserResult();

            if (userId == 0 || appId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.EmailConfirmationEmailNotResentMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, userId, license),
                    _cachingStrategy.Medium,
                    userId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (userResponse.IsSuccess)
                {
                    var user = (User)userResponse.Object;

                    if (!user.IsEmailConfirmed)
                    {
                        cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                            _appsRepository,
                            _distributedCache,
                            string.Format(_cacheKeys.GetAppCacheKey, appId),
                            _cachingStrategy.Medium,
                            appId);

                        var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                        if (appResponse.IsSuccess)
                        {
                            var app = (App)appResponse.Object;

                            if (await _emailConfirmationsRepository.HasOutstandingEmailConfirmationAsync(userId, appId))
                            {
                                var emailConfirmationResponse = await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(userId, appId);

                                if (emailConfirmationResponse.IsSuccess)
                                {
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
                                        else if (app.Environment == ReleaseEnvironment.QA)
                                        {
                                            EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                                app.QaUrl,
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
                                    else if (app.Environment == ReleaseEnvironment.QA)
                                    {
                                        url = app.QaUrl;
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

                                    if ((bool)userResult.ConfirmationEmailSuccessfullySent)
                                    {
                                        userResult.User = user;
                                        result.IsSuccess = true;
                                        result.Message = UsersMessages.EmailConfirmationEmailResentMessage;
                                        result.Payload.Add(userResult);

                                        return result;
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        result.Message = UsersMessages.EmailConfirmationEmailNotResentMessage;

                                        return result;
                                    }

                                }
                                else if (!emailConfirmationResponse.IsSuccess && emailConfirmationResponse.Exception != null)
                                {
                                    result.IsSuccess = emailConfirmationResponse.IsSuccess;
                                    result.Message = emailConfirmationResponse.Exception.Message;

                                    return result;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.Message = UsersMessages.EmailConfirmationRequestNotFoundMessage;

                                    return result;
                                }

                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = UsersMessages.EmailConfirmationRequestNotFoundMessage;

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
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmedMessage;

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
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentNullException(nameof(emailTemplatePath));

            var result = new Result();

            var confirmEmailResult = new ConfirmEmailResult();

            try
            {
                var emailConfirmationResponse = await _emailConfirmationsRepository.GetAsync(token);

                if (emailConfirmationResponse.IsSuccess)
                {
                    var emailConfirmation = (EmailConfirmation)emailConfirmationResponse.Object;

                    var license = (await _cacheService.GetLicenseWithCacheAsync(
                        _appsRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetAppLicenseCacheKey, emailConfirmation.AppId),
                        _cachingStrategy.Medium,
                        _cacheKeys,
                        emailConfirmation.AppId)).Item1;

                    if (!emailConfirmation.IsUpdate)
                    {
                        var response = await _cacheService.ConfirmEmailWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            emailConfirmation,
                            license);

                        if (response.IsSuccess)
                        {
                            var user = (User)response.Object;

                            result.IsSuccess = response.IsSuccess;
                            confirmEmailResult.UserName = user.UserName;
                            confirmEmailResult.Email = user.Email;
                            confirmEmailResult.DateUpdated = user.DateUpdated;
                            confirmEmailResult.NewEmailAddressConfirmed = true;
                            confirmEmailResult.IsUpdate = emailConfirmation.IsUpdate;

                            confirmEmailResult.AppTitle = user
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
                                confirmEmailResult.AppUrl = user
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
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.StagingUrl)
                                    .FirstOrDefault();
                            }
                            else if (user
                                .Apps
                                .Where(ua => ua.AppId == emailConfirmation.AppId)
                                .Select(ua => ua.App.Environment == ReleaseEnvironment.QA)
                                .FirstOrDefault())
                            {
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.QaUrl)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.ProdUrl)
                                    .FirstOrDefault();
                            }
                            
                            _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                            result.Message = UsersMessages.EmailConfirmedMessage;
                            result.Payload.Add(confirmEmailResult);

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
                            result.Message = UsersMessages.EmailNotConfirmedMessage;

                            return result;
                        }
                    }
                    else if (emailConfirmation.IsUpdate && !(bool)emailConfirmation.OldEmailAddressConfirmed)
                    {
                        var response = await _cacheService.UpdateEmailWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            emailConfirmation,
                            license);

                        var user = (User)response.Object;
                        var app = (App)(await _appsRepository.GetAsync(emailConfirmation.AppId)).Object;

                        if (response.IsSuccess)
                        {
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
                            else if (app.Environment == ReleaseEnvironment.QA)
                            {
                                url = app.QaUrl;
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

                            confirmEmailResult.ConfirmationEmailSuccessfullySent = await _emailService
                                .SendAsync(user.Email, emailSubject, html, app.Id);

                            emailConfirmation.OldEmailAddressConfirmed = true;

                            emailConfirmation = (EmailConfirmation)(await _emailConfirmationsRepository.UpdateAsync(emailConfirmation)).Object;

                            result.IsSuccess = response.IsSuccess;
                            result.Message = UsersMessages.OldEmailConfirmedMessage;
                            confirmEmailResult.UserName = user.UserName;
                            confirmEmailResult.Email = user.Email;
                            confirmEmailResult.DateUpdated = user.DateUpdated;
                            confirmEmailResult.IsUpdate = emailConfirmation.IsUpdate;
                            confirmEmailResult.AppTitle = appTitle;
                            confirmEmailResult.AppUrl = url;
                            result.Payload.Add(confirmEmailResult);

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
                            result.Message = UsersMessages.OldEmailNotConfirmedMessage;

                            return result;
                        }
                    }
                    else
                    {
                        var response = await _cacheService.ConfirmEmailWithCacheAsync(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            emailConfirmation,
                            license);

                        if (response.IsSuccess)
                        {
                            var user = (User)response.Object;

                            result.IsSuccess = response.IsSuccess;
                            confirmEmailResult.Email = user.Email;
                            confirmEmailResult.UserName = user.UserName;
                            confirmEmailResult.DateUpdated = user.DateUpdated;
                            confirmEmailResult.IsUpdate = emailConfirmation.IsUpdate;
                            confirmEmailResult.NewEmailAddressConfirmed = true;
                            confirmEmailResult.AppTitle = user
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
                                confirmEmailResult.AppUrl = user
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
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.StagingUrl)
                                    .FirstOrDefault();
                            }
                            else if (user
                                .Apps
                                .Where(ua => ua.AppId == emailConfirmation.AppId)
                                .Select(ua => ua.App.Environment == ReleaseEnvironment.QA)
                                .FirstOrDefault())
                            {
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.QaUrl)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                confirmEmailResult.AppUrl = user
                                    .Apps
                                    .Where(ua => ua.AppId == emailConfirmation.AppId)
                                    .Select(ua => ua.App.ProdUrl)
                                    .FirstOrDefault();
                            }

                            _ = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                            result.Message = UsersMessages.EmailConfirmedMessage;
                            result.Payload.Add(confirmEmailResult);

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
                            result.Message = UsersMessages.EmailNotConfirmedMessage;

                            return result;
                        }
                    }
                }
                else if (!emailConfirmationResponse.IsSuccess && emailConfirmationResponse.Exception != null)
                {
                    result.IsSuccess = emailConfirmationResponse.IsSuccess;
                    result.Message = emailConfirmationResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.EmailNotConfirmedMessage;

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

            var userResult = new UserResult();

            if (id == 0 || appId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.EmailConfirmationRequestNotCancelledMessage;

                return result;
            }

            try
            {
                var license = await _appsRepository.GetLicenseAsync(appId);

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (userResponse.IsSuccess)
                {
                    if (await _emailConfirmationsRepository.HasOutstandingEmailConfirmationAsync(id, appId))
                    {
                        var user = (User)userResponse.Object;

                        var emailConfirmation = (EmailConfirmation)
                            (await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(id, appId))
                            .Object;

                        var response = await _emailConfirmationsRepository.DeleteAsync(emailConfirmation);

                        if (response.IsSuccess)
                        {
                            // Role back email request
                            user.Email = emailConfirmation.OldEmailAddress;
                            user.ReceivedRequestToUpdateEmail = false;
                            user.IsEmailConfirmed = true;

                            userResult.User = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                                _usersRepository,
                                _distributedCache,
                                _cacheKeys,
                                user,
                                license)).Object;

                            userResult.User.NullifyPassword();

                            result.IsSuccess = response.IsSuccess;
                            result.Message = UsersMessages.EmailConfirmationRequestCancelledMessage;
                            result.Payload.Add(userResult);

                            return result;
                        }
                        else if (response.IsSuccess == false && response.Exception != null)
                        {
                            userResult.User = (User)(await _usersRepository.UpdateAsync(user)).Object;
                            result.IsSuccess = response.IsSuccess;
                            result.Message = response.Exception.Message;
                            result.Payload.Add(userResult);

                            return result;
                        }
                        else
                        {
                            userResult.User = (User)(await _usersRepository.UpdateAsync(user)).Object;
                            result.IsSuccess = false;
                            result.Message = UsersMessages.EmailConfirmationRequestNotCancelledMessage;
                            result.Payload.Add(userResult);

                            return result;
                        }
                    }
                    else
                    {
                        userResult.User = (User)(await _usersRepository.GetAsync(id)).Object;
                        result.IsSuccess = false;
                        result.Message = UsersMessages.EmailConfirmationRequestNotFoundMessage;
                        result.Payload.Add(userResult);

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
            var userResult = new UserResult();

            if (id == 0 || appId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.UserNotFoundMessage + " or " + AppsMessages.AppNotFoundMessage;

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

                if (userResponse.IsSuccess)
                {
                    if (await _appsRepository.HasEntityAsync(appId))
                    {
                        var emailConfirmationResponse = await _emailConfirmationsRepository.RetrieveEmailConfirmationAsync(id, appId);
                        var passwordResetResponse = await _passwordResetsRepository.RetrievePasswordResetAsync(id, appId);
                        var user = (User)userResponse.Object;

                        if (emailConfirmationResponse.IsSuccess || passwordResetResponse.IsSuccess)
                        {
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
                                        string.Format("{0} and {1}", result.Message, UsersMessages.PasswordResetRequestCancelledMessage);
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

                            userResult.User = user;
                            result.Payload.Add(userResult);
                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.EmailRequestsNotFoundMessage;

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
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

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

        public async Task<IResult> InitiatePasswordResetAsync(string token, string license)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            var result = new Result();

            var initiatePasswordResetResult = new InitiatePasswordResetResult();

            try
            {
                var passwordResetResponse = await _passwordResetsRepository.GetAsync(token);

                if (passwordResetResponse.IsSuccess)
                {
                    var passwordReset = (PasswordReset)passwordResetResponse.Object;

                    var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, passwordReset.UserId, license),
                        _cachingStrategy.Medium,
                        passwordReset.UserId);

                    var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
                        var user = (User)userResponse.Object;

                        cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                            _appsRepository,
                            _distributedCache,
                            string.Format(_cacheKeys.GetAppCacheKey, passwordReset.AppId),
                            _cachingStrategy.Medium,
                            passwordReset.AppId);

                        var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                        if (appResponse.IsSuccess)
                        {
                            var app = (App)appResponse.Object;

                            user.NullifyPassword();

                            if (user.Apps.Any(ua => ua.AppId == app.Id))
                            {
                                if (user.ReceivedRequestToUpdatePassword)
                                {
                                    result.IsSuccess = true;
                                    result.Message = UsersMessages.UserFoundMessage;
                                    initiatePasswordResetResult.User = user;
                                    initiatePasswordResetResult.App = app;
                                    result.Payload.Add(initiatePasswordResetResult);

                                    return result;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.Message = UsersMessages.NoOutstandingRequestToResetPassworMessage;

                                    return result;
                                }
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = AppsMessages.UserNotSignedUpToAppMessage;

                                return result;
                            }
                        }
                        else if (!appResponse.IsSuccess && appResponse.Exception != null)
                        {
                            result.IsSuccess = passwordResetResponse.IsSuccess;
                            result.Message = passwordResetResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = appResponse.IsSuccess;
                            result.Message = AppsMessages.AppNotFoundMessage;

                            return result;
                        }
                    }
                    else if (!userResponse.IsSuccess && userResponse.Exception != null)
                    {
                        result.IsSuccess = passwordResetResponse.IsSuccess;
                        result.Message = passwordResetResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = userResponse.IsSuccess;
                        result.Message = UsersMessages.UserNotFoundMessage;

                        return result;
                    }
                }
                else if (!passwordResetResponse.IsSuccess && passwordResetResponse.Exception != null)
                {
                    result.IsSuccess = passwordResetResponse.IsSuccess;
                    result.Message = passwordResetResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = passwordResetResponse.IsSuccess;
                    result.Message = UsersMessages.PasswordResetRequestNotFoundMessage;

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

        public async Task<IResult> ResendPasswordResetAsync(
            int userId, 
            int appId, 
            string baseUrl, 
            string emailTemplatePath)
        {
            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentException(nameof(emailTemplatePath));

            var result = new Result();

            if (userId == 0 || appId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.PasswordResetRequestNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<App>(
                    _appsRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetAppCacheKey, appId),
                    _cachingStrategy.Medium,
                    appId);

                var appResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (appResponse.IsSuccess)
                {
                    var app = (App)((await _appsRepository.GetAsync(appId)).Object);
                    app.License = await _appsRepository.GetLicenseAsync(app.Id);

                    cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserCacheKey, userId, app.License),
                        _cachingStrategy.Medium,
                        userId);

                    var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
                        var user = (User)userResponse.Object;

                        if (user.ReceivedRequestToUpdatePassword)
                        {
                            if (await _passwordResetsRepository.HasOutstandingPasswordResetAsync(userId, appId))
                            {
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
                                    else if (app.Environment == ReleaseEnvironment.QA)
                                    {
                                        EmailConfirmationAction = string.Format("{0}/{1}/{2}",
                                            app.QaUrl,
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
                                else if (app.Environment == ReleaseEnvironment.QA)
                                {
                                    url = app.QaUrl;
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

                                if (result.IsSuccess)
                                {
                                    result.Message = UsersMessages.PasswordResetEmailResentMessage;

                                    return result;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.Message = UsersMessages.PasswordResetEmailNotResentMessage;

                                    return result;
                                }
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.Message = UsersMessages.NoOutstandingRequestToResetPassworMessage;

                                return result;
                            }
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.NoOutstandingRequestToResetPassworMessage;

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
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            if (string.IsNullOrEmpty(emailTemplatePath)) throw new ArgumentNullException(nameof(emailTemplatePath));

            var result = new Result();

            try
            {
                var cacheServiceResponse = await _cacheService.GetAppByLicenseWithCacheAsync(
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

                if (appResponse.IsSuccess)
                {
                    cacheServiceResponse = await _cacheService.GetByEmailWithCacheAsync(
                        _usersRepository,
                        _distributedCache,
                        string.Format(_cacheKeys.GetUserByEmailCacheKey, request.Email, app.License),
                        _cachingStrategy.Medium,
                        request.Email);

                    var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                    if (userResponse.IsSuccess)
                    {
                        var user = (User)userResponse.Object;
                        PasswordReset passwordReset;

                        if (user.Apps.Any(ua => ua.AppId == app.Id))
                        {
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

                                if (passwordResetResponse.IsSuccess)
                                {
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
                                else if (!passwordResetResponse.IsSuccess && passwordResetResponse.Exception != null)
                                {
                                    result.IsSuccess = passwordResetResponse.IsSuccess;
                                    result.Message = passwordResetResponse.Exception.Message;

                                    return result;
                                }
                                else
                                {
                                    result.IsSuccess = userResponse.IsSuccess;
                                    result.Message = UsersMessages.UnableToProcessPasswordResetRequesMessage;

                                    return result;
                                }
                            }
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = AppsMessages.UserNotSignedUpToAppMessage;

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
                        result.Message = UsersMessages.NoUserIsUsingThisEmailMessage;

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
                return DataUtilities.ProcessException<UsersService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> UpdatePasswordAsync(IUpdatePasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            var salt = BCrypt.Net.BCrypt.GenerateSalt();

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, request.UserId, request.License),
                    _cachingStrategy.Medium,
                    request.UserId);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;
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


                if (userResponse.IsSuccess)
                {
                    if (user.ReceivedRequestToUpdatePassword)
                    {
                        user.Password = BCrypt.Net.BCrypt
                                .HashPassword(request.NewPassword, salt);

                        user.DateUpdated = DateTime.UtcNow;

                        user.ReceivedRequestToUpdatePassword = false;

                        var updateUserResponse = await _cacheService.UpdateWithCacheAsync<User>(
                            _usersRepository,
                            _distributedCache,
                            _cacheKeys,
                            user,
                            app.License);

                        if (updateUserResponse.IsSuccess)
                        {
                            var passwordResetReponse = await _passwordResetsRepository
                                .RetrievePasswordResetAsync(
                                    user.Id, 
                                    app.Id);

                            _ = await _passwordResetsRepository
                                .DeleteAsync((PasswordReset)passwordResetReponse.Object);

                            user = (User)updateUserResponse.Object;

                            user.NullifyPassword();

                            result.IsSuccess = userResponse.IsSuccess;
                            result.Message = UsersMessages.PasswordResetMessage;
                            result.Payload.Add(user);

                            return result;
                        }
                        else if (!updateUserResponse.IsSuccess && updateUserResponse.Exception != null)
                        {
                            result.IsSuccess = userResponse.IsSuccess;
                            result.Message = userResponse.Exception.Message;

                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = UsersMessages.PasswordNotResetMessage;

                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = UsersMessages.NoOutstandingRequestToResetPassworMessage;

                        return result;
                    }
                }
                else if (!userResponse.IsSuccess && userResponse.Exception != null)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserNotFoundMessage;

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

        public async Task<IResult> CancelPasswordResetRequestAsync(int id, int appId)
        {
            var result = new Result();

            var userResult = new UserResult();

            if (id == 0 || appId == 0)
            {
                result.IsSuccess = false;
                result.Message = UsersMessages.PasswordResetRequestNotCancelledMessage;

                return result;
            }

            try
            {
                var license = await _appsRepository.GetLicenseAsync(appId);

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<User>(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserCacheKey, id, license),
                    _cachingStrategy.Medium,
                    id);

                var userResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                if (userResponse.IsSuccess)
                {
                    if (await _passwordResetsRepository.HasOutstandingPasswordResetAsync(id, appId))
                    {
                        var user = (User)userResponse.Object;

                        var passwordReset = (PasswordReset)
                            (await _passwordResetsRepository.RetrievePasswordResetAsync(id, appId))
                            .Object;

                        var response = await _passwordResetsRepository.DeleteAsync(passwordReset);

                        if (response.IsSuccess)
                        {
                            // Role back password reset
                            user.ReceivedRequestToUpdatePassword = false;

                            userResult.User = (User)(await _cacheService.UpdateWithCacheAsync<User>(
                                _usersRepository,
                                _distributedCache,
                                _cacheKeys,
                                user,
                                license)).Object;
                            
                            userResult.User.NullifyPassword();

                            result.IsSuccess = response.IsSuccess;
                            result.Message = UsersMessages.PasswordResetRequestCancelledMessage;
                            result.Payload.Add(userResult);

                            return result;
                        }
                        else if (response.IsSuccess == false && response.Exception != null)
                        {
                            userResult.User = (User)(await _usersRepository.UpdateAsync(user)).Object;
                            result.IsSuccess = response.IsSuccess;
                            result.Message = response.Exception.Message;
                            result.Payload.Add(userResult);

                            return result;
                        }
                        else
                        {
                            userResult.User = (User)(await _usersRepository.UpdateAsync(user)).Object;
                            result.IsSuccess = false;
                            result.Message = UsersMessages.PasswordResetRequestNotCancelledMessage;
                            result.Payload.Add(userResult);

                            return result;
                        }
                    }
                    else
                    {
                        userResult.User = (User)(await _usersRepository.GetAsync(id)).Object;
                        result.IsSuccess = false;
                        result.Message = UsersMessages.PasswordResetRequestNotFoundMessage;
                            result.Payload.Add(userResult);

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
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }

        private async Task<PasswordReset> EnsurePasswordResetTokenIsUnique(PasswordReset passwordReset)
        {
            try
            {
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
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

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
                    else if (app.Environment == ReleaseEnvironment.QA)
                    {
                        emailUrl = app.QaUrl;
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
                else if (app.Environment == ReleaseEnvironment.QA)
                {
                    url = app.QaUrl;
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

                if (result.IsSuccess)
                {
                    if (newRequest)
                    {
                        result.Message = UsersMessages.ProcessedPasswordResetRequestMessage;
                    }
                    else
                    {
                        result.Message = UsersMessages.ResentPasswordResetRequestMessage;
                    }

                    return result;
                }
                else
                {
                    result.Message = UsersMessages.UnableToProcessPasswordResetRequesMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UsersService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }
        #endregion
    }
}
