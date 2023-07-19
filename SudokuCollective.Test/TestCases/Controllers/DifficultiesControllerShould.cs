using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Data.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Test.TestData;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class DifficultiesControllerShould
    {
        private DatabaseContext context;
        private DifficultiesController sutSuccess;
        private DifficultiesController sutFailure;
        private MockedDifficultiesService mockedDifficultiesService;
        private MockedAppsService mockedAppsService;
        private MockedRequestService mockedRequestService;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<DifficultiesController>> mockedLogger;
        private Request request;
        private CreateDifficultyPayload createDifficultyPayload;
        private UpdateDifficultyPayload updateDifficultyPayload;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedDifficultiesService = new MockedDifficultiesService(context);
            mockedAppsService = new MockedAppsService(context);
            mockedRequestService = new MockedRequestService();
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<DifficultiesController>>();

            request = TestObjects.GetRequest();

            createDifficultyPayload = new CreateDifficultyPayload()
            {
                Name = "Test Difficulty",
                DisplayName = "Test Difficulty",
                DifficultyLevel = DifficultyLevel.TEST
            };

            updateDifficultyPayload = new UpdateDifficultyPayload()
            {
                Id = 1,
                Name = "Test Difficulty",
                DisplayName = "Test Difficulty"
            };

            sutSuccess = new DifficultiesController(
                mockedDifficultiesService.SuccessfulRequest.Object,
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object);

            sutFailure = new DifficultiesController(
                mockedDifficultiesService.FailedRequest.Object,
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object);
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetDifficulty()
        {
            // Arrange
            var difficultyId = 2;

            // Act
            var actionResult = await sutSuccess.GetAsync(difficultyId);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var difficulty = (Difficulty)result.Payload[0];
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(difficulty, Is.InstanceOf<Difficulty>());
            Assert.That(message, Is.EqualTo("Status Code 200: Difficulty found"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetDifficultyFail()
        {
            // Arrange
            var difficultyId = 2;

            // Act
            var actionResult = await sutFailure.GetAsync(difficultyId);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Difficulty not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetDifficulties()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.GetDifficultiesAsync();
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var difficulties = result.Payload.ConvertAll(d => (Difficulty)d);
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Difficulties found"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(difficulties.Count, Is.EqualTo(4));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetDifficultiesFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.GetDifficultiesAsync();
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Difficulties not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyUpdateDifficulties()
        {
            // Arrange
            request.Payload = updateDifficultyPayload;

            // Act
            var actionResult = await sutSuccess.UpdateAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Difficulty updated"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldUpdateGamesFail()
        {
            // Arrange
            request.Payload = updateDifficultyPayload;

            // Act
            var actionResult = await sutFailure.UpdateAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Difficulty not updated"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyCreateDifficulties()
        {
            // Arrange
            request.Payload = createDifficultyPayload;

            // Act
            var actionResult = await sutSuccess.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var difficulty = (Difficulty)result.Payload[0];
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 201: Difficulty created"));
            Assert.That(statusCode, Is.EqualTo(201));
            Assert.That(difficulty, Is.InstanceOf<Difficulty>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldCreateDifficultiesFail()
        {
            // Arrange
            request.Payload = createDifficultyPayload;

            // Act
            var actionResult = await sutFailure.PostAsync(request);
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Difficulty not created"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyDeleteDifficulties()
        {
            // Arrange
            var difficultyId = 2;

            // Act
            var actionResult = await sutSuccess.DeleteAsync(difficultyId, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = ((Result)((OkObjectResult)actionResult.Result).Value).Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Difficulty deleted"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldDeleteDifficultiesFail()
        {
            // Arrange
            var difficultyId = 2;

            // Act
            var actionResult = await sutFailure.DeleteAsync(difficultyId, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Difficulty not deleted"));
            Assert.That(statusCode, Is.EqualTo(404));
        }
    }
}
