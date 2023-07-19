using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Data.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Test.Services
{
    public class MockedDifficultiesService
    {
        private MockedDifficultiesRepository MockedDifficultiesRepository { get; set; }

        internal Mock<IDifficultiesService> SuccessfulRequest { get; set; }
        internal Mock<IDifficultiesService> FailedRequest { get; set; }

        public MockedDifficultiesService(DatabaseContext context)
        {
            MockedDifficultiesRepository = new MockedDifficultiesRepository(context);

            SuccessfulRequest = new Mock<IDifficultiesService>();
            FailedRequest = new Mock<IDifficultiesService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(Service =>
                Service.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyFoundMessage,
                        Payload = new List<object>()
                        {
                            MockedDifficultiesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.GetDifficultiesAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultiesFoundMessage,
                        Payload = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(d => (object)d)
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyCreatedMessage,
                        Payload = new List<object>()
                            {
                                MockedDifficultiesRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyUpdatedMessage
                    } as IResult));

            SuccessfulRequest.Setup(Service =>
                Service.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyDeletedMessage
                    } as IResult));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(Service =>
                Service.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .FailedRequest
                            .Object
                            .AddAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.GetDifficultiesAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultiesNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                {
                    IsSuccess = MockedDifficultiesRepository
                        .FailedRequest
                        .Object
                        .AddAsync(It.IsAny<Difficulty>())
                        .Result
                        .IsSuccess,
                    Message = DifficultiesMessages.DifficultyNotCreatedMessage
                } as IResult));

            FailedRequest.Setup(Service =>
                Service.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(Service =>
                Service.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedDifficultiesRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<Difficulty>())
                            .Result
                            .IsSuccess,
                        Message = DifficultiesMessages.DifficultyNotDeletedMessage
                    } as IResult));
            #endregion
        }
    }
}
