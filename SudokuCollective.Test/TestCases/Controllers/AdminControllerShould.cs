using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Controllers.V1;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Controllers
{
    public class AdminControllerShould
    {
        private Mock<IAdminService> mockedAdminService;
        private Mock<IAppsService> mockedAppsService;
        private Mock<IRequestService> mockedRequestService;
        private Mock<ICacheService> mockedCacheService;
        private Mock<IUsersService> mockedUsersService;
        private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
        private Mock<ILogger<AdminController>> mockedLogger;
        private AdminController sut;
        private Request request;

        [SetUp]
        public void Setup()
        {
            mockedAdminService = new Mock<IAdminService>();
            mockedAppsService = new Mock<IAppsService>();
            mockedRequestService = new Mock<IRequestService>();
            mockedCacheService = new Mock<ICacheService>();
            mockedUsersService = new Mock<IUsersService>();
            mockedRequestService
                .Setup(service => service.Get())
                .Returns(new SudokuCollective.Logs.Models.Request());
            mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockedLogger = new Mock<ILogger<AdminController>>();
            request = new Request
            {
                License = TestObjects.GetLicense(),
                AppId = 1,
                RequestorId = 1
            };

            sut = new AdminController(
                mockedAdminService.Object,
                mockedAppsService.Object,
                mockedRequestService.Object,
                mockedCacheService.Object,
                mockedUsersService.Object,
                mockedHttpContextAccessor.Object,
                mockedLogger.Object);
        }

        [Test, Category("Controllers")]
        public async Task ReturnOkWhenAdminRequestSucceeds()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);
            mockedAdminService
                .Setup(service => service.GetSystemStatsAndValues(1))
                .ReturnsAsync(new Result
                {
                    IsSuccess = true,
                    Message = "System statistics retrieved."
                });

            var actionResult = await sut.PostAsync(request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)actionResult.Result).StatusCode, Is.EqualTo(200));
            Assert.That(result.Message, Is.EqualTo("Status Code 200: System statistics retrieved."));
            mockedRequestService.Verify(service => service.Update(request), Times.Once);
        }

        [Test, Category("Controllers")]
        public async Task ReturnNotFoundWhenAdminRequestFails()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);
            mockedAdminService
                .Setup(service => service.GetSystemStatsAndValues(1))
                .ReturnsAsync(new Result
                {
                    IsSuccess = false,
                    Message = "System statistics were not retrieved."
                });

            var actionResult = await sut.PostAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)actionResult.Result).StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Is.EqualTo("Status Code 404: System statistics were not retrieved."));
        }

        [Test, Category("Controllers")]
        public async Task ReturnForbiddenWhenTokenIsInvalid()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(false);

            var actionResult = await sut.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<ObjectResult>());
            Assert.That(((ObjectResult)actionResult.Result).StatusCode, Is.EqualTo(403));
            Assert.That(result.Message, Is.EqualTo("Status Code 403: Invalid request on this authorization token."));
            mockedAdminService.Verify(service => service.GetSystemStatsAndValues(1), Times.Never);
        }

        [Test, Category("Controllers")]
        public async Task ReturnBadRequestWhenRequestIsNull()
        {
            var actionResult = await sut.PostAsync(null);
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)actionResult.Result).StatusCode, Is.EqualTo(400));
            Assert.That(result.Message, Does.Contain("request"));
        }

        [Test, Category("Controllers")]
        public async Task ReturnInternalServerErrorWhenDependencyThrows()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var actionResult = await sut.PostAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<ObjectResult>());
            Assert.That(((ObjectResult)actionResult.Result).StatusCode, Is.EqualTo(500));
            Assert.That(result.Message, Is.EqualTo("Status Code 500: Test exception"));
        }

        [Test, Category("Controllers")]
        public async Task ReturnOkWhenFlushCacheSucceeds()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);
            mockedCacheService
                .Setup(service => service.FlushCacheAsync(
                    It.IsAny<Request>(),
                    It.IsAny<IUsersService>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ReturnsAsync(true);

            var actionResult = await sut.FlushCacheAsync(request);
            var result = (Result)((OkObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)actionResult.Result).StatusCode, Is.EqualTo(200));
            Assert.That(result.Message, Is.EqualTo("Status Code 200: Cache was flushed successfully."));
            mockedCacheService.Verify(service => service.FlushCacheAsync(
                request,
                mockedUsersService.Object,
                It.IsAny<string>(),
                It.IsAny<System.Net.Http.HttpMessageHandler>()), Times.Once);
        }

        [Test, Category("Controllers")]
        public async Task ReturnNotFoundWhenFlushCacheFails()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);
            mockedCacheService
                .Setup(service => service.FlushCacheAsync(
                    It.IsAny<Request>(),
                    It.IsAny<IUsersService>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ReturnsAsync(false);

            var actionResult = await sut.FlushCacheAsync(request);
            var result = (Result)((NotFoundObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)actionResult.Result).StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Is.EqualTo("Status Code 404: Cache was not flushed successfully."));
        }

        [Test, Category("Controllers")]
        public async Task ReturnForbiddenWhenTokenIsInvalidForFlushCache()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(false);

            var actionResult = await sut.FlushCacheAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<ObjectResult>());
            Assert.That(((ObjectResult)actionResult.Result).StatusCode, Is.EqualTo(403));
            Assert.That(result.Message, Is.EqualTo("Status Code 403: Invalid request on this authorization token."));
            mockedCacheService.Verify(service => service.FlushCacheAsync(
                It.IsAny<Request>(),
                It.IsAny<IUsersService>(),
                It.IsAny<string>(),
                It.IsAny<System.Net.Http.HttpMessageHandler>()), Times.Never);
        }

        [Test, Category("Controllers")]
        public async Task ReturnBadRequestWhenFlushCacheRequestIsNull()
        {
            var actionResult = await sut.FlushCacheAsync(null);
            var result = (Result)((BadRequestObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)actionResult.Result).StatusCode, Is.EqualTo(400));
            Assert.That(result.Message, Does.Contain("request"));
        }

        [Test, Category("Controllers")]
        public async Task ReturnInternalServerErrorWhenFlushCacheDependencyThrows()
        {
            mockedAppsService
                .Setup(service => service.IsRequestValidOnThisTokenAsync(
                    It.IsAny<IHttpContextAccessor>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);
            mockedCacheService
                .Setup(service => service.FlushCacheAsync(
                    It.IsAny<Request>(),
                    It.IsAny<IUsersService>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ThrowsAsync(new InvalidOperationException("Flush failed"));

            var actionResult = await sut.FlushCacheAsync(request);
            var result = (Result)((ObjectResult)actionResult.Result).Value;

            Assert.That(actionResult.Result, Is.TypeOf<ObjectResult>());
            Assert.That(((ObjectResult)actionResult.Result).StatusCode, Is.EqualTo(500));
            Assert.That(result.Message, Is.EqualTo("Status Code 500: Flush failed"));
        }
    }
}
