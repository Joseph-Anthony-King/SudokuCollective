using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.Services
{
    internal class MockedUserManagementService
    {
        internal Mock<IUserManagementService> SuccssfulRequest { get; set; }
        internal Mock<IUserManagementService> FailedRequest { get; set; }
        internal Mock<IUserManagementService> UserNameFailedRequest { get; set; }
        internal Mock<IUserManagementService> PasswordFailedRequest { get; set; }

        public MockedUserManagementService()
        {
            SuccssfulRequest = new Mock<IUserManagementService>();
            FailedRequest = new Mock<IUserManagementService>();
            UserNameFailedRequest = new Mock<IUserManagementService>();
            PasswordFailedRequest = new Mock<IUserManagementService>();

            #region SuccessfulRequest
            SuccssfulRequest.Setup(userManagementService =>
                userManagementService.IsValidUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(true));

            SuccssfulRequest.Setup(userManagementService =>
                userManagementService.ConfirmUserNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.UserNameConfirmedMessage,
                        Payload = new List<object> { new AuthenticatedUserNameResult { UserName = "TestSuperUser" } }
                    } as IResult));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(userManagementService =>
                userManagementService.IsValidUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(userManagementService =>
                userManagementService.ConfirmUserNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = UsersMessages.NoUserIsUsingThisEmailMessage
                    } as IResult));
            #endregion

            #region UserNameFailedRequest
            UserNameFailedRequest.Setup(userManagementService =>
                userManagementService.ConfirmAuthenticationIssueAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                    .Returns(Task.FromResult(UserAuthenticationErrorType.USERNAMEINVALID));
            #endregion

            #region PasswordFailedRequest
            PasswordFailedRequest.Setup(userManagementService =>
                userManagementService.ConfirmAuthenticationIssueAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                    .Returns(Task.FromResult(UserAuthenticationErrorType.PASSWORDINVALID));
            #endregion
        }
    }
}
