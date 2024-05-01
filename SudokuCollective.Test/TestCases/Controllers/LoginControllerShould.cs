using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Test.TestData;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class LoginControllerShould
    {
        private LoginController sut;
        private LoginController sutInvalid;
        private LoginController sutInvalidUserName;
        private LoginController sutInvalidPassword;
        private LoginController sutUserNameNotFound;
        private MockedAuthenticateService mockedAuthenticateService;
        private MockedUserManagementService mockedUserManagementService;
        private MockedUserManagementService mockedUserManagementInvalidUserNameService;
        private MockedUserManagementService mockedUserManagementInvalidPasswordService;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<LoginController>> mockedLogger;
        private LoginRequest loginRequest;
        private string userName;
        private string password;
        private string email;

        [SetUp]
        public void Setup()
        {
            userName = "TestSuperUser";
            password = "T#stP@ssw0rd";
            email = "TestSuperUser@example.com";

            mockedAuthenticateService = new MockedAuthenticateService();
            mockedUserManagementService = new MockedUserManagementService();
            mockedUserManagementInvalidUserNameService = new MockedUserManagementService();
            mockedUserManagementInvalidPasswordService = new MockedUserManagementService();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<LoginController>>();

            loginRequest = new LoginRequest()
            {
                UserName = userName,
                Password = password,
                License = TestObjects.GetLicense()
            };

            sut = new LoginController(
                mockedAuthenticateService.SuccessfulRequest.Object,
                mockedUserManagementService.SuccssfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);
            sutInvalid = new LoginController(
                mockedAuthenticateService.FailedRequest.Object,
                mockedUserManagementService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);
            sutInvalidUserName = new LoginController(
                mockedAuthenticateService.FailedRequest.Object,
                mockedUserManagementInvalidUserNameService.UserNameFailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);
            sutInvalidPassword = new LoginController(
                mockedAuthenticateService.FailedRequest.Object,
                mockedUserManagementInvalidPasswordService.PasswordFailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);
            sutUserNameNotFound = new LoginController(
                mockedAuthenticateService.FailedRequest.Object,
                mockedUserManagementService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);
        }

        [Test, Category("Controllers")]
        public async Task AuthenticateUsers()
        {
            // Arrange

            // Act
            var actionResult = await sut.PostAsync(loginRequest);
            var result = ((OkObjectResult)actionResult.Result).Value;
            var message = ((Result)((OkObjectResult)actionResult.Result).Value).Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: User found"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controllers")]
        public async Task ReturnBadRequestMessageWhenUserNameIsInvalid()
        {
            // Arrange

            // Act
            var actionResult = await sutInvalidUserName.PostAsync(loginRequest);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: No user is using this user name"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test, Category("Controllers")]
        public async Task ReturnBadRequestMessageWhenPasswordIsIncorrect()
        {
            // Arrange

            // Act
            var actionResult = await sutInvalidPassword.PostAsync(loginRequest);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Password is incorrect"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test, Category("Controllers")]
        public async Task ReturnBadRequestMessageWhenUsersArentAuthenticated()
        {
            // Arrange

            // Act
            var actionResult = await sutInvalid.PostAsync(loginRequest);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: User not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test, Category("Controllers")]
        public async Task ReturnUserName()
        {
            // Arrange
            var request = new ConfirmUserNameRequest
            {
                Email = email,
                License = TestObjects.GetLicense()
            };

            // Act
            var actionResult = await sut.ConfirmUserNameAsync(request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var username = ((AuthenticatedUserNameResult)((Result)((OkObjectResult)actionResult.Result).Value).Payload[0]).UserName;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: User name confirmed"));
            Assert.That(username, Is.EqualTo(userName));
        }

        [Test, Category("Controllers")]
        public async Task ReturnErrorMessageIfUserNameNotFound()
        {
            // Arrange
            var request = new ConfirmUserNameRequest
            {
                Email = email,
                License = TestObjects.GetLicense()
            };

            // Act
            var actionResult = await sutUserNameNotFound.ConfirmUserNameAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: No user is using this email"));
        }
    }
}
