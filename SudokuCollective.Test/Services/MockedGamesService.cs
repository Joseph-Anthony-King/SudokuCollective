using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Data.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Enums;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.Services
{
    public class MockedGamesService
    {
        private MockedGamesRepository MockedGamesRepository { get; set; }

        internal Mock<IGamesService> SuccessfulRequest { get; set; }
        internal Mock<IGamesService> FailedRequest { get; set; }

        public MockedGamesService(DatabaseContext context)
        {
            MockedGamesRepository = new MockedGamesRepository(context);

            SuccessfulRequest = new Mock<IGamesService>();
            FailedRequest = new Mock<IGamesService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(Service =>
                Service.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameCreatedMessage,
                        Payload = new List<object>()
                        {
                            MockedGamesRepository
                                .SuccessfulRequest
                                .Object
                                .AddAsync(It.IsAny<Game>())
                                .Result
                                .Object
                        }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameUpdatedMessage,
                        Payload = new List<object>()
                            {
                                MockedGamesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .AddAsync(It.IsAny<Game>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameDeletedMessage
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.GetGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedGamesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .AddAsync(It.IsAny<Game>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.GetGamesAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GamesFoundMessage,
                        Payload = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(g => (object)g)
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.GetMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedGamesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .AddAsync(It.IsAny<Game>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.GetMyGamesAsync(It.IsAny<Request>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GamesFoundMessage,
                        Payload = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .GetMyGamesAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .Objects
                            .ConvertAll(g => (object)g)
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.UpdateMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameUpdatedMessage,
                        Payload = new List<object>()
                            {
                                MockedGamesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .AddAsync(It.IsAny<Game>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedGamesRepository
                        .SuccessfulRequest
                        .Object
                        .DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
                        .Result
                        .IsSuccess,
                    Message = GamesMessages.GameDeletedMessage
                } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.CheckAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameSolvedMessage,
                        Payload = new List<object>()
                            {
                                MockedGamesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .AddAsync(It.IsAny<Game>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.CreateAnnonymousAsync(It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = GamesMessages.GameCreatedMessage,
                        Payload = new List<object>() { TestObjects.GetAnnonymousGame() }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.CheckAnnonymous(It.IsAny<List<int>>()))
                .Returns(new Result()
                    {
                        IsSuccess = true,
                        Message = GamesMessages.GameSolvedMessage
                    } as IResult);
            #endregion

            #region FailedRequest
            FailedRequest.Setup(Service =>
                Service.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .AddAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotCreatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotDeletedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.GetGameAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .GetAppGameAsync(It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.GetGamesAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .GetAppGamesAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GamesNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.GetMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.GetMyGamesAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .GetMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GamesNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.UpdateMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .DeleteMyGameAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotDeletedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.CheckAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedGamesRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<Game>())
                            .Result
                            .IsSuccess,
                        Message = GamesMessages.GameNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.CreateAnnonymousAsync(It.IsAny<DifficultyLevel>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = GamesMessages.GameNotCreatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.CheckAnnonymous(It.IsAny<List<int>>()))
                .Returns(new Result()
                    {
                        IsSuccess = false,
                        Message = GamesMessages.GameNotSolvedMessage
                    } as IResult);
            #endregion
        }
    }
}
