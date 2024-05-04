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
using SudokuCollective.Data.Models.Params;
using IResult = SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IResult;
using System.Net.Http;
using Moq.Protected;
using System.Net;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Data.Models.Results;
using StackExchange.Redis;
using SudokuCollective.Heroku.Models;

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
        public async Task ProcessExceptions()
        {
            // Arrange
            var mockedRequestService = new MockedRequestService();
            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedException = new Mock<Exception>();
            mockedException.Setup(x => x.Message).Returns("An error occurred...");

            try
            {
                // Act
                var result = await ControllerUtilities.ProcessException<UsersController>(
                    usersController,
                    mockedRequestService.SuccessfulRequest.Object,
                    mockedLogger.Object,
                    mockedException.Object,
                    mockWebHostEnvironment.Object);
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
        public async Task ProcessExceptionsCanHandleHerokuIOExceptions()
        {
            // Arrange
            var mockedRequestService = new MockedRequestService();
            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedException = new Mock<Exception>();
            mockedException.Setup(x => x.Message).Returns("It was not possible to connect to the redis server(s). There was an authentication failure; check that passwords (or client certificates) are configured correctly: (IOException) Unable to read data from the transport connection: Connection aborted.");
            var mockedEnvironment = new Mock<IWebHostEnvironment>();
            mockedEnvironment.Setup(env => env.EnvironmentName).Returns("Production");
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                });

            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUREDIS", "sudokucollective-prod-cache");

            try
            {
                // Act
                var result = await ControllerUtilities.ProcessException<UsersController>(
                    usersController,
                    mockedRequestService.SuccessfulRequest.Object,
                    mockedLogger.Object,
                    mockedException.Object,
                    mockedEnvironment.Object,
                    mockedMessageHandler.Object);
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

        [Test, Category("Utilities")]
        public async Task InterceptHerokuIOExceptions()
        {
            // Arrange and Act
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {  
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                });

            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedEnvironment = new Mock<IWebHostEnvironment>();

            mockedEnvironment.Setup(env => env.EnvironmentName).Returns("Production");

            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUREDIS", "sudokucollective-prod-cache");

            var result = new Result()
            {
                IsSuccess = false,
                IsFromCache = false,
                Message = "It was not possible to connect to the redis server(s). There was an authentication failure; check that passwords (or client certificates) are configured correctly: (IOException) Unable to read data from the transport connection: Connection aborted."
            };

            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions<UsersController>(
                result,
                mockedEnvironment.Object,
                mockedLogger.Object,
                mockedMessageHandler.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<IResult>());
            Assert.That(result.Message, Is.EqualTo("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."));
        }

        [Test, Category("Utilities")]
        public async Task InterceptHerokuIOExceptionsProducesErrorMessageIfHttpClientFails()
        {
            // Arrange and Act
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            using StringContent body = new(
                JsonSerializer.Serialize<TestFailed>(new() 
                    { 
                        Id = "Unauthorized", 
                        Message = "Request was unauthorized"
                    }),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = body
                });

            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedEnvironment = new Mock<IWebHostEnvironment>();

            mockedEnvironment.Setup(env => env.EnvironmentName).Returns("Production");

            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUREDIS", "sudokucollective-prod-cache");

            var result = new Result()
            {
                IsSuccess = false,
                IsFromCache = false,
                Message = "It was not possible to connect to the redis server(s). There was an authentication failure; check that passwords (or client certificates) are configured correctly: (IOException) Unable to read data from the transport connection: Connection aborted."
            };

            result = (Result)await ControllerUtilities.InterceptHerokuIOExceptions<UsersController>(
                result,
                mockedEnvironment.Object,
                mockedLogger.Object,
                mockedMessageHandler.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<IResult>());
            Assert.That(result.Message, Is.EqualTo("It was not possible to connect to the redis server, the attempt to restart the redis server connections failed. Please resubmit your request."));
        }

        [Test, Category("Utilities")]
        public async Task InterceptHerokuIOExceptionsForILicenseResults()
        {
            // Arrange and Act
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                });

            var mockedLogger = new Mock<ILogger<UsersController>>();
            var mockedEnvironment = new Mock<IWebHostEnvironment>();

            mockedEnvironment.Setup(env => env.EnvironmentName).Returns("Production");

            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:PROD:HEROKUREDIS", "sudokucollective-prod-cache");

            var result = new LicenseResult()
            {
                IsSuccess = false,
                IsFromCache = false,
                Message = "It was not possible to connect to the redis server(s). There was an authentication failure; check that passwords (or client certificates) are configured correctly: (IOException) Unable to read data from the transport connection: Connection aborted."
            };

            result = (LicenseResult)await ControllerUtilities.InterceptHerokuIOExceptions<UsersController>(
                result,
                mockedEnvironment.Object,
                mockedLogger.Object,
                mockedMessageHandler.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<IResult>());
            Assert.That(result.Message, Is.EqualTo("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."));
        }
    }

    public class TestVar
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
    public class TestFailed
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}