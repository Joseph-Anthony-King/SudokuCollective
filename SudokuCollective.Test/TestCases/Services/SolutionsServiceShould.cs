using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Jobs;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Services;
using SudokuCollective.Test.Cache;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Models.Requests;

namespace SudokuCollective.Test.TestCases.Services
{
    public class SolutionsServiceShould
    {
        private DatabaseContext context;
        private MockedSolutionsRepository mockedSolutionsRepository;
        private MockedRequestService mockedRequestService;
        private MockedCacheService mockedCacheService;
        private MemoryDistributedCache memoryCache;
        private Mock<IBackgroundJobClient> mockedJobClient;
        private Mock<IDataJobs> mockedDataJobs;
        private Mock<ILogger<SolutionsService>> mockedLogger;
        private ISolutionsService sut;
        private ISolutionsService sutFailure;
        private Request request;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedSolutionsRepository = new MockedSolutionsRepository(context);
            mockedRequestService = new MockedRequestService();
            mockedCacheService = new MockedCacheService(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));

            mockedJobClient = new Mock<IBackgroundJobClient>();
            mockedJobClient.Verify(client =>
                client.Create(
                    It.IsAny<Job>(),
                    It.IsAny<EnqueuedState>()), Times.Never);

            mockedDataJobs = new Mock<IDataJobs>();
            mockedDataJobs.Setup(jobs =>
                jobs.AddSolutionJobAsync(It.IsAny<List<int>>()));

            mockedLogger = new Mock<ILogger<SolutionsService>>();

            sut = new SolutionsService(
                mockedSolutionsRepository.SuccessfulRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.SuccessfulRequest.Object,
                new CacheKeys(),
                mockedJobClient.Object,
                mockedDataJobs.Object,
                mockedLogger.Object);

            sutFailure = new SolutionsService(
                mockedSolutionsRepository.FailedRequest.Object,
                mockedRequestService.SuccessfulRequest.Object,
                memoryCache,
                mockedCacheService.FailedRequest.Object,
                new CacheKeys(),
                mockedJobClient.Object,
                mockedDataJobs.Object,
                mockedLogger.Object);

            request = TestObjects.GetRequest();
        }

        [Test, Category("Services")]
        public async Task GetASolution()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Solution found"));
            Assert.That((SudokuSolution)result.Payload[0], Is.TypeOf<SudokuSolution>());
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfGetSolutionFails()
        {
            // Arrange

            // Act
            var result = await sutFailure.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Solution not found"));
        }

        [Test, Category("Services")]
        public async Task GetSolutions()
        {
            // Arrange

            // Act
            var result = await sut.GetSolutionsAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Solutions found"));
            Assert.That(result.Payload.ConvertAll(s => (ISudokuSolution)s), Is.TypeOf<List<ISudokuSolution>>());
        }

        [Test, Category("Services")]
        public async Task IssueMessageIfGetSolutionsFails()
        {
            // Arrange

            // Act
            var result = await sutFailure.GetSolutionsAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Solutions not found"));
        }

        [Test, Category("Services")]
        public async Task SolveSudokuMatrices()
        {
            // Arrange
            var solveRequest = new AnnonymousCheckRequest()
            {
                FirstRow = [0, 2, 0, 5, 0, 0, 8, 7, 6],
                SecondRow = [7, 0, 0, 1, 8, 0, 0, 5, 0],
                ThirdRow = [8, 5, 9, 7, 0, 0, 0, 4, 0],
                FourthRow = [5, 9, 0, 0, 0, 4, 6, 8, 1],
                FifthRow = [0, 1, 0, 0, 3, 0, 0, 0, 0],
                SixthRow = [0, 0, 0, 8, 6, 0, 0, 9, 5],
                SeventhRow = [2, 0, 7, 0, 0, 8, 0, 0, 9],
                EighthRow = [9, 0, 4, 0, 0, 7, 2, 0, 8],
                NinthRow = [0, 0, 0, 0, 0, 2, 4, 6, 0]
            };

            // Act
            var result = await sut.SolveAsync(solveRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Sudoku solution found"));
            Assert.That((AnnonymousGameResult)result.Payload[0], Is.TypeOf<AnnonymousGameResult>());
        }

        [Test, Category("Services")]
        public async Task GenerateASolution()
        {
            // Arrange

            // Act
            var result = await sut.GenerateAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Solution generated"));
            Assert.That((AnnonymousGameResult)result.Payload[0], Is.TypeOf<AnnonymousGameResult>());
        }

        [Test, Category("Services")]
        public void AddSolutions()
        {
            // Arrange
            request.Payload = new AddSolutionsPayload { Limit = 10 };

            // Act
            var result = sut.GenerateSolutions(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Solutions added"));
        }

        [Test, Category("Services")]
        public void IssueMessageIfAddSolutionsFails()
        {
            // Arrange
            request.Payload = new AddSolutionsPayload { Limit = 1001 };

            // Act
            var result = sutFailure.GenerateSolutions(request);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("The amount of solutions requested, 1001, Exceeds the service's 1,000 limit"));
        }
    }
}
