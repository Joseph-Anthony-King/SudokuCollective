using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class SudokuMatrixUtilitiesShould
    {
        DatabaseContext context;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();
        }

        [Test, Category("Utilities")]
        public void SolveSudokuMatrices()
        {
            // Arrange
            var sudokuMatrix = context.SudokuMatrices
                .FirstOrDefault(m => m.Id == 1);

            // Act
            var result = SudokuMatrixUtilities.SolveByElimination(
                sudokuMatrix, 
                sudokuMatrix.ToIntList());

            // Assert
            Assert.That(result.Count(), Is.EqualTo(81));
            Assert.That(result.Contains(0), Is.False);
        }
    }
}
