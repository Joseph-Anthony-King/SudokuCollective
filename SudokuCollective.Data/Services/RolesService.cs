using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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
using SudokuCollective.Data.Utilities;

namespace SudokuCollective.Data.Services
{
    public class RolesService(
        IRolesRepository<Role> rolesRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<RolesService> logger) : IRolesService
    {
        #region Fields
        private readonly IRolesRepository<Role> _rolesRepository = rolesRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<RolesService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> CreateAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                CreateRolePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(CreateRolePayload), out IPayload conversionResult))
                {
                    payload = (CreateRolePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                ArgumentException.ThrowIfNullOrEmpty(payload.Name, nameof(payload.Name));

                var hasRole = !await _cacheService.HasRoleLevelWithCacheAsync(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, payload.RoleLevel),
                    _cachingStrategy.Heavy,
                    payload.RoleLevel);

                if(!hasRole)
                {
                    result.IsSuccess = false;
                    result.Message = RolesMessages.RoleAlreadyExistsMessage;

                    return result;
                }

                var role = new Role()
                {
                    Name = payload.Name,
                    RoleLevel = payload.RoleLevel
                };

                var response = await _cacheService.AddWithCacheAsync(
                    _rolesRepository,
                    _distributedCache,
                    _cacheKeys.GetRoleCacheKey,
                    _cachingStrategy.Heavy,
                    _cacheKeys,
                    role);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        RolesMessages.RoleNotCreatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = response.IsSuccess;
                result.Message = RolesMessages.RoleCreatedMessage;
                result.Payload.Add((IRole)response.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<RolesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<Role>(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, id),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        RolesMessages.RoleNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var role = (Role)response.Object;

                result.IsSuccess = response.IsSuccess;
                result.Message = RolesMessages.RoleFoundMessage;
                result.Payload.Add(role);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<RolesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetRolesAsync()
        {
            var result = new Result();

            try
            {
                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync<Role>(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRolesCacheKey),
                    _cachingStrategy.Heavy,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        RolesMessages.RolesNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var roles = response.Objects.ConvertAll(r => (IRole)r);

                result.IsSuccess = response.IsSuccess;
                result.Message = RolesMessages.RolesFoundMessage;
                result.Payload.AddRange(roles);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<RolesService>(
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
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                UpdateRolePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateRolePayload), out IPayload conversionResult))
                {
                    payload = (UpdateRolePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<Role>(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, id),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var roleResponse = (RepositoryResponse)cacheServiceResponse.Item1;

                #region roleResponse fails
                if (!roleResponse.IsSuccess)
                {
                    result.IsSuccess = roleResponse.IsSuccess;
                    result.Message = roleResponse.Exception != null ? 
                        roleResponse.Exception.Message : 
                        RolesMessages.RoleNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                var role = (Role)roleResponse.Object;

                role.Name = payload.Name;

                var updateResponse = await _cacheService.UpdateWithCacheAsync(
                    _rolesRepository,
                    _distributedCache,
                    _cacheKeys,
                    role);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        RolesMessages.RoleNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Message = RolesMessages.RoleUpdatedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<RolesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeleteAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var roleResponse = await _rolesRepository.GetAsync(id);

                #region roleResponse fails
                if (!roleResponse.IsSuccess)
                {
                    result.IsSuccess = roleResponse.IsSuccess;
                    result.Message = roleResponse.Exception != null ?
                        roleResponse.Exception.Message :
                        RolesMessages.RoleNotFoundMessage;

                    return result;
                }
                #endregion

                var deleteResponse = await _cacheService.DeleteWithCacheAsync(
                    _rolesRepository,
                    _distributedCache,
                    _cacheKeys,
                    (Role)roleResponse.Object);

                #region deleteResponse
                if (!deleteResponse.IsSuccess)
                {
                    result.IsSuccess = deleteResponse.IsSuccess;
                    result.Message = deleteResponse.Exception != null ? 
                        deleteResponse.Exception.Message : 
                        RolesMessages.RoleNotDeletedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = deleteResponse.IsSuccess;
                result.Message = RolesMessages.RoleDeletedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<RolesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }
        #endregion
    }
}
