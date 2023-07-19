using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class AnnonymousGameResultShould
    {
        private IAnnonymousGameResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new AnnonymousGameResult();
        }

        [Test, Category("Results")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SudokuMatrix, Is.InstanceOf<object>());
        }

        [Test, Category("Results")]
        public void HaveADefaultConstructor()
        {
            // Arrange and Act
            sut = new AnnonymousGameResult();

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsJaggedArrayOfInts()
        {
            // Arrange
            var sudokuArray = new int[9][];

            for (var i = 0; i < sudokuArray.Length; i++)
            {
                sudokuArray[i] = new int[9];
            }

            // Act
            sut = new AnnonymousGameResult(sudokuArray);

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameResult>());
        }

        [Test, Category("Results")]
        public void HaveAConstructorThatAcceptsEnums()
        {
            // Arrange
            var sudokuList = new List<List<int>>();

            // Act
            sut = new AnnonymousGameResult(sudokuList);

            // Assert
            Assert.That(sut, Is.InstanceOf<AnnonymousGameResult>());
        }
    }
}
