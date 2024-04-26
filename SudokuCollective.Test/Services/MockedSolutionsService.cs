using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Requests;

namespace SudokuCollective.Test.Services
{
    public class MockedSolutionsService
    {
        private MockedSolutionsRepository MockedSolutionsRepository { get; set; }

        internal Mock<ISolutionsService> SuccessfulRequest { get; set; }
        internal Mock<ISolutionsService> FailedRequest { get; set; }
        internal Mock<ISolutionsService> SolveFailedRequest { get; set; }

        public MockedSolutionsService(DatabaseContext context)
        {
            MockedSolutionsRepository = new MockedSolutionsRepository(context);

            SuccessfulRequest = new Mock<ISolutionsService>();
            FailedRequest = new Mock<ISolutionsService>();
            SolveFailedRequest = new Mock<ISolutionsService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(service =>
                service.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionFoundMessage,
                        Payload = new List<object>()
                        {
                            MockedSolutionsRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GetSolutionsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionsFoundMessage,
                        Payload = MockedSolutionsRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(s => (object)s)
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.SolveAsync(It.IsAny<IAnnonymousCheckRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = SolutionsMessages.SudokuSolutionFoundMessage,
                        Payload = new List<object>()
                            {
                                MockedSolutionsRepository
                                    .SuccessfulRequest
                                    .Object
                                    .GetAsync(It.IsAny<int>())
                                    .Result
                                    .Object
                            }
                    } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GenerateAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<SudokuSolution>())
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionGeneratedMessage,
                        Payload = new List<object>()
                                {
                                    MockedSolutionsRepository
                                        .SuccessfulRequest
                                        .Object
                                        .GetAsync(It.IsAny<int>())
                                        .Result
                                        .Object
                                }
                } as IResult));

            SuccessfulRequest.Setup(service =>
                service.GenerateSolutions(It.IsAny<IRequest>()))
                .Returns(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .SuccessfulRequest
                            .Object
                            .AddSolutionsAsync(It.IsAny<List<ISudokuSolution>>())
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionsAddedMessage
                    } as IResult);
            #endregion

            #region FailedRequest
            FailedRequest.Setup(service =>
                service.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GetSolutionsAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionsNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.SolveAsync(It.IsAny<IAnnonymousCheckRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = SolutionsMessages.SudokuSolutionNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GenerateAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                        .FailedRequest
                        .Object
                        .AddAsync(It.IsAny<SudokuSolution>())
                        .Result
                        .IsSuccess,
                        Message = SolutionsMessages.SolutionNotGeneratedMessage
                    } as IResult));

            FailedRequest.Setup(service =>
                service.GenerateSolutions(It.IsAny<IRequest>()))
                .Returns(new Result()
                    {
                        IsSuccess = MockedSolutionsRepository
                            .FailedRequest
                            .Object
                            .AddSolutionsAsync(It.IsAny<List<ISudokuSolution>>())
                            .Result
                            .IsSuccess,
                        Message = SolutionsMessages.SolutionsNotAddedMessage
                    } as IResult);
            #endregion

            #region SolveFailedRequest
            SolveFailedRequest.Setup(service =>
                service.SolveAsync(It.IsAny<IAnnonymousCheckRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = SolutionsMessages.SudokuSolutionNotFoundMessage
                    } as IResult));
            #endregion
        }
    }
}
