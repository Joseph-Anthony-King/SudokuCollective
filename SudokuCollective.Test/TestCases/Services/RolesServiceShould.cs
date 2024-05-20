using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Cache;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Services
{
    public class RolesServiceShould
    {
        private DatabaseContext context;
        private MockedRolesRepository mockedkRolesRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private MemoryDistributedCache memoryCache;
        private Mock<ILogger<RolesService>> mockedLogger;
        private RolesService sut;
        private RolesService sutFailue;
        private Request request;
        private UpdateRolePayload updateRolePayload;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedkRolesRepository = new MockedRolesRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));
            mockedLogger = new Mock<ILogger<RolesService>>();

            sut = new RolesService(
                mockedkRolesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
            sutFailue = new RolesService(
                mockedkRolesRepository.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.FailedRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
        }

        [Test, Category("Services")]
        public async Task GetARole()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Role was found."));
            Assert.That((Role)result.Payload[0], Is.TypeOf<Role>());
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfRoleNotFound()
        {
            // Arrange

            // Act
            var result = await sutFailue.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Role was not found."));
        }

        [Test, Category("Services")]
        public async Task GetRoles()
        {
            // Arrange

            // Act
            var result = await sut.GetRolesAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Roles were found."));
            Assert.That(result.Payload.ConvertAll(r => (IRole)r), Is.TypeOf<List<IRole>>());
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfRolesNotFound()
        {
            // Arrange

            // Act
            var result = await sutFailue.GetRolesAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Roles were not found."));
        }

        [Test, Category("Services")]
        public async Task GetRolesWithoutNullOrSuperUserRoleLevel()
        {
            // Arrange

            // Act
            var result = await sut.GetRolesAsync();
            var nullAndSuperUserRoleLevelsBlocked = result.Payload
                .ConvertAll(r => (Role)r)
                .Any(role =>
                    role.RoleLevel.Equals(RoleLevel.NULL)
                    || role.RoleLevel.Equals(RoleLevel.SUPERUSER));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Roles were found."));
            Assert.That(nullAndSuperUserRoleLevelsBlocked, Is.False);
        }

        [Test, Category("Services")]
        public async Task UpdateARole()
        {
            // Arrange
            request = TestObjects.GetRequest();
            updateRolePayload = TestObjects.GetUpdateRolePayload();
            request.Payload = updateRolePayload;

            // Act
            var result = await sut.UpdateAsync(1, request);
            var updatedDifficulty = context.Roles
                .FirstOrDefault(role => role.Id == 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Role was updated."));
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfUpdateFails()
        {
            // Arrange
            request = TestObjects.GetRequest();
            updateRolePayload = TestObjects.GetInvalidUpdateRolePayload();
            request.Payload = updateRolePayload;

            // Act
            var result = await sutFailue.UpdateAsync(1, request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Role was not found."));
        }

        [Test, Category("Services")]
        public async Task DeleteADifficulty()
        {
            // Arrange

            // Act
            var result = await sut.DeleteAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Role was deleted."));
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfRoleNotDeleted()
        {
            // Arrange

            // Act
            var result = await sutFailue.DeleteAsync(10);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Role was not found."));
        }
    }
}
