using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.TestData;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Test.Cache;
using SudokuCollective.Cache;
using SudokuCollective.Test.Services;

namespace SudokuCollective.Test.TestCases.Services
{
    public class DifficultiesServiceShould
    {
        private DatabaseContext context;
        private MockedDifficultiesRepository mockedDifficultiesRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private MemoryDistributedCache memoryCache;
        private Mock<ILogger<DifficultiesService>> mockedLogger;
        private IDifficultiesService sut;
        private IDifficultiesService sutCreateDifficulty;
        private string license;
        private Paginator paginator;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedDifficultiesRepository = new MockedDifficultiesRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));
            mockedLogger = new Mock<ILogger<DifficultiesService>>();

            sut = new DifficultiesService(
                mockedDifficultiesRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
            sutCreateDifficulty = new DifficultiesService(
                mockedDifficultiesRepository.CreateDifficultyRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.CreateDifficultyRoleSuccessfulRequest.Object,
                new CacheKeys(),
                new CachingStrategy(),
                mockedLogger.Object);
            license = TestObjects.GetLicense();
            paginator = TestObjects.GetPaginator();
        }

        [Test, Category("Services")]
        public async Task GetADifficulty()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(3);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulty found"));
            Assert.That((Difficulty)result.Payload[0], Is.TypeOf<Difficulty>());
        }

        [Test, Category("Services")]
        public async Task FailGetADifficultyOfIdOneOrTwo()
        {
            // Arrange

            // Act
            var resultOne = await sut.GetAsync(1);
            var resultTwo = await sut.GetAsync(2);

            // Assert
            Assert.That(resultOne.IsSuccess, Is.False);
            Assert.That(resultTwo.IsSuccess, Is.False);
            Assert.That(resultOne.Message, Is.EqualTo("Null and test difficulties are not available through the api"));
            Assert.That(resultTwo.Message, Is.EqualTo("Null and test difficulties are not available through the api"));
        }

        [Test, Category("Services")]
        public async Task GetDifficulties()
        {
            // Arrange

            // Act
            var result = await sut.GetDifficultiesAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulties found"));
            Assert.That(result.Payload.Count, Is.EqualTo(4));
        }

        [Test, Category("Services")]
        public async Task GetDifficultiesWithoutNullOrTestDifficultyLevel()
        {
            // Arrange

            // Act
            var result = await sut.GetDifficultiesAsync();
            var nullAndTestDifficultyLevelsBlocked = result.Payload
                .ConvertAll(d => (Difficulty)d)
                .Any(difficulty =>
                    difficulty.DifficultyLevel.Equals(DifficultyLevel.NULL)
                    || difficulty.DifficultyLevel.Equals(DifficultyLevel.TEST));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulties found"));
            Assert.That(nullAndTestDifficultyLevelsBlocked, Is.False);
        }

        [Test, Category("Services")]
        public async Task UpdateADifficulty()
        {
            // Arrange
            var request = new Request
            {
                License = license,
                RequestorId = 1,
                Paginator = paginator,
                Payload = new UpdateDifficultyPayload()
                {
                    Id = 1,
                    Name = "Medium UPDATED!",
                }
            };

            // Act
            var result = await sut.UpdateAsync(1, request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulty updated"));
            Assert.That(((Difficulty)result.Payload[0]).Name, Is.EqualTo("Medium UPDATED!"));
        }

        [Test, Category("Services")]
        public async Task DeleteADifficulty()
        {
            // Arrange

            // Act
            var result = await sut.DeleteAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulty deleted"));
        }

        [Test, Category("Services")]
        public async Task CreateADifficulty()
        {
            // Arrange
            var request = new Request
            {
                License = license,
                RequestorId = 1,
                Paginator = new Paginator(),
                Payload = new CreateDifficultyPayload()
                {
                    Name = "new-difficulty",
                    DisplayName = "New Difficulty",
                    DifficultyLevel = DifficultyLevel.NULL
                }
            };

            // Act
            var result = await sutCreateDifficulty.CreateAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Difficulty created"));
            Assert.That((Difficulty)result.Payload[0], Is.InstanceOf<Difficulty>());
        }
    }
}
