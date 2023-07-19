using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class SolutionsRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<SolutionsRepository<SudokuSolution>>> mockedLogger;
        private ISolutionsRepository<SudokuSolution> sut;
        private SudokuSolution newSolution;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<SolutionsRepository<SudokuSolution>>>();

            sut = new SolutionsRepository<SudokuSolution>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newSolution = new SudokuSolution();
        }

        [Test, Category("Repository")]
        public async Task CreateSolutionsAsync()
        {
            // Arrange

            // Act
            var result = await sut.AddAsync(newSolution);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((SudokuSolution)result.Object, Is.InstanceOf<SudokuSolution>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfCreateSolutionsAsyncFails()
        {
            // Arrange
            newSolution.Id = 10;

            // Act
            var result = await sut.AddAsync(newSolution);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetSolutionsByIdAsync()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((SudokuSolution)result.Object, Is.InstanceOf<SudokuSolution>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByIdAsyncFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(5);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllSolutionsAsync()
        {
            // Arrange

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(s => (SudokuSolution)s), Is.InstanceOf<List<SudokuSolution>>());
        }

        [Test, Category("Repository")]
        public async Task AddSolutions()
        {
            // Arrange
            var solutions = new List<SudokuSolution>();
            solutions.Add(newSolution);

            // Act
            var result = await sut.AddSolutionsAsync(solutions.ConvertAll(s => (ISudokuSolution)s));

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task GetSolvedSolutions()
        {
            // Arrange

            // Act
            var result = await sut.GetSolvedSolutionsAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(s => (SudokuSolution)s), Is.InstanceOf<List<SudokuSolution>>());
        }
    }
}
