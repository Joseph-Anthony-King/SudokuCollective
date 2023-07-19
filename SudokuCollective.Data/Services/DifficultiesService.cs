using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.Cache;
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
    public class DifficultiesService : IDifficultiesService
    {
        #region Fields
        private readonly IDifficultiesRepository<Difficulty> _difficultiesRepository;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;
        private readonly ILogger<DifficultiesService> _logger;
        #endregion

        #region Constructor
        public DifficultiesService(
            IDifficultiesRepository<Difficulty> difficultiesRepository,
            IRequestService requestService,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            ICachingStrategy cachingStrategy,
            ILogger<DifficultiesService> logger)
        {
            _difficultiesRepository = difficultiesRepository;
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

            CreateDifficultyPayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(CreateDifficultyPayload), out IPayload conversionResult))
            {
                payload = (CreateDifficultyPayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
            }
            
            if (string.IsNullOrEmpty(payload.Name)) 
                throw new ArgumentNullException(nameof(payload.DifficultyLevel));

            if (string.IsNullOrEmpty(payload.DisplayName)) 
                throw new ArgumentNullException(nameof(payload.DisplayName));

            try
            {
                if (!await _cacheService.HasDifficultyLevelWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetDifficultyCacheKey, payload.DifficultyLevel),
                    _cachingStrategy.Heavy,
                    payload.DifficultyLevel))
                {

                    var difficulty = new Difficulty()
                    {
                        Name = payload.Name,
                        DisplayName = payload.DisplayName,
                        DifficultyLevel = payload.DifficultyLevel
                    };

                    var response = await _cacheService.AddWithCacheAsync<Difficulty>(
                        _difficultiesRepository,
                        _distributedCache,
                        _cacheKeys.GetDifficultyCacheKey,
                        _cachingStrategy.Heavy,
                        _cacheKeys,
                        difficulty);

                    if (response.IsSuccess)
                    {
                        result.IsSuccess = response.IsSuccess;
                        result.Message = DifficultiesMessages.DifficultyCreatedMessage;
                        result.Payload.Add(response.Object);

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
                        result.Message = DifficultiesMessages.DifficultyNotCreatedMessage;

                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = DifficultiesMessages.DifficultyAlreadyExistsMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<DifficultiesService>(
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
                result.Message = DifficultiesMessages.DifficultiesNotFoundMessage;

                return result;
            }

            try
            {
                if (id == 1 || id == 2)
                {
                    result.IsSuccess = false;
                    result.Message = DifficultiesMessages.NullAndTestDifficultiesAreNotAvailableThroughTheApi;

                    return result;
                }

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetDifficultyCacheKey, id),
                    _cachingStrategy.Heavy,
                    id,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = DifficultiesMessages.DifficultyFoundMessage;
                    result.Payload.Add(response.Object);

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
                    result.Message = DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<DifficultiesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetDifficultiesAsync()
        {
            var result = new Result();

            try
            {
                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    _cacheKeys.GetDifficultiesCacheKey,
                    _cachingStrategy.Heavy,
                    result);

                var response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = DifficultiesMessages.DifficultiesFoundMessage;

                    result.Payload.AddRange(response.Objects);


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
                    result.Message = DifficultiesMessages.DifficultiesNotFoundMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<DifficultiesService>(
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

            UpdateDifficultyPayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateDifficultyPayload), out IPayload conversionResult))
            {
                payload = (UpdateDifficultyPayload)conversionResult;
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
                result.Message = DifficultiesMessages.DifficultiesNotFoundMessage;

                return result;
            }

            try
            {
                var response = await _difficultiesRepository.GetAsync(id);

                if (response.IsSuccess)
                {
                    var difficulty = (Difficulty)response.Object;

                    difficulty.Name = payload.Name;
                    difficulty.DisplayName = payload.DisplayName;

                    var updateDifficultyResponse = await _cacheService.UpdateWithCacheAsync(
                        _difficultiesRepository,
                        _distributedCache,
                        _cacheKeys,
                        difficulty);

                    if (updateDifficultyResponse.IsSuccess)
                    {
                        result.IsSuccess = updateDifficultyResponse.IsSuccess;
                        result.Message = DifficultiesMessages.DifficultyUpdatedMessage;
                        result.Payload.Add(updateDifficultyResponse.Object);

                        return result;
                    }
                    else if (!updateDifficultyResponse.IsSuccess && updateDifficultyResponse.Exception != null)
                    {
                        result.IsSuccess = updateDifficultyResponse.IsSuccess;
                        result.Message = updateDifficultyResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = DifficultiesMessages.DifficultyNotUpdatedMessage;

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
                    result.Message = DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<DifficultiesService>(
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
                result.Message = DifficultiesMessages.DifficultiesNotFoundMessage;

                return result;
            }

            try
            {
                var response = await _difficultiesRepository.GetAsync(id);

                if (response.IsSuccess)
                {
                    var updateDeleteResponse = await _cacheService.DeleteWithCacheAsync(
                        _difficultiesRepository,
                        _distributedCache,
                        _cacheKeys,
                        (Difficulty)response.Object);

                    if (updateDeleteResponse.IsSuccess)
                    {
                        result.IsSuccess = updateDeleteResponse.IsSuccess;
                        result.Message = DifficultiesMessages.DifficultyDeletedMessage;

                        return result;
                    }
                    else if (!updateDeleteResponse.IsSuccess && updateDeleteResponse.Exception != null)
                    {
                        result.IsSuccess = updateDeleteResponse.IsSuccess;
                        result.Message = updateDeleteResponse.Exception.Message;

                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = DifficultiesMessages.DifficultyNotDeletedMessage;

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
                    result.Message = DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<DifficultiesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }
        #endregion
    }
}
