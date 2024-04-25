using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.TestData;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Test.Services;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Test.Cache;
using SudokuCollective.Cache;

namespace SudokuCollective.Test.TestCases.Services
{
    public class AuthenticateServiceShould
    {
        private DatabaseContext context;
        private MockedUsersRepository mockedUsersRepository;
        private MockedRolesRepository mockedRolesRepository;
        private MockedUserManagementService mockedUserManagementService;
        private MockedAppsRepository mockedAppsRepository;
        private MockedAppAdminsRepository mockedAppAdminsRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private TokenManagement tokenManagement;
        private MemoryDistributedCache memoryCache;
        private Mock<ILogger<AuthenticateService>> mockedLogger;
        private IAuthenticateService sutValid;
        private IAuthenticateService sutInvalid;
        private string userName;
        private string password;

        [SetUp]
        public async Task Setup()
        {
            userName = "TestSuperUser";
            password = "T3stp4ssw@rd";

            context = await TestDatabase.GetDatabaseContext();

            mockedUsersRepository = new MockedUsersRepository(context);
            mockedRolesRepository = new MockedRolesRepository(context);
            mockedAppsRepository = new MockedAppsRepository(context);
            mockedAppAdminsRepository = new MockedAppAdminsRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));

            mockedUserManagementService = new MockedUserManagementService();
            mockedLogger = new Mock<ILogger<AuthenticateService>>();

            tokenManagement = new TokenManagement()
            {
                Secret = "3c1ad157-be37-40d2-9cc8-e7527a56aa7b",
                Issuer = "testProject",
                Audience = "testEnvironment",
                AccessExpiration = 24,
                RefreshExpiration = 24
            };

            sutValid = new AuthenticateService(
                mockedUsersRepository.SuccessfulRequest.Object,
                mockedRolesRepository.SuccessfulRequest.Object,
                mockedAppsRepository.SuccessfulRequest.Object,
                mockedAppAdminsRepository.SuccessfulRequest.Object,
                mockedUserManagementService.SuccssfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                tokenManagement,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
            sutInvalid = new AuthenticateService(
                mockedUsersRepository.FailedRequest.Object,
                mockedRolesRepository.FailedRequest.Object,
                mockedAppsRepository.FailedRequest.Object,
                mockedAppAdminsRepository.FailedRequest.Object,
                mockedUserManagementService.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                tokenManagement,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
        }

        [Test, Category("Services")]
        public void AuthenticateUsersIfValidated()
        {
            // Arrange
            var tokenRequest = new LoginRequest()
            {
                UserName = userName,
                Password = password
            };

            // Act
            var result = (sutValid.AuthenticateAsync(tokenRequest)).Result;

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("User found"));
            Assert.That(result.Payload[0], Is.Not.Null);
            Assert.That(userName, Is.EqualTo(((AuthenticationResult)result.Payload[0]).User.UserName));
        }

        [Test, Category("Services")]
        public void RejectUsersIfNotValidated()
        {
            // Arrange
            var tokenRequest = new LoginRequest()
            {
                UserName = userName,
                Password = password
            };

            // Act
            var result = (sutInvalid.AuthenticateAsync(tokenRequest)).Result;

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("User not found"));
        }
    }
}
