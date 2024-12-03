using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SudokuCollective.Test.TestData;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Test.Services;
using SudokuCollective.Api.Utilities;
using SudokuCollective.Data.Models;
using System.Text.Json.Serialization;

namespace SudokuCollective.Test.TestCases.ControllerUtilitiesShould
{
    public class ControllerUtilitiesShould
    {
        private DatabaseContext context;
        private UsersController usersController;
        private MockedUsersService mockedUsersService;
        private MockedAppsService mockedAppsService;
        private MockedRequestService mockedRequestService;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<UsersController>> mockedLogger;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedUsersService = new MockedUsersService(context);
            mockedAppsService = new MockedAppsService(context);
            mockedRequestService = new MockedRequestService();
            mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<UsersController>>();

            usersController = new UsersController(
                mockedUsersService.SuccessfulRequest.Object,
                mockedAppsService.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object,
                mockWebHostEnvironment.Object);
        }

        [Test, Category("Utilities")]
        public void ProcessExceptions()
        {
            // Arrange
            var mockedRequestService = new MockedRequestService();
            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedException = new Mock<Exception>();
            mockedException.Setup(x => x.Message).Returns("An error occurred...");

            try
            {
                // Act
                var result = ControllerUtilities.ProcessException<UsersController>(
                    usersController,
                    mockedRequestService.SuccessfulRequest.Object,
                    mockedLogger.Object,
                    mockedException.Object);
                var statusCode = ((ObjectResult)result).StatusCode;

                // Assert
                Assert.That(result, Is.InstanceOf<ObjectResult>());
                Assert.That(statusCode, Is.EqualTo(500));
            }
            catch
            {
                Assert.That(false);
            }
        }

        [Test, Category("Utilities")]
        public void ProcessTokenError()
        {
            try
            {
                // Arrange and Act
                var result = ControllerUtilities.ProcessTokenError(usersController);
                var statusCode = ((ObjectResult)result).StatusCode;

                // Assert
                Assert.That(result, Is.InstanceOf<ObjectResult>());
                Assert.That(statusCode, Is.EqualTo(403));
            }
            catch
            {
                Assert.That(false);
            }
        }
    }

    internal class TestVar
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public TestVar()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        [JsonConstructor]
        public TestVar(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    internal class TestFailed
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public TestFailed()
        {
            Id = string.Empty;
            Message = string.Empty;
        }

        [JsonConstructor]
        public TestFailed(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}