using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SudokuCollective.Data.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Test.TestData;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class SignupControllerShould
    {
        private DatabaseContext context;
        private SignupController sutSuccess;
        private SignupController sutFailure;
        private MockedUsersService mockedUsersService;
        private MockedAuthenticateService mockedAuthenticateService;
        private MockedRequestService mockedRequestService;
        private SignupRequest signupRequest;
        private ResendEmailConfirmationRequest Request;
        private Mock<IWebHostEnvironment> mockedWebHostEnvironment;
        private Mock<ILogger<SignupController>> mockedLogger;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();

            mockedUsersService = new MockedUsersService(context);
            mockedAuthenticateService = new MockedAuthenticateService();
            mockedRequestService = new MockedRequestService();
            mockedWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockedLogger = new Mock<ILogger<SignupController>>();

            sutSuccess = new SignupController(
                mockedUsersService.SuccessfulRequest.Object,
                mockedAuthenticateService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedWebHostEnvironment.Object,
                mockedLogger.Object);
            sutFailure = new SignupController(
                mockedUsersService.FailedRequest.Object,
                mockedAuthenticateService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedWebHostEnvironment.Object,
                mockedLogger.Object);

            signupRequest = new SignupRequest()
            {
                UserName = "TestUser3",
                FirstName = "Test",
                LastName = "User 3",
                NickName = "My Nickname",
                Email = "testuser3@example.com",
                Password = "T#stP@ssw0rd",
                License = TestObjects.GetLicense()
            };

            Request = new ResendEmailConfirmationRequest();
        }

        [Test, Category("Controllers")]
        public async Task SuccessfullyRegisterUsers()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.PostAsync(signupRequest);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var user = ((UserCreatedResult)result.Payload[0]).User;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 201: User created"));
            Assert.That(statusCode, Is.EqualTo(201));
            Assert.That(user, Is.InstanceOf<UserDTO>());
        }

        [Test, Category("Controllers")]
        public async Task IssueErrorAndMessageShouldSuccessfullyRegisterUsersFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.PostAsync(signupRequest);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var errorMessage = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(errorMessage, Is.EqualTo("Status Code 404: User not created"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test, Category("Controllers")]
        public async Task SuccessfullyResendEmailConfirmation()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.ResendEmailConfirmationAsync(Request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var emailResent = ((UserResult)result.Payload[0]).ConfirmationEmailSuccessfullySent;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Email confirmation email resent"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(emailResent, Is.True);
        }

        [Test, Category("Controllers")]
        public async Task IssueErrorAndMessageShouldSuccessfullyResendEmailConfirmationFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.ResendEmailConfirmationAsync(Request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Email confirmation email not resent"));
            Assert.That(statusCode, Is.EqualTo(404));
        }
    }
}
