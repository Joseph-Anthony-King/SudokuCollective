using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;
using SudokuCollective.Api.V1.Controllers;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Enums;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Requests;
using Microsoft.AspNetCore.Hosting;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class GamesControllerShould
    {
        private DatabaseContext context;
        private GamesController sutSuccess;
        private GamesController sutFailure;
        private MockedGamesService mockGamesService;
        private MockedAppsService mockAppsService;
        private MockedRequestService mockedRequestService;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<GamesController>> mockedLogger;
        private Mock<IWebHostEnvironment> mockedWebHostEnvironment;
        private Request request;
        private CreateGamePayload createGamePayload;
        private GamePayload updateGamePayload;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockGamesService = new MockedGamesService(context);
            mockAppsService = new MockedAppsService(context);
            mockedRequestService = new MockedRequestService();
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<GamesController>>();
            mockedWebHostEnvironment = new Mock<IWebHostEnvironment>();

            request = new Request();

            createGamePayload = new CreateGamePayload();

            updateGamePayload = TestObjects.GetGamePayload(6);

            sutSuccess = new GamesController(
                mockGamesService.SuccessfulRequest.Object,
                mockAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);

            sutFailure = new GamesController(
                mockGamesService.FailedRequest.Object,
                mockAppsService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetGame()
        {
            // Arrange
            var gameId = 1;

            // Act
            var actionResult = await sutSuccess.GetGameAsync(gameId, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var game = (Game)result.Payload[0];
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game found"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(game, Is.InstanceOf<Game>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetGameFail()
        {
            // Arrange
            var gameId = 1;

            // Act
            var actionResult = await sutFailure.GetGameAsync(gameId, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetGames()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.GetGamesAsync(request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Games found"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetGamesFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.GetGamesAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Games not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyDeleteGames()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.DeleteAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game deleted"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldDeleteGamesFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.DeleteAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not deleted"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyUpdateGames()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.UpdateAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game updated"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldUpdateGamesFail()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.UpdateAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not updated"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyCreateGames()
        {
            // Arrange
            request.Payload = createGamePayload;

            // Act
            var actionResult = await sutSuccess.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var game = (Game)result.Payload[0];
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 201: Game created"));
            Assert.That(statusCode, Is.EqualTo(201));
            Assert.That(game, Is.InstanceOf<Game>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldCreateGamesFail()
        {
            // Arrange
            request.Payload = createGamePayload;

            // Act
            var actionResult = await sutFailure.PostAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not created"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyCheckGames()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.CheckAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var game = result.Payload[0];
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game solved"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(game, Is.InstanceOf<Game>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldCheckGamesFail()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.CheckAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not updated"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetGameByUserId()
        {
            // Arrange
            var userId = 1;
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.GetMyGameAsync(userId, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var game = (Game)result.Payload[0];
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game found"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(game, Is.InstanceOf<Game>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetGameByUserIdFail()
        {
            // Arrange
            var userId = 1;
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.GetMyGameAsync(userId, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetGamesByUserId()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.GetMyGamesAsync(request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Games found"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetGamesByUserIdFail()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.GetMyGamesAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Games not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyUpdateGamesByUserId()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.UpdateMyGameAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game updated"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldUpdateGamesByUserIdFail()
        {
            // Arrange
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.UpdateMyGameAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not updated"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyDeleteGameByUserId()
        {
            // Arrange
            var userId = 1;
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutSuccess.DeleteMyGameAsync(userId, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game deleted"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldDeleteGameByUserIdFail()
        {
            // Arrange
            var userId = 1;
            request.Payload = updateGamePayload;

            // Act
            var actionResult = await sutFailure.DeleteMyGameAsync(userId, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Game not deleted"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyCreateAnnonymousGames()
        {
            // Arrange

            // Assert
            var actionResult = await sutSuccess.CreateAnnonymousAsync(
                new AnnonymousGameRequest { 
                    DifficultyLevel = DifficultyLevel.EASY 
                });
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Game created"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueMessageShouldCreateAnnonymousGamesFail()
        {
            // Arrange

            // Assert
            var actionResult = await sutFailure.CreateAnnonymousAsync(
                new AnnonymousGameRequest {
                    DifficultyLevel = DifficultyLevel.TEST
                });
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Difficulty not valid"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test]
        [Category("Controllers")]
        public void SuccessfullyCheckAnnonymousGames()
        {
            // Arrange

            // Assert
            var actionResult = sutSuccess.CheckAnnonymous(
                new AnnonymousCheckRequest
                {
                    FirstRow = [2, 9, 8, 1, 3, 4, 6, 7, 5],
                    SecondRow = [3, 1, 6, 5, 8, 7, 2, 9, 4],
                    ThirdRow = [4, 5, 7, 6, 9, 2, 1, 8, 3],
                    FourthRow = [9, 7, 1, 2, 4, 3, 5, 6, 8],
                    FifthRow = [5, 8, 3, 7, 6, 1, 4, 2, 9],
                    SixthRow = [6, 2, 4, 9, 5, 8, 3, 1, 7],
                    SeventhRow = [7, 3, 5, 8, 2, 6, 9, 4, 1],
                    EighthRow = [8, 4, 2, 3, 1, 9, 7, 5, 6],
                    NinthRow = [1, 6, 9, 4, 7, 5, 8, 3, 2]
                });
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Status Code 200: Game solved"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public void IssueMessageShouldCheckAnnonymousGamesFail()
        {
            // Arrange

            // Assert
            var actionResult = sutFailure.CheckAnnonymous(
                new AnnonymousCheckRequest
                {
                    FirstRow = [5, 9, 8, 1, 3, 4, 6, 7, 2],
                    SecondRow = [3, 1, 6, 5, 8, 7, 2, 9, 4],
                    ThirdRow = [4, 5, 7, 6, 9, 2, 1, 8, 3],
                    FourthRow = [9, 7, 1, 2, 4, 3, 5, 6, 8],
                    FifthRow = [5, 8, 3, 7, 6, 1, 4, 2, 9],
                    SixthRow = [6, 2, 4, 9, 5, 8, 3, 1, 7],
                    SeventhRow = [7, 3, 5, 8, 2, 6, 9, 4, 1],
                    EighthRow = [8, 4, 2, 3, 1, 9, 7, 5, 6],
                    NinthRow = [1, 6, 9, 4, 7, 5, 8, 3, 2]
                });
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Status Code 400: Game not solved"));
            Assert.That(statusCode, Is.EqualTo(400));
        }
    }
}
