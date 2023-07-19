using System;
using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Payloads
{
    public class GamePayloadShould
    {
        private IGamePayload sut;

        [SetUp]
        public void Setup()
        {
            sut = new GamePayload();
        }

        [Test, Category("Payloads")]
        public void HasRequiredProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.SudokuCells, Is.InstanceOf<List<SudokuCell>>());
        }

        [Test, Category("Payloads")]
        public void AcceptsValidSudokuCells()
        {
            // Arrange and Act
            sut.SudokuCells = TestObjects.GetValidSudokuCells();

            // Assert
            Assert.That(sut.SudokuCells, Is.InstanceOf<List<SudokuCell>>());
            Assert.That(sut.SudokuCells.Count, Is.EqualTo(81));
        }

        [Test, Category("Payloads")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new GamePayload();

            // Assert
            Assert.That(sut, Is.InstanceOf<GamePayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsISudokuCellArray()
        {
            // Arrange
            var sudokuCellArray = TestObjects
                .GetValidSudokuCells()
                .ToArray();

            // Act
            sut = new GamePayload(sudokuCellArray);

            // Assert
            Assert.That(sut, Is.InstanceOf<GamePayload>());
        }

        [Test, Category("Payloads")]
        public void HasAConstructorThatAcceptsISudokuCellList()
        {
            // Arrange
            var sudokuCellList = TestObjects
                .GetValidSudokuCells();

            // Act
            sut = new GamePayload(sudokuCellList);

            // Assert
            Assert.That(sut, Is.InstanceOf<GamePayload>());
        }

        [Test, Category("Payloads")]
        public void ThrowsAnExceptionIfSudokuCellsAreInvalid()
        {
            // Arrange
            var sudokuCellList = TestObjects
                .GetValidSudokuCells();

            sudokuCellList.Add(new SudokuCell());

            // Act and Assert
            Assert.Throws<ArgumentException>(() => 
                new GamePayload(sudokuCellList));
        }
    }
}
