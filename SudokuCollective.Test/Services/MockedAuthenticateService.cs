using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.Services
{
    public class MockedAuthenticateService
    {
        internal Mock<IAuthenticateService> SuccessfulRequest { get; set; }
        internal Mock<IAuthenticateService> FailedRequest { get; set; }

        public MockedAuthenticateService()
        {
            SuccessfulRequest = new Mock<IAuthenticateService>();
            FailedRequest = new Mock<IAuthenticateService>();

            SuccessfulRequest.Setup(authenticateService =>
                authenticateService.AuthenticateAsync(It.IsAny<LoginRequest>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = true,
                        Message = UsersMessages.UserFoundMessage,
                        Payload = new List<object>()
                        {
                            new AuthenticationResult()
                            {
                                Token = TestObjects.GetToken(),
                                User = new AuthenticatedUser()
                            }
                        }
                    } as IResult));

            FailedRequest.Setup(authenticateService =>
                authenticateService.AuthenticateAsync(It.IsAny<LoginRequest>()))
                    .Returns(Task.FromResult(new Result()
                    {
                        IsSuccess = false,
                        Message = UsersMessages.UserNotFoundMessage
                    } as IResult));
        }
    }
}
