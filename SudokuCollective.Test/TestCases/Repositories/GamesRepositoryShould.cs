using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    public class GamesRepositoryShould
    {
        private DatabaseContext context;
        private MockedRequestService mockedRequestService;
        private Mock<ILogger<GamesRepository<Game>>> mockedLogger;
        private IGamesRepository<Game> sut;
        private Game newGame;
        private User user;
        private SudokuMatrix matrix;
        private Difficulty difficulty;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedRequestService = new MockedRequestService();
            mockedLogger = new Mock<ILogger<GamesRepository<Game>>>();
            
            sut = new GamesRepository<Game>(
                context,
                mockedRequestService.SuccessfulRequest.Object,
                mockedLogger.Object);

            user = context
                .Users
                .FirstOrDefault(u => u.Id == 1);

            matrix = new SudokuMatrix();

            difficulty = context
                .Difficulties
                .FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.MEDIUM);

            newGame = new Game(user, matrix, difficulty, 1);
        }

        [Test, Category("Repository")]
        public async Task CreateGames()
        {
            // Arrange
            newGame.SudokuMatrix.GenerateSolution();

            // Act
            var result = await sut.AddAsync(newGame);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Game)result.Object, Is.InstanceOf<Game>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFaliseIfCreateGamesFail()
        {
            // Arrange
            newGame.Id = 10;

            // Act
            var result = await sut.AddAsync(newGame);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetGamesById()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Game)result.Object, Is.InstanceOf<Game>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetGamesByIdFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAsync(3);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetAllGames()
        {
            // Arrange

            // Act
            var result = await sut.GetAllAsync();

            // Arrange
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(g => (Game)g), Is.InstanceOf<List<Game>>());
        }

        [Test, Category("Repository")]
        public async Task UpdateGames()
        {
            // Arrange
            var game = context
                .Games
                    .Include(g => g.SudokuMatrix)
                        .ThenInclude(m => m.SudokuCells)
                    .Include(g => g.SudokuSolution)
                .FirstOrDefault(g => g.Id == 1);

            // Act
            var result = await sut.UpdateAsync(game);

            // Arrange
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Game)result.Object, Is.InstanceOf<Game>());
            Assert.That(((Game)result.Object).DateUpdated, Is.GreaterThan(DateTime.MinValue));

        }

        [Test, Category("Repository")]
        public async Task ReturnsFalseIfUpdateGamesFails()
        {
            // Arrange

            // Act
            var result = await sut.UpdateAsync(newGame);

            // Arrange
            Assert.That(result.IsSuccess, Is.False);

        }

        [Test, Category("Repository")]
        public async Task DeleteGames()
        {
            // Arrange
            var game = context.Games
                .Include(g => g.User)
                    .ThenInclude(u => u.Apps)
                .Include(g => g.User)
                    .ThenInclude(u => u.Roles)
                .Include(g => g.SudokuMatrix)
                    .ThenInclude(m => m.SudokuCells)
                .Include(g => g.SudokuSolution)
                .FirstOrDefault(g => g.Id == 1);

            foreach (var userApp in game.User.Apps)
            {
                userApp.App = context
                    .Apps
                    .FirstOrDefault(a => a.Id == userApp.AppId);
            }

            foreach (var userRole in game.User.Roles)
            {
                userRole.Role = context
                    .Roles
                    .FirstOrDefault(r => r.Id == userRole.RoleId);
            }

            // Act
            var result = await sut.DeleteAsync(game);

            // Arrange
            Assert.That(result.IsSuccess, Is.True);

        }

        [Test, Category("Repository")]
        public async Task ReturnsFalseIfDeleteGamesFails()
        {
            // Arrange

            // Act
            var result = await sut.DeleteAsync(newGame);

            // Arrange
            Assert.That(result.IsSuccess, Is.False);

        }

        [Test, Category("Repository")]
        public async Task ConfirmItHasAGame()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfConfirmItHasAGameFails()
        {
            // Arrange
            var id = context
                .Games
                .ToList()
                .OrderBy(g => g.Id)
                .Last<Game>()
                .Id + 1;

            // Act
            var result = await sut.HasEntityAsync(id);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetUserGamesById()
        {
            // Arrange

            // Act
            var result = await sut.GetMyGameAsync(1, 1, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Game)result.Object, Is.InstanceOf<Game>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetUserGamesByIdFails()
        {
            // Arrange

            // Act
            var result = await sut.GetMyGameAsync(1, 3, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetAllUserGames()
        {
            // Arrange

            // Act
            var result = await sut.GetMyGamesAsync(1, 1);

            // Arrange
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Objects.ConvertAll(g => (Game)g), Is.InstanceOf<List<Game>>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetAllUserGamesFails()
        {
            // Arrange

            // Act
            var result = await sut.GetMyGamesAsync(1, 3);

            // Arrange
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task DeleteUserGamesById()
        {
            // Arrange

            // Act
            var result = await sut.DeleteMyGameAsync(1, 1, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfDeleteUserGamesByIdFails()
        {
            // Arrange

            // Act
            var result = await sut.DeleteMyGameAsync(1, 3, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetGameByApp()
        {
            // Arrange

            // Act
            var result = await sut.GetAppGameAsync(1, 1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Game)result.Object, Is.InstanceOf<Game>());
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetGameByAppFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAppGameAsync(1, 5);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Repository")]
        public async Task GetGamesByApp()
        {
            // Arrange

            // Act
            var result = await sut.GetAppGamesAsync(1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Repository")]
        public async Task ReturnFalseIfGetGamesByAppFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAppGamesAsync(5);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }
    }
}
