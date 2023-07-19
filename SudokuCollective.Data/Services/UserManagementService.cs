using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Data.Services
{
    public class UserManagementService : IUserManagementService
    {
        #region Fields
        private readonly IUsersRepository<User> _usersRepository;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly ILogger<UserManagementService> _logger;
        #endregion

        #region Constructors
        public UserManagementService(
            IUsersRepository<User> usersRepository,
            IRequestService requestService,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            ICachingStrategy cachingStrategy,
            ILogger<UserManagementService> logger)
        {
            _usersRepository = usersRepository;
            _requestService = requestService;
            _distributedCache = distributedCache;
            _cacheService = cacheService;
            _cacheKeys = cacheKeys;
            _cachingStrategy = cachingStrategy;
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<bool> IsValidUserAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            try
            {
                var userResponse = await _usersRepository.GetByUserNameAsync(username);

                if (userResponse.IsSuccess)
                {
                    if ((IUser)userResponse.Object != null
                        && BCrypt.Net.BCrypt.Verify(password, ((IUser)userResponse.Object).Password))
                    {
                        return true;
                    }
                    else
                    {
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
                SudokuCollectiveLogger.LogError<UserManagementService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }

        public async Task<UserAuthenticationErrorType> ConfirmAuthenticationIssueAsync(string username, string password, string license)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            try
            {
                var cachFactoryResponse = await _cacheService.GetByUserNameWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserByUsernameCacheKey, username, license),
                    _cachingStrategy.Medium,
                    _cacheKeys,
                    username,
                    license);

                var userResponse = (RepositoryResponse)cachFactoryResponse.Item1;

                if (userResponse.IsSuccess)
                {
                    if (!BCrypt.Net.BCrypt.Verify(password, ((IUser)userResponse.Object).Password))
                    {
                        return UserAuthenticationErrorType.PASSWORDINVALID;
                    }
                    else
                    {
                        return UserAuthenticationErrorType.NULL;
                    }
                }
                else if (!userResponse.IsSuccess && userResponse.Object == null)
                {
                    return UserAuthenticationErrorType.USERNAMEINVALID;
                }
                else
                {
                    return UserAuthenticationErrorType.NULL;
                }
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UserManagementService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }

        public async Task<IResult> ConfirmUserNameAsync(string email, string license)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

            try
            {
                var result = new Result();
                var authenticatedUserNameResult = new AuthenticatedUserNameResult();

                var cachFactoryResponse = await _cacheService.GetByEmailWithCacheAsync(
                    _usersRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetUserByUsernameCacheKey, email, license),
                    _cachingStrategy.Medium,
                    email,
                    result);

                var userResponse = (RepositoryResponse)cachFactoryResponse.Item1;
                result = (Result)cachFactoryResponse.Item2;

                if (userResponse.IsSuccess)
                {
                    authenticatedUserNameResult.UserName = ((User)userResponse.Object).UserName;
                    result.IsSuccess = true;
                    result.Message = UsersMessages.UserNameConfirmedMessage;
                    result.Payload.Add(authenticatedUserNameResult);

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoUserIsUsingThisEmailMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<UserManagementService>(
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
