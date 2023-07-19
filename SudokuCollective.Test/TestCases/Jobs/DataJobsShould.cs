using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Jobs;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Jobs
{
    public class DataJobsShould
    {
        private DatabaseContext context;
        private Mock<ILogger<DataJobs>> mockedLogger;
        private Game game;
        private DataJobs sut;

        [SetUp]
        public async Task Setup()
        {
            context = await TestDatabase.GetDatabaseContext();
            mockedLogger = new Mock<ILogger<DataJobs>>();
            game = new Game();
            game.SudokuMatrix.GenerateSolution();

            sut = new DataJobs(context, mockedLogger.Object);
        }
        
        [Test, Category("Jobs")]
        public async Task AddSolutions()
        {
            try
            {
                // Arrange and Assert
                await sut.AddSolutionJobAsync(game.SudokuMatrix.ToIntList());

                Assert.That(true);
            }
            catch
            {
                Assert.That(false);
            }
        }
        
        [Test, Category("Jobs")]
        public async Task GenerateSolutions()
        {
            try
            {
                // Arrange and Assert
                await sut.GenerateSolutionsJobAsync(1000);

                Assert.That(true);
            }
            catch
            {
                Assert.That(false);
            }
        }
    }
}