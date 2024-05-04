using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class RolesControllerShould
    {
        private DatabaseContext context;
        private RolesController sutSuccess;
        private RolesController sutFailure;
        private MockedRolesService mockedRolesService;
        private MockedAppsService mockedAppsService;
        private MockedRequestService mockedRequestService;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<RolesController>> mockedLogger;
        private Request request;
        private CreateRolePayload createRolePayload;
        private UpdateRolePayload updateRolePayload;
        private Mock<IWebHostEnvironment> mockedWebHostEnvironment;


        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRolesService = new MockedRolesService(context);
            mockedAppsService = new MockedAppsService(context);
            mockedRequestService = new MockedRequestService();
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<RolesController>>();
            mockedWebHostEnvironment = new Mock<IWebHostEnvironment>();

            request = TestObjects.GetRequest();
            
            updateRolePayload = TestObjects.GetUpdateRolePayload();
            
            createRolePayload = TestObjects.GetCreateRolePayload();

            sutSuccess = new RolesController(
                mockedRolesService.SuccessfulRequest.Object,
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);

            sutFailure = new RolesController(
                mockedRolesService.FailedRequest.Object,
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetRole()
        {
            // Arrange
            var roleId = 1;

            // Act
            var actionResult = await sutSuccess.GetAsync(roleId);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var role = (Role)result.Payload[0];
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Role found"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(role, Is.InstanceOf<Role>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetRoleFail()
        {
            // Arrange
            var roleId = 2;

            // Act
            var actionResult = await sutFailure.GetAsync(roleId);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Role not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetRoles()
        {
            // Arrange

            // Act
            var actionResult = await sutSuccess.GetRolesAsync( );
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Roles found"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetRolesFail()
        {
            // Arrange

            // Act
            var actionResult = await sutFailure.GetRolesAsync();
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Roles not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyUpdateRoles()
        {
            // Arrange
            request.Payload = updateRolePayload;

            // Act
            var actionResult = await sutSuccess.UpdateAsync(1, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Role updated"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldUpdateRoleFail()
        {
            // Arrange
            request.Payload = updateRolePayload;

            // Act
            var actionResult = await sutFailure.UpdateAsync(1, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Role not updated"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyCreateRoles()
        {
            // Arrange
            request.Payload = createRolePayload;

            // Act
            var actionResult = await sutSuccess.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 201: Role created"));
            Assert.That(statusCode, Is.EqualTo(201));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldCreateRolesFail()
        {
            // Arrange
            request.Payload = createRolePayload;

            // Act
            var actionResult = await sutFailure.PostAsync(request);
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: Role not created"));
            Assert.That(statusCode, Is.EqualTo(400));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyDeleteRoles()
        {
            // Arrange
            var roleId = 1;

            // Act
            var actionResult = await sutSuccess.DeleteAsync(roleId, request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 200: Role deleted"));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldDeleteRolesFail()
        {
            // Arrange
            var roleId = 1;

            // Act
            var actionResult = await sutFailure.DeleteAsync(roleId, request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 404: Role not deleted"));
            Assert.That(statusCode, Is.EqualTo(404));
        }
    }
}