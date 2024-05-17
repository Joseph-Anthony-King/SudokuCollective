using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Hangfire;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Jobs;
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
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Data.Services
{
    public class SolutionsService(
        ISolutionsRepository<SudokuSolution> solutionsRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        IBackgroundJobClient jobClient,
        IDataJobs dataJobs,
        ILogger<SolutionsService> logger) : ISolutionsService
    {
        #region Fields
        private readonly ISolutionsRepository<SudokuSolution> _solutionsRepository = solutionsRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly IBackgroundJobClient _jobClient = jobClient;
        private readonly IDataJobs _dataJobs = dataJobs;
        private readonly ILogger<SolutionsService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> GetAsync(int id)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                var solutionResponse = await _solutionsRepository.GetAsync(id);

                if (!solutionResponse.IsSuccess)
                {
                    result.IsSuccess = solutionResponse.IsSuccess;
                    result.Message = solutionResponse.Exception != null ? 
                        solutionResponse.Exception.Message : 
                        SolutionsMessages.SolutionNotFoundMessage;

                    return result;
                }

                var solution = (SudokuSolution)solutionResponse.Object;

                result.IsSuccess = solutionResponse.IsSuccess;
                result.Message = SolutionsMessages.SolutionFoundMessage;
                result.Payload.Add(solution);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<SolutionsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetSolutionsAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var response = new RepositoryResponse();

                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync<SudokuSolution>(
                    _solutionsRepository,
                    _distributedCache,
                    _cacheKeys.GetSolutionsCacheKey,
                    DateTime.Now.AddHours(1),
                    result);

                response = (RepositoryResponse)cacheServiceResponse.Item1;

                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        SolutionsMessages.SolutionsNotFoundMessage;

                    return result;
                }

                result = (Result)cacheServiceResponse.Item2;

                if (request.Paginator != null)
                {
                    if (DataUtilities.IsPageValid(request.Paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateSolutions(request.Paginator, response, result);

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
                }
                else
                {
                    result.Payload.AddRange(response
                        .Objects
                        .OrderBy(s => ((ISudokuSolution)s).Id)
                        .ToList()
                        .ConvertAll(s => (object)s));
                }

                result.IsSuccess = response.IsSuccess;
                result.Message = SolutionsMessages.SolutionsFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<SolutionsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> SolveAsync(IAnnonymousCheckRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var gameResult = new AnnonymousGameResult();

                var intList = new List<int>();

                intList.AddRange(request.FirstRow);
                intList.AddRange(request.SecondRow);
                intList.AddRange(request.ThirdRow);
                intList.AddRange(request.FourthRow);
                intList.AddRange(request.FifthRow);
                intList.AddRange(request.SixthRow);
                intList.AddRange(request.SeventhRow);
                intList.AddRange(request.EighthRow);
                intList.AddRange(request.NinthRow);

                var sudokuSolver = new SudokuMatrix(intList);

                await sudokuSolver.SolveAsync();

                if (sudokuSolver.IsValid())
                {
                    _jobClient.Enqueue(() => _dataJobs.AddSolutionJobAsync(sudokuSolver.ToIntList()));

                    result.IsSuccess = true;

                    var solution = new SudokuSolution(sudokuSolver.ToIntList());

                    for (var i = 0; i < 73; i += 9)
                    {
                        gameResult.SudokuMatrix.Add(solution.SolutionList.GetRange(i, 9));
                    }
                    
                    result.Payload.Add(gameResult);

                    result.Message = SolutionsMessages.SudokuSolutionFoundMessage;
                }
                else
                {
                    var response = await _solutionsRepository.GetAllAsync();

                    var solvedSolutions = response
                        .Objects
                        .ConvertAll(s => (SudokuSolution)s)
                        .ToList();

                    intList = sudokuSolver.ToIntList();

                    if (solvedSolutions.Count > 0)
                    {
                        var solutonInDB = false;

                        foreach (var solution in solvedSolutions)
                        {
                            var possibleSolution = true;

                            for (var i = 0; i < intList.Count - 1; i++)
                            {
                                if (intList[i] != 0 && intList[i] != solution.SolutionList[i])
                                {
                                    possibleSolution = false;
                                    break;
                                }
                            }

                            if (possibleSolution)
                            {
                                solutonInDB = possibleSolution;
                                result.IsSuccess = possibleSolution;

                                var solutionMatrix = new SudokuMatrix(solution.SolutionList);
                                result.Payload.Add(solutionMatrix);
                                result.Message = SolutionsMessages.SudokuSolutionFoundMessage;
                                break;
                            }
                        }

                        if (!solutonInDB)
                        {
                            result.IsSuccess = false;
                            result.Message = SolutionsMessages.SudokuSolutionNotFoundMessage;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = SolutionsMessages.SudokuSolutionNotFoundMessage;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<SolutionsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GenerateAsync()
        {
            var result = new Result();

            try
            {
                var gameResult = new AnnonymousGameResult();

                var game = new Game();

                await game.SudokuMatrix.GenerateSolutionAsync();

                for (var i = 0; i < 73; i += 9)
                {
                    gameResult.SudokuMatrix.Add(game.SudokuMatrix.ToIntList().GetRange(i, 9));
                }
                
                result.Payload.Add(gameResult);
                
                _jobClient.Enqueue(() => _dataJobs.AddSolutionJobAsync(game.SudokuMatrix.ToIntList()));

                result.IsSuccess = true;
                result.Message = SolutionsMessages.SolutionGeneratedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<SolutionsService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public IResult GenerateSolutions(IRequest request)
        {
            var result = new Result();

            ArgumentNullException.ThrowIfNull(request, nameof(request));

            AddSolutionsPayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(AddSolutionsPayload), out IPayload conversionResult))
            {
                payload = (AddSolutionsPayload)conversionResult;
            }
            else
            {
                throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
            }

            if (payload.Limit == 0)
            {
                result.IsSuccess = false;
                result.Message = SolutionsMessages.SolutionsNotAddedMessage;

                return result;
            }

            var limit = 100;

            if (payload.Limit > limit)
            {
                result.IsSuccess = false;
                result.Message = SolutionsMessages.LimitExceedsSolutionsLimitMessage(payload.Limit.ToString());

                return result;
            }

            if (payload.Limit <= limit)
            {
                limit = payload.Limit;
            }
            
            _jobClient.Enqueue(() => _dataJobs.GenerateSolutionsJobAsync(limit));

            result.IsSuccess = true;
            result.Message = SolutionsMessages.SolutionsAddedMessage;

            return result;
        }
        #endregion
    }
}
