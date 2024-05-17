using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Jobs;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Extensions;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Utilities;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using Request = SudokuCollective.Logs.Models.Request;

namespace SudokuCollective.Data.Services
{
    public class GamesService(
        IGamesRepository<Game> gamesRepsitory,
        IAppsRepository<App> appsRepository,
        IUsersRepository<User> usersRepository,
        IDifficultiesRepository<Difficulty> difficultiesRepository,
        IRequestService requestService,
        IBackgroundJobClient jobClient,
        IDataJobs dataJobs,
        ILogger<GamesService> logger) : IGamesService
    {
        #region Fields
        private readonly IGamesRepository<Game> _gamesRepository = gamesRepsitory;
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly IUsersRepository<User> _usersRepository = usersRepository;
        private readonly IDifficultiesRepository<Difficulty> _difficultiesRepository = difficultiesRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IBackgroundJobClient _jobClient = jobClient;
        private readonly IDataJobs _dataJobs = dataJobs;
        private readonly ILogger<GamesService> _logger = logger;
        #endregion

        #region Methods
        public async Task<IResult> CreateAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                CreateGamePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(CreateGamePayload), out IPayload conversionResult))
                {
                    payload = (CreateGamePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var userResponse = await _usersRepository.GetAsync(request.RequestorId);

                #region userResponse fails
                if (!userResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = UsersMessages.UserDoesNotExistMessage;

                    return result;
                }
                #endregion

                var difficultyResponse = await _difficultiesRepository.GetByDifficultyLevelAsync(payload.DifficultyLevel);

                #region difficultyResponse fails
                if (!difficultyResponse.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.Message = DifficultiesMessages.DifficultyDoesNotExistMessage;

                    return result;
                }
                #endregion

                var user = (User)userResponse.Object;
                var difficulty = (Difficulty)difficultyResponse.Object;

                var game = new Game(
                    user,
                    new SudokuMatrix(),
                    difficulty,
                    request.AppId);

                await game.SudokuMatrix.GenerateSolutionAsync();

                var gameResponse = await _gamesRepository.AddAsync(game);

                #region gameResponse fails
                if (!gameResponse.IsSuccess)
                {
                    result.IsSuccess = gameResponse.IsSuccess;
                    result.Message = gameResponse.Exception != null ? 
                        gameResponse.Exception.Message : 
                        GamesMessages.GameNotCreatedMessage;

                    return result;
                }
                #endregion

                game = (Game)gameResponse.Object;

                game.User = null;
                _ = game.SudokuMatrix.SudokuCells.OrderBy(cell => cell.Index);

                result.IsSuccess = gameResponse.IsSuccess;
                result.Message = GamesMessages.GameCreatedMessage;
                result.Payload.Add(game);

                return result;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;

                SudokuCollectiveLogger.LogError<GamesService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, result.Message),
                    e,
                    (Request)_requestService.Get());

                return result;
            }
        }

        public async Task<IResult> GetGameAsync(int id, int appId)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(appId, nameof(appId));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(appId, nameof(appId));

                var appExists = await _appsRepository.HasEntityAsync(appId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var getResponse = await _gamesRepository.GetAppGameAsync(id, appId);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                var game = (Game)getResponse.Object;

                result.IsSuccess = true;
                result.Message = GamesMessages.GameFoundMessage;
                result.Payload.Add(game);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetGamesAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var appExists = await _appsRepository.HasEntityAsync(request.AppId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var response = await _gamesRepository.GetAppGamesAsync(request.AppId);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        GamesMessages.GamesNotFoundMessage;

                    return result;
                }
                #endregion

                if (request.Paginator != null)
                {
                    if (DataUtilities.IsPageValid(request.Paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateGames(
                            request.Paginator, 
                            response, 
                            result);

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
                    result.Payload.AddRange(response.Objects);
                }

                result.IsSuccess = response.IsSuccess;
                result.Message = GamesMessages.GamesFoundMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetMyGameAsync(int id, IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                var appExists = await _appsRepository.HasEntityAsync(request.AppId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var getResponse = await _gamesRepository.GetMyGameAsync(
                    request.RequestorId, 
                    id,
                    request.AppId);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                var game = (Game)getResponse.Object;

                result.IsSuccess = true;
                result.Message = GamesMessages.GameFoundMessage;
                result.Payload.Add(game);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> GetMyGamesAsync(IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));

                GamesPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(GamesPayload), out IPayload conversionResult))
                {
                    payload = (GamesPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var appExists = await _appsRepository.HasEntityAsync(request.AppId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var response = await _gamesRepository.GetMyGamesAsync(
                    payload.UserId,
                    request.AppId);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        GamesMessages.GamesNotFoundMessage;

                    return result;
                }
                #endregion

                if (request.Paginator != null)
                {
                    if (DataUtilities.IsPageValid(request.Paginator, response.Objects))
                    {
                        result = PaginatorUtilities.PaginateGames(
                            request.Paginator, 
                            response, 
                            result);

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
                    result.Payload.AddRange(response.Objects);
                }

                result.IsSuccess = response.IsSuccess;
                result.Message = GamesMessages.GamesFoundMessage;

                return result; 
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
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

                GamePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(GamePayload), out IPayload conversionResult))
                {
                    payload = (GamePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var appExists = await _gamesRepository.HasEntityAsync(id);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = GamesMessages.GameNotFoundMessage;

                    return result;
                }

                var getResponse = await _gamesRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                var game = (Game)getResponse.Object;

                foreach (var cell in payload.SudokuCells)
                {
                    foreach (var savedCell in (game.SudokuMatrix.SudokuCells))
                    {
                        if (savedCell.Id == cell.Id && savedCell.Hidden)
                        {
                            savedCell.DisplayedValue = cell.DisplayedValue;
                        }
                    }
                }

                var updateResponse = await _gamesRepository.UpdateAsync(game);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        GamesMessages.GameNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Message = GamesMessages.GameUpdatedMessage;
                result.Payload.Add((Game)updateResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> UpdateMyGameAsync(int id, IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                GamePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(GamePayload), out IPayload conversionResult))
                {
                    payload = (GamePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var appExists = await _appsRepository.HasEntityAsync(request.AppId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var getResponse = await _gamesRepository.GetMyGameAsync(
                    request.RequestorId, 
                    id,
                    request.AppId);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                var game = (Game)getResponse.Object;

                foreach (var cell in payload.SudokuCells)
                {
                    foreach (var savedCell in game.SudokuMatrix.SudokuCells)
                    {
                        if (savedCell.Id == cell.Id && savedCell.Hidden)
                        {
                            savedCell.DisplayedValue = cell.DisplayedValue;
                        }
                    }
                }

                var updateResponse = await _gamesRepository.UpdateAsync(game);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        GamesMessages.GameNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Message = GamesMessages.GameUpdatedMessage;
                result.Payload.Add((Game)updateResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
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

                var getResponse = await _gamesRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message :
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                var deleteResponse = await _gamesRepository.DeleteAsync((Game)getResponse.Object);

                #region deleteResponse fails
                if (!deleteResponse.IsSuccess)
                {
                    result.IsSuccess = deleteResponse.IsSuccess;
                    result.Message = deleteResponse.Exception != null ? 
                        deleteResponse.Exception.Message : 
                        GamesMessages.GameNotDeletedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = deleteResponse.IsSuccess;
                result.Message = GamesMessages.GameDeletedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> DeleteMyGameAsync(int id, IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                GamesPayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(GamesPayload), out IPayload conversionResult))
                {
                    payload = (GamesPayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var appExists = await _appsRepository.HasEntityAsync(request.AppId);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = AppsMessages.AppNotFoundMessage;

                    return result;
                }

                var response = await _gamesRepository.DeleteMyGameAsync(
                    payload.UserId, 
                    id, 
                    request.AppId);

                #region response fails
                if (!response.IsSuccess)
                {
                    result.IsSuccess = response.IsSuccess;
                    result.Message = response.Exception != null ? 
                        response.Exception.Message : 
                        GamesMessages.GameNotDeletedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = true;
                result.Message = GamesMessages.GameDeletedMessage;

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> CheckAsync(int id, IRequest request)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(id, nameof(id));

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

                ArgumentNullException.ThrowIfNull(request, nameof(request));

                GamePayload payload;

                if (request.Payload.ConvertToPayloadSuccessful(typeof(GamePayload), out IPayload conversionResult))
                {
                    payload = (GamePayload)conversionResult;
                }
                else
                {
                    throw new ArgumentException(ServicesMesages.InvalidRequestMessage);
                }

                var getResponse = await _gamesRepository.GetAsync(id);

                #region getResponse fails
                if (!getResponse.IsSuccess)
                {
                    result.IsSuccess = getResponse.IsSuccess;
                    result.Message = getResponse.Exception != null ? 
                        getResponse.Exception.Message : 
                        GamesMessages.GameNotFoundMessage;

                    return result;
                }
                #endregion

                foreach (var cell in payload.SudokuCells)
                {
                    foreach (var savedCell in ((Game)getResponse.Object).SudokuMatrix.SudokuCells)
                    {
                        if (savedCell.Id == cell.Id && savedCell.Hidden)
                        {
                            savedCell.DisplayedValue = cell.DisplayedValue;
                        }
                    }
                }

                if (((Game)getResponse.Object).IsSolved())
                {
                    result.Message = GamesMessages.GameSolvedMessage;
                }
                else
                {
                    result.Message = GamesMessages.GameNotSolvedMessage;
                }

                var updateResponse = await _gamesRepository.UpdateAsync((Game)getResponse.Object);

                #region updateResponse fails
                if (!updateResponse.IsSuccess)
                {
                    result.IsSuccess = updateResponse.IsSuccess;
                    result.Message = updateResponse.Exception != null ? 
                        updateResponse.Exception.Message : 
                        GamesMessages.GameNotUpdatedMessage;

                    return result;
                }
                #endregion

                result.IsSuccess = updateResponse.IsSuccess;
                result.Payload.Add((Game)updateResponse.Object);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public async Task<IResult> CreateAnnonymousAsync(DifficultyLevel difficultyLevel)
        {
            var result = new Result();

            try
            {
                ArgumentNullException.ThrowIfNull(nameof(difficultyLevel));

                if (difficultyLevel == DifficultyLevel.NULL || difficultyLevel == DifficultyLevel.TEST)
                {
                    throw new ArgumentException(DifficultiesMessages.DifficultyNotValidMessage);
                }

                var appExists = await _difficultiesRepository.HasDifficultyLevelAsync(difficultyLevel);

                if (!appExists)
                {
                    result.IsSuccess = false;
                    result.Message = DifficultiesMessages.DifficultyNotFoundMessage;

                    return result;
                }

                var repositoryResponse = await _difficultiesRepository.GetByDifficultyLevelAsync(difficultyLevel);

                var game = new Game((Difficulty)repositoryResponse.Object);

                await game.SudokuMatrix.GenerateSolutionAsync();

                var gameResult = new AnnonymousGameResult();

                for (var i = 0; i < 73; i += 9)
                {
                    gameResult.SudokuMatrix.Add(game.SudokuMatrix.ToDisplayedIntList().GetRange(i, 9));
                }

                result.IsSuccess = true;
                result.Message = GamesMessages.GameCreatedMessage;
                result.Payload.Add(gameResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }

        public IResult CheckAnnonymous(List<int> intList)
        {
            ArgumentNullException.ThrowIfNull(intList, nameof(intList));

            try
            {
                var result = new Result();

                if (intList.Count != 81 || intList.Contains(0))
                {
                    result.IsSuccess = false;
                    result.Message = GamesMessages.GameNotSolvedMessage;

                    return result;
                }

                var game = new Game(
                    new Difficulty
                    {
                        DifficultyLevel = DifficultyLevel.TEST
                    },
                    intList);

                game.SudokuMatrix.SetPattern();

                result.IsSuccess = game.IsSolved();

                if (result.IsSuccess)
                {
                    _jobClient.Enqueue(() => _dataJobs.AddSolutionJobAsync(game.SudokuMatrix.ToIntList()));

                    result.Message = GamesMessages.GameSolvedMessage;
                }
                else
                {
                    result.Message = GamesMessages.GameNotSolvedMessage;
                }

                return result;
            }
            catch (Exception e)
            {
                SudokuCollectiveLogger.LogError<GamesService>(
                    _logger,
                    LogsUtilities.GetServiceErrorEventId(), 
                    string.Format(LoggerMessages.ErrorThrownMessage, e.Message),
                    e,
                    (SudokuCollective.Logs.Models.Request)_requestService.Get());

                throw;
            }
        }

        public IResult ScheduleCreateGame(DifficultyLevel difficultyLevel, IRequest request = null)
        {
            var result = new Result();

            try
            {
                if (difficultyLevel == DifficultyLevel.NULL || difficultyLevel == DifficultyLevel.TEST)
                {
                    throw new ArgumentException(DifficultiesMessages.DifficultyNotValidMessage);
                }

                string jobId;

                if (request == null)
                {
                    jobId = _jobClient.Schedule(() => CreateAnnonymousAsync(difficultyLevel),
                        TimeSpan.FromMicroseconds(500));
                }
                else
                {
                    jobId = _jobClient.Schedule(() => CreateAsync(request),
                        TimeSpan.FromMicroseconds(500));
                }

                result.IsSuccess = true;
                result.Message = string.Format(
                    "Create game job {0} scheduled.", 
                    !string.IsNullOrEmpty(jobId) ? 
                        jobId : 
                        "5d74fa7b-db93-4213-8e0c-da2f3179ed05");
                result.Payload.Add(new { jobId });

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<GamesService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }

        }
        #endregion
    }
}
