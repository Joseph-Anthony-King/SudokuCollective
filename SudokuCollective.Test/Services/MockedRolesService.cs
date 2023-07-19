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
    public class MockedRolesService
    {
        private MockedRolesRepository MockedRolesRepository { get; set; }

        internal Mock<IRolesService> SuccessfulRequest { get; set; }
        internal Mock<IRolesService> FailedRequest { get; set; }

        public MockedRolesService(DatabaseContext context)
        {
            MockedRolesRepository = new MockedRolesRepository(context);

            SuccessfulRequest = new Mock<IRolesService>();
            FailedRequest = new Mock<IRolesService>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(rolesService =>
                rolesService.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleFoundMessage,
                        Payload = new List<object>()
                        {
                            MockedRolesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }
                    } as IResult));

            SuccessfulRequest.Setup(rolesService =>
                rolesService.GetRolesAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RolesFoundMessage,
                        Payload = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .Objects
                            .ConvertAll(r => (object)r)
                    } as IResult));

            SuccessfulRequest.Setup(rolesService =>
                rolesService.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .AddAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleCreatedMessage,
                        Payload = new List<object>()
                        {
                            MockedRolesRepository
                                .SuccessfulRequest
                                .Object
                                .GetAsync(It.IsAny<int>())
                                .Result
                                .Object
                        }
                    } as IResult));

            SuccessfulRequest.Setup(rolesService =>
                rolesService.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .UpdateAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleUpdatedMessage
                    } as IResult));

            SuccessfulRequest.Setup(rolesService =>
                rolesService.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .SuccessfulRequest
                            .Object
                            .DeleteAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleDeletedMessage
                    } as IResult));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(rolesService =>
                rolesService.GetAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .FailedRequest
                            .Object
                            .GetAsync(It.IsAny<int>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleNotFoundMessage,
                    } as IResult));

            FailedRequest.Setup(rolesService =>
                rolesService.GetRolesAsync())
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .FailedRequest
                            .Object
                            .GetAllAsync()
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RolesNotFoundMessage
                    } as IResult));

            FailedRequest.Setup(rolesService =>
                rolesService.CreateAsync(It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .FailedRequest
                            .Object
                            .AddAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleNotCreatedMessage
                    } as IResult));

            FailedRequest.Setup(rolesService =>
                rolesService.UpdateAsync(It.IsAny<int>(), It.IsAny<IRequest>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .FailedRequest
                            .Object
                            .UpdateAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleNotUpdatedMessage
                    } as IResult));

            FailedRequest.Setup(rolesService =>
                rolesService.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = MockedRolesRepository
                            .FailedRequest
                            .Object
                            .DeleteAsync(It.IsAny<Role>())
                            .Result
                            .IsSuccess,
                        Message = RolesMessages.RoleNotDeletedMessage
                    } as IResult));
            #endregion
        }
    }
}
