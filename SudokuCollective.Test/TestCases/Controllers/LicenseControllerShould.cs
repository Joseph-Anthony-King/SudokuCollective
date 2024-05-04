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
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class LicenseControllerShould
    {
        private DatabaseContext context;
        private LicensesController sutSuccess;
        private LicensesController sutFailure;
        private MockedAppsService mockedAppsService;
        private MockedRequestService mockedRequestService;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<LicensesController>> mockedLogger;
        private Mock<IWebHostEnvironment> mockedWebHostEnvironment;
        private Request request;
        private LicensePayload licensePayload;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedAppsService = new MockedAppsService(context);
            mockedRequestService = new MockedRequestService();
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<LicensesController>>();
            mockedWebHostEnvironment = new Mock<IWebHostEnvironment>();

            request = TestObjects.GetRequest();

            licensePayload = TestObjects.GetLicensePayload();

            sutSuccess = new LicensesController(
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);
            sutFailure = new LicensesController(
                mockedAppsService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockedWebHostEnvironment.Object);
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyGetLicense()
        {
            // Arrange
            var appId = 1;

            // Act
            var actionResult = await sutSuccess.GetAsync(appId, request);
            var result = (LicenseResult)((OkObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var license = result.License;
            var statusCode = ((OkObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<LicenseResult>());
            Assert.That(message, Is.EqualTo("Status Code 200: App found"));
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(license, Is.InstanceOf<string>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldGetLicenseFail()
        {
            // Arrange
            var appId = 1;

            // Act
            var actionResult = await sutFailure.GetAsync(appId, request);
            var result = (LicenseResult)((NotFoundObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((NotFoundObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<LicenseResult>());
            Assert.That(message, Is.EqualTo("Status Code 404: App not found"));
            Assert.That(statusCode, Is.EqualTo(404));
        }

        [Test]
        [Category("Controllers")]
        public async Task SuccessfullyPostLicenses()
        {
            // Arrange
            request.Payload = licensePayload;

            // Act
            var actionResult = await sutSuccess.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var app = (App)result.Payload[0];
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 201: App created"));
            Assert.That(statusCode, Is.EqualTo(201));
            Assert.That(app, Is.InstanceOf<App>());
        }

        [Test]
        [Category("Controllers")]
        public async Task IssueErrorAndMessageShouldPostLicenseFail()
        {
            // Arrange
            request.Payload = licensePayload;

            // Act
            var actionResult = await sutFailure.PostAsync(request);
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;
            var message = result.Message;
            var statusCode = ((BadRequestObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ActionResult<Result>>());
            Assert.That(result, Is.InstanceOf<Result>());
            Assert.That(message, Is.EqualTo("Status Code 400: App not created"));
            Assert.That(statusCode, Is.EqualTo(400));

        }
    }
}
