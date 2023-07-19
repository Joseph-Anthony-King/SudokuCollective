using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Repos;
using SudokuCollective.Test.Services;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Repositories
{
    public class DifficultiesRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<DifficultiesRepository<Difficulty>>> mockedLogger;
        private IDifficultiesRepository<Difficulty> sut;
        private Difficulty newDifficutly;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<DifficultiesRepository<Difficulty>>>();

            sut = new DifficultiesRepository<Difficulty>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            newDifficutly = new Difficulty()
            {
                Name = "New Test",
                DisplayName = "New Test",
                DifficultyLevel = DifficultyLevel.TEST
            };
        }

        [Test, Category("Repository")]
        public async Task CreateDifficulties()
        {
            // Arrange
            var testDifficulty = context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST);
            context.Difficulties.Remove(testDifficulty);
            context.SaveChanges();

            // Act
            var result = await sut.AddAsync(newDifficutly);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Difficulty)result.Object, Is.InstanceOf<Difficulty>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfCreateDifficutliesFails()
        {
            // Arrange and Act
            var result = await sut.AddAsync(newDifficutly);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetDifficultiesById()
        {
            // Arrange and Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Difficulty)result.Object, Is.InstanceOf<Difficulty>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetByIdFails()
        {
            // Arrange and Act
            var result = await sut.GetAsync(7);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task GetAllDifficulties()
        {
            // Arrange and Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(d => (Difficulty)d), Is.InstanceOf<List<Difficulty>>());
        }

        [Test, Category("Repository")]
        public async Task UpdateDifficulties()
        {
            // Arrange
            var difficulty = context.Difficulties.FirstOrDefault(d => d.Id == 1);
            difficulty.Name = string.Format("{0} UPDATED!", difficulty.Name);

            // Act
            var result = await sut.UpdateAsync(difficulty);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Object, Is.InstanceOf<Difficulty>());
            Assert.That(((Difficulty)result.Object).Name, Is.EqualTo(difficulty.Name));
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfUpdateDifficultiesFails()
        {
            // Arrange and Act
            var result = await sut.UpdateAsync(newDifficutly);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Object, Is.Null);
        }

        [Test, Category("Repository")]
        public async Task DeleteDifficulties()
        {
            // Arrange
            var difficulty = context.Difficulties.FirstOrDefault(d => d.Id == 1);

            // Act
            var result = await sut.DeleteAsync(difficulty);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteDifficultiesFails()
        {
            // Arrange and Act
            var result = await sut.DeleteAsync(newDifficutly);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnDifficulty()
        {
            // Arrange and Act
            var result = await sut.HasEntityAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAnDifficultyFails()
        {
            // Arrange
            var id = context
                .Difficulties
                .ToList()
                .OrderBy(d => d.Id)
                .Last<Difficulty>()
                .Id + 1;

            // Act
            var result = await sut.HasEntityAsync(id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAnDifficultyLevel()
        {
            // Arrange and Act
            var result = await sut.HasDifficultyLevelAsync(DifficultyLevel.TEST);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAnDifficultyLevelFails()
        {
            // Arrange
            var testDifficulty = context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST);
            context.Difficulties.Remove(testDifficulty);
            context.SaveChanges();

            // Act
            var result = await sut.HasDifficultyLevelAsync(DifficultyLevel.TEST);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
