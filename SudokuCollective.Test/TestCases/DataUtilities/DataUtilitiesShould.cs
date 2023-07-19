using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Services;
using SudokuCollective.Data.Utilities;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class DataUtilitiesShould
    {
        private DatabaseContext context;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();
        }
        
        [Test, Category("Utilities")]
        public async Task AttachSudokuCells()
        {
            // Arrange
            var matrix = context.SudokuMatrices.FirstOrDefault(m => m.Id == 1);

            // Act
            await matrix.AttachSudokuCells(context);

            // Assert
            Assert.That(matrix.SudokuCells, Is.TypeOf<List<SudokuCell>>());
            Assert.That(matrix.SudokuCells.Count(), Is.EqualTo(81));
        }
        
        [Test, Category("Utilities")]
        public async Task ConfirmIfGameIsActive()
        {
            // Arrange
            var game = context.Games.FirstOrDefault(m => m.Id == 1);

            // Act
            var result = await game.IsGameInActiveApp(context);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test, Category("Utilities")]
        public void ConfirmIfPaginatorIsValid()
        {
            // Arrange
            var paginator = TestObjects.GetPaginator();
            var entities = context.Games.ToList().ConvertAll(g => (IDomainEntity)g).ToList();

            // Act
            var result = DataUtilities.IsPageValid(paginator, entities);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test, Category("Utilities")]
        public void ProcessExceptions()
        {
            // Arrange
            var requestService = new RequestService();
            var mockedLogger = new Mock<ILogger<AppsService>>();
            IResult result = new Result();
            result.IsSuccess = false;
            var exception = new Exception();

            try
            {
                // Act
                result = DataUtilities.ProcessException<AppsService>(requestService, mockedLogger.Object, result, exception);
                
                // Assert
                Assert.That(result.IsSuccess, Is.False);
            }
            catch
            {
                Assert.That(false);
            }
        }
    }
}
