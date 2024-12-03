using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.Services;

namespace SudokuCollective.Test.TestCases.Controllers
{
    internal class JobsControllerShould
    {
        private JobsController sut;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<JobsController>> mockedLogger;
        private Mock<IMonitoringApi> mockedMonitoringApi;
        private Mock<IStorageConnection> mockedStorageConnection;

        [SetUp]
        public void Setup()
        {
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<JobsController>>();
            mockedMonitoringApi = new Mock<IMonitoringApi>();
            mockedStorageConnection = new Mock<IStorageConnection>();

            sut = new JobsController(
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object,
                mockedMonitoringApi.Object,
                mockedStorageConnection.Object);
        }

        [Test, Category("Controllers")]
        public void SuccessfullyPollBackgroundJobs()
        {
            // Arrange
            mockedStorageConnection
                .Setup(connection => connection.GetJobData(It.IsAny<string>()))
                .Returns(new JobData()
                {
                    State = "Processing"
                });

            // Act
            var actionResult = sut.Poll("5d74fa7b-db93-4213-8e0c-da2f3179ed05");
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Status Code 200: Job 5d74fa7b-db93-4213-8e0c-da2f3179ed05 is not completed with status processing."));
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test, Category("Controllers")]
        public void ReturnTrueIfTheJobStatusIsSucceeded()
        {
            // Arrange
            mockedStorageConnection
                .Setup(connection => connection.GetJobData(It.IsAny<string>()))
                .Returns(new JobData()
                {
                    State = "Succeeded"
                });

            // Act
            var actionResult = sut.Poll("5d74fa7b-db93-4213-8e0c-da2f3179ed05");
            var result = (Result)((ObjectResult)actionResult.Result).Value;
            var success = result.IsSuccess;
            var message = result.Message;
            var statusCode = ((ObjectResult)actionResult.Result).StatusCode;

            // Assert
            Assert.That(actionResult, Is.TypeOf<ActionResult<Result>>());
            Assert.That(result, Is.TypeOf<Result>());
            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Status Code 200: Job 5d74fa7b-db93-4213-8e0c-da2f3179ed05 is completed with status succeeded."));
            Assert.That(statusCode, Is.EqualTo(200));
        }
    }
}
