using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Cache;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Services
{
    public class UserManagementServiceShould
    {
        private DatabaseContext context;
        private MockedUsersRepository mockedUsersRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private MemoryDistributedCache memoryCache;
        private Mock<ILogger<UserManagementService>> mockedLogger;
        private IUserManagementService sut;
        private IUserManagementService sutFailure;
        private string userName;
        private string password;
        private string email;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedUsersRepository = new MockedUsersRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));
            mockedLogger = new Mock<ILogger<UserManagementService>>();

            sut = new UserManagementService(
                mockedUsersRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
            sutFailure = new UserManagementService(
                mockedUsersRepository.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.FailedRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);

            userName = "TestSuperUser";
            password = "T3stPass0rd?1";
            email = "TestSuperUser@example.com";
        }

        [Test, Category("Services")]
        public async Task ConfirmUserIfValid()
        {
            // Arrange

            // Act
            var result = await sut.IsValidUserAsync(userName, password);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Services")]
        public async Task DenyUserIfUserNameInvalid()
        {
            // Arrange
            var invalidUserName = "invalidUser";

            // Act
            var result = await sutFailure.IsValidUserAsync(invalidUserName, password);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Services")]
        public async Task DenyUserIfPasswordInvalid()
        {
            // Arrange
            var invalidPassword = "invalidPassword";

            // Act
            var result = await sut.IsValidUserAsync(userName, invalidPassword);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Services")]
        public async Task ReturnUserAuthenticationErrorTypeIfUserInvalid()
        {
            // Arrange
            var invalidUserName = "invalidUser";
            var license = TestObjects.GetLicense();

            // Act
            var result = await sutFailure.ConfirmAuthenticationIssueAsync(
                invalidUserName,
                password,
                license);

            // Assert
            Assert.That(result, Is.EqualTo(UserAuthenticationErrorType.USERNAMEINVALID));
        }

        [Test, Category("Services")]
        public async Task ReturnPasswordAuthenticationErrorTypeIfPasswordInvalid()
        {
            // Arrange
            var invalidPassword = "invalidPassword";
            var license = TestObjects.GetLicense();

            // Act
            var result = await sut.ConfirmAuthenticationIssueAsync(
                userName,
                invalidPassword,
                license);

            // Assert
            Assert.That(result, Is.EqualTo(UserAuthenticationErrorType.PASSWORDINVALID));
        }

        [Test, Category("Services")]
        public async Task ReturnUserNameIfEmailIsValid()
        {
            // Arrange

            // Act
            var result = await sut.ConfirmUserNameAsync(
                email,
                TestObjects.GetLicense());
            var success = result.IsSuccess;
            var username = ((AuthenticatedUserNameResult)result.Payload[0]).UserName;

            // Assert
            Assert.That(success, Is.True);
            Assert.That((AuthenticatedUserNameResult)result.Payload[0], Is.InstanceOf<AuthenticatedUserNameResult>());
            Assert.That(username, Is.EqualTo("TestSuperUser"));
        }

        [Test, Category("Services")]
        public async Task ReturnMessageIfUserNameInvalid()
        {
            // Arrange

            // Act
            var result = await sutFailure.ConfirmUserNameAsync(
                "invalidEmail@example.com",
                TestObjects.GetLicense());
            var success = result.IsSuccess;
            var message = result.Message;

            // Assert
            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("No user is using this email"));
        }
    }
}
