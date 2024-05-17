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
    public class UserManagementService(
        IUsersRepository<User> usersRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<UserManagementService> logger) : IUserManagementService
    {
        #region Fields
        private readonly IUsersRepository<User> _usersRepository = usersRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<UserManagementService> _logger = logger;
        #endregion

        #region Methods
        public async Task<bool> IsValidUserAsync(string username, string password)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(nameof(username));

                ArgumentException.ThrowIfNullOrEmpty(nameof(password));

                var userResponse = await _usersRepository.GetByUserNameAsync(username);

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    return false;
                }
                #endregion

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
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(nameof(username));

                ArgumentException.ThrowIfNullOrEmpty(nameof(password));

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
            try
            {
                ArgumentNullException.ThrowIfNull(nameof(email));

                ArgumentNullException.ThrowIfNull(nameof(license));

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

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.NoUserIsUsingThisEmailMessage;

                    return result;
                }
                #endregion

                result = (Result)cachFactoryResponse.Item2;

                result.IsSuccess = true;
                result.Message = UsersMessages.UserNameConfirmedMessage;

                authenticatedUserNameResult.UserName = ((User)userResponse.Object).UserName;

                result.Payload.Add(authenticatedUserNameResult);

                return result;
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
