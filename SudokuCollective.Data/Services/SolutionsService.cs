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
    public class SolutionsService : ISolutionsService
    {
        #region Fields
        private readonly ISolutionsRepository<SudokuSolution> _solutionsRepository;
        private readonly IRequestService _requestService;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly IBackgroundJobClient _jobClient;
        private readonly IDataJobs _dataJobs;
        private readonly ILogger<SolutionsService> _logger;
        #endregion

        #region Constructor
        public SolutionsService(
            ISolutionsRepository<SudokuSolution> solutionsRepository,
            IRequestService requestService,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            IBackgroundJobClient jobClient,
            IDataJobs dataJobs,
            ILogger<SolutionsService> logger)
        {
            _solutionsRepository = solutionsRepository;
            _requestService = requestService;
            _distributedCache = distributedCache;
            _cacheService = cacheService;
            _cacheKeys = cacheKeys;
            _jobClient = jobClient;
            _dataJobs = dataJobs;
            _logger = logger;
        }
        #endregion

        #region Methods

        public async Task<IResult> GetAsync(int id)
        {
            var result = new Result();

            if (id == 0)
            {
                result.IsSuccess = false;
                result.Message = SolutionsMessages.SolutionNotFoundMessage;

                return result;
            }

            try
            {
                var solutionResponse = await _solutionsRepository.GetAsync(id);

                if (solutionResponse.IsSuccess)
                {
                    var solution = (SudokuSolution)solutionResponse.Object;

                    result.IsSuccess = solutionResponse.IsSuccess;
                    result.Message = SolutionsMessages.SolutionFoundMessage;
                    result.Payload.Add(solution);

                    return result;
                }
                else if (!solutionResponse.IsSuccess && solutionResponse.Exception != null)
                {
                    result.IsSuccess = solutionResponse.IsSuccess;
                    result.Message = solutionResponse.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = SolutionsMessages.SolutionNotFoundMessage;

                    return result;
                }
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            var response = new RepositoryResponse();

            try
            {
                var cacheServiceResponse = await _cacheService.GetAllWithCacheAsync<SudokuSolution>(
                    _solutionsRepository,
                    _distributedCache,
                    _cacheKeys.GetSolutionsCacheKey,
                    DateTime.Now.AddHours(1),
                    result);

                response = (RepositoryResponse)cacheServiceResponse.Item1;
                result = (Result)cacheServiceResponse.Item2;

                if (response.IsSuccess)
                {
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
                else if (!response.IsSuccess && response.Exception != null)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception.Message;

                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = SolutionsMessages.SolutionsNotFoundMessage;

                    return result;
                }
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();
            var gameResult = new AnnonymousGameResult();

            try
            {
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
            var gameResult = new AnnonymousGameResult();
            
            try
            {
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = new Result();

            AddSolutionsPayload payload;

            if (request.Payload.ConvertToPayloadSuccessful(typeof(AddSolutionsPayload), out IPayload conversionResult))
            {
                payload = (AddSolutionsPayload)conversionResult;
            }
            else
            {
                result.IsSuccess = false;
                result.Message = ServicesMesages.InvalidRequestMessage;

                return result;
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
