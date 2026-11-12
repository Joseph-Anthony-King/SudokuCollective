using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Enums;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Services;

namespace SudokuCollective.Test.TestCases.Services
{
    public class AdminServiceShould
    {
        private Mock<ICacheService> mockedCacheService;
        private Mock<IRequestService> mockedRequestService;
        private Mock<ILogger<AdminService>> mockedLogger;
        private IAdminService sut;

        [SetUp]
        public void Setup()
        {
            mockedCacheService = new Mock<ICacheService>();
            mockedRequestService = new Mock<IRequestService>();
            mockedRequestService
                .Setup(service => service.Get())
                .Returns(new SudokuCollective.Logs.Models.Request());
            mockedLogger = new Mock<ILogger<AdminService>>();

            sut = CreateService(mockedCacheService.Object);
        }

        [Test, Category("Services")]
        public async Task GetSystemStatsAndValues()
        {
            SetupCachedUser(true);
            var adminSummaryResult = new AdminSummaryResult();
            var response = new RepositoryResponse
            {
                IsSuccess = true,
                Object = adminSummaryResult
            };

            mockedCacheService
                .Setup(cache => cache.GetSystemStatsAndValues(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IEmailConfirmationsRepository<EmailConfirmation>>(),
                    It.IsAny<IGamesRepository<Game>>(),
                    It.IsAny<IPasswordResetsRepository<PasswordReset>>(),
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(response, new Result()));

            var result = await sut.GetSystemStatsAndValues(1);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Payload, Has.Count.EqualTo(1));
            Assert.That(result.Payload[0], Is.SameAs(adminSummaryResult));
        }

        [Test, Category("Services")]
        public async Task ReturnFailureWhenRequestorIsNotSuperUser()
        {
            SetupCachedUser(false);

            var result = await sut.GetSystemStatsAndValues(2);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Requestor must be a super user to complete this request."));
            Assert.That(result.Payload, Is.Empty);
            mockedCacheService.Verify(cache => cache.GetSystemStatsAndValues(
                It.IsAny<IAppsRepository<App>>(),
                It.IsAny<IDifficultiesRepository<Difficulty>>(),
                It.IsAny<IEmailConfirmationsRepository<EmailConfirmation>>(),
                It.IsAny<IGamesRepository<Game>>(),
                It.IsAny<IPasswordResetsRepository<PasswordReset>>(),
                It.IsAny<IRolesRepository<Role>>(),
                It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                It.IsAny<IUsersRepository<User>>(),
                It.IsAny<IDistributedCache>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<IResult>(),
                It.IsAny<System.Net.Http.HttpMessageHandler>()), Times.Never);
        }

        [Test, Category("Services")]
        public async Task ReturnFailureWhenAdminSummaryIsNotFound()
        {
            SetupCachedUser(true);
            mockedCacheService
                .Setup(cache => cache.GetSystemStatsAndValues(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IEmailConfirmationsRepository<EmailConfirmation>>(),
                    It.IsAny<IGamesRepository<Game>>(),
                    It.IsAny<IPasswordResetsRepository<PasswordReset>>(),
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse { IsSuccess = false },
                    new Result()));

            var result = await sut.GetSystemStatsAndValues(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Admin summary not was found."));
            Assert.That(result.Payload, Is.Empty);
        }

        [Test, Category("Services")]
        public async Task ReturnFailureIfGetSystemStatsAndValuesThrows()
        {
            SetupCachedUser(true);
            mockedCacheService
                .Setup(cache => cache.GetSystemStatsAndValues(
                    It.IsAny<IAppsRepository<App>>(),
                    It.IsAny<IDifficultiesRepository<Difficulty>>(),
                    It.IsAny<IEmailConfirmationsRepository<EmailConfirmation>>(),
                    It.IsAny<IGamesRepository<Game>>(),
                    It.IsAny<IPasswordResetsRepository<PasswordReset>>(),
                    It.IsAny<IRolesRepository<Role>>(),
                    It.IsAny<ISolutionsRepository<SudokuSolution>>(),
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IResult>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var result = await sut.GetSystemStatsAndValues(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Test exception"));
            Assert.That(result.Payload, Is.Empty);
        }

        private AdminService CreateService(ICacheService cacheService)
        {
            return new AdminService(
                new Mock<IAppsRepository<App>>().Object,
                new Mock<IDifficultiesRepository<Difficulty>>().Object,
                new Mock<IEmailConfirmationsRepository<EmailConfirmation>>().Object,
                new Mock<IGamesRepository<Game>>().Object,
                new Mock<IPasswordResetsRepository<PasswordReset>>().Object,
                new Mock<IRolesRepository<Role>>().Object,
                new Mock<ISolutionsRepository<SudokuSolution>>().Object,
                new Mock<IUsersRepository<User>>().Object,
                mockedRequestService.Object,
                new Mock<IDistributedCache>().Object,
                cacheService,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
        }

        private void SetupCachedUser(bool isSuperUser)
        {
            var user = new User();
            if (isSuperUser)
            {
                user.Roles.Add(new UserRole
                {
                    Role = new Role(2, "Super User", RoleLevel.SUPERUSER)
                });
            }

            mockedCacheService
                .Setup(cache => cache.GetWithCacheAsync(
                    It.IsAny<IUsersRepository<User>>(),
                    It.IsAny<IDistributedCache>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<IResult>(),
                    It.IsAny<System.Net.Http.HttpMessageHandler>()))
                .ReturnsAsync(new Tuple<IRepositoryResponse, IResult>(
                    new RepositoryResponse
                    {
                        IsSuccess = true,
                        DomainObject = user
                    },
                    new Result()));
        }
    }
}
