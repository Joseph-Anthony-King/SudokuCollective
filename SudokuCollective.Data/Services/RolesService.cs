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
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Data.Services
{
    public class RolesService : IRolesService
    {
        #region Fields
        private readonly IRolesRepository<Role> _rolesRepository;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly ILogger<RolesService> _logger;
        #endregion

        #region Constructor
        public RolesService(
            IRolesRepository<Role> rolesRepository,
            IRequestService requestService,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            ICachingStrategy cachingStrategy,
            ILogger<RolesService> logger)
        {
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
        public async Task<IResult> CreateAsync(IRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            CreateRolePayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(CreateRolePayload), out IPayload conversionResult))
            {
                payload = (CreateRolePayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
            }

            if (string.IsNullOrEmpty(payload.Name)) throw new ArgumentNullException(nameof(payload.Name));

            try
            {
                if (!await _cacheService.HasRoleLevelWithCacheAsync(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, payload.RoleLevel),
                    _cachingStrategy.Heavy,
                    payload.RoleLevel))
                {
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

                    if (response.IsSuccess)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = RolesMessages.RoleCreatedMessage;
                        result.Payload.Add((IRole)response.Object);

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
                        result.Message = RolesMessages.RoleNotCreatedMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = RolesMessages.RoleAlreadyExistsMessage;

                    return result;
                }
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

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = RolesMessages.RoleNotFoundMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<Role>(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, id),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    var role = (Role)response.Object;

                    result.IsSuccess = response.IsSuccess;
                    result.Message = RolesMessages.RoleFoundMessage;
                    result.Payload.Add(role);

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
                    result.Message = RolesMessages.RoleNotFoundMessage;

                    return result;
                }
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
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    var roles = response.Objects.ConvertAll(r => (IRole)r);

                    result.IsSuccess = response.IsSuccess;
                    result.Message = RolesMessages.RolesFoundMessage;
                    result.Payload.AddRange(roles);

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
                    result.Message = RolesMessages.RolesNotFoundMessage;

                    return result;
                }
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = RolesMessages.RoleNotFoundMessage;

                return result;
            }

            UpdateRolePayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateRolePayload), out IPayload conversionResult))
            {
                payload = (UpdateRolePayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
            }

            try
            {
                var cacheServiceResponse = await _cacheService.GetWithCacheAsync<Role>(
                    _rolesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetRoleCacheKey, id),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    var role = (Role)response.Object;

                    role.Name = payload.Name;

                    var updateResponse = await _cacheService.UpdateWithCacheAsync(
                        _rolesRepository,
                        _distributedCache,
                        _cacheKeys,
                        role);

                    if (updateResponse.IsSuccess)
                    {
                        result.IsSuccess = updateResponse.IsSuccess;
                        result.Message = RolesMessages.RoleUpdatedMessage;

                        return result;
                    }
                    else if (!updateResponse.IsSuccess && updateResponse.Exception != null)
                    {
                        result.IsSuccess = updateResponse.IsSuccess;
                        result.Message = updateResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = RolesMessages.RoleNotUpdatedMessage;

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
                    result.Message = RolesMessages.RoleNotFoundMessage;

                    return result;
                }
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

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = RolesMessages.RoleNotFoundMessage;

                return result;
            }

            try
            {
                var response = await _rolesRepository.GetAsync(id);

                if (response.IsSuccess)
                {
                    var deleteResponse = await _cacheService.DeleteWithCacheAsync(
                        _rolesRepository,
                        _distributedCache,
                        _cacheKeys,
                        (Role)response.Object);

                    if (deleteResponse.IsSuccess)
                    {
                        result.IsSuccess = deleteResponse.IsSuccess;
                        result.Message = RolesMessages.RoleDeletedMessage;

                        return result;
                    }
                    else if (!deleteResponse.IsSuccess && deleteResponse.Exception != null)
                    {
                        result.IsSuccess = deleteResponse.IsSuccess;
                        result.Message = deleteResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = RolesMessages.RoleNotDeletedMessage;

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
                    result.Message = RolesMessages.RoleNotFoundMessage;

                    return result;
                }
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
