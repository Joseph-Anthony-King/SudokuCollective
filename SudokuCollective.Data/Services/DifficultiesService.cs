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
    public class DifficultiesService(
        IDifficultiesRepository<Difficulty> difficultiesRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<DifficultiesService> logger) : IDifficultiesService
    {
        #region Fields
        private readonly IDifficultiesRepository<Difficulty> _difficultiesRepository = difficultiesRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<DifficultiesService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> CreateAsync(IRequest request)
        {
            var result = new Result();

            try
            {

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                CreateDifficultyPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(CreateDifficultyPayload), out IPayload conversionResult))
                {
                    payload = (CreateDifficultyPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                ArgumentException.ThrowIfNullOrEmpty(payload.Name, nameof(payload.Name));

                ArgumentException.ThrowIfNullOrEmpty(payload.DisplayName, nameof(payload.DisplayName));

                ArgumentNullException.ThrowIfNull(payload.DifficultyLevel, nameof(payload.DifficultyLevel));

                var hasDifficulty = !await _cacheService.HasDifficultyLevelWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetDifficultyCacheKey, payload.DifficultyLevel),
                    _cachingStrategy.Heavy,
                    payload.DifficultyLevel);

                if (!hasDifficulty)
                {
                    result.IsSuccess = false;
                    result.Message = DifficultiesMessages.DifficultyAlreadyExistsMessage;

                    return result;
                }

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

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        DifficultiesMessages.DifficultyNotCreatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = response.IsSuccess;
                result.Message = DifficultiesMessages.DifficultyCreatedMessage;
                result.Payload.Add(response.Object);

                return result;
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

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id)); 

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                if (id == 1 || id == 2)
                {
                    throw new ArgumentException(DifficultiesMessages.NullAndTestDifficultiesAreNotAvailableThroughTheApi);
                }

                var cacheServiceResponse = await _cacheService.GetWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    string.Format(_cacheKeys.GetDifficultyCacheKey, id),
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
                        DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                result.IsSuccess = response.IsSuccess;
                result.Message = DifficultiesMessages.DifficultyFoundMessage;
                result.Payload.Add(response.Object);

                return result;
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

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        DifficultiesMessages.DifficultiesNotFoundMessage;

                    return result;
                }
                #endregion

                result = (Result)cacheServiceResponse.Item2;

                result.IsSuccess = response.IsSuccess;
                result.Message = DifficultiesMessages.DifficultiesFoundMessage;

                result.Payload.AddRange(response.Objects);


                return result;
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
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                UpdateDifficultyPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(UpdateDifficultyPayload), out IPayload conversionResult))
                {
                    payload = (UpdateDifficultyPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var getResponse = await _difficultiesRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
                #endregion

                var difficulty = (Difficulty)getResponse.Object;

                difficulty.Name = payload.Name;
                difficulty.DisplayName = payload.DisplayName;

                var updateResponse = await _cacheService.UpdateWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    _cacheKeys,
                    difficulty);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        DifficultiesMessages.DifficultyNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Message = DifficultiesMessages.DifficultyUpdatedMessage;
                result.Payload.Add(updateResponse.Object);

                return result;
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

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var getResponse = await _difficultiesRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }
                #endregion

                var updateResponse = await _cacheService.DeleteWithCacheAsync(
                    _difficultiesRepository,
                    _distributedCache,
                    _cacheKeys,
                    (Difficulty)getResponse.Object);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        DifficultiesMessages.DifficultyNotDeletedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Message = DifficultiesMessages.DifficultyDeletedMessage;

                return result;
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
